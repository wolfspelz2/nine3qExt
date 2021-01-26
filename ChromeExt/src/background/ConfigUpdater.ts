import log = require('loglevel');
import * as $ from 'jquery';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Memory } from '../lib/Memory';
import { Client } from '../lib/Client';
import { Utils } from '../lib/Utils';
import { BackgroundApp } from './BackgroundApp';

interface ConfigUpdaterCallabck { (): void }

export class ConfigUpdater
{
    private gotConfig = false;

    constructor(private app: BackgroundApp)
    {
    }

    private updateCheckIntervalSec: number = Config.get('config.checkUpdateIntervalSec', 61);
    private updateCheckTimer: number = null;
    async startUpdateTimer(onUpdate: () => void)
    {
        if (this.updateCheckTimer == null) {
            this.updateCheckTimer = <number><unknown>setTimeout(async () =>
            {
                this.updateCheckTimer = null;
                await this.checkUpdate(onUpdate);
                await this.startUpdateTimer(onUpdate);
            }, this.updateCheckIntervalSec * 1000);
        }
    }

    stopUpdateTimer(): void
    {
        if (this.updateCheckTimer) {
            clearTimeout(this.updateCheckTimer);
            this.updateCheckTimer = null;
        }
    }

    async checkUpdate(onUpdate: ConfigUpdaterCallabck)
    {
        try {
            let lastUpdateConfigTime = as.Int(Memory.getSession('config.lastUpdateTime', 0), 0);
            let intervalSec = as.Int(Config.get('config.updateIntervalSec', 86331));
            let secsSinceUpdate = Date.now() / 1000 - lastUpdateConfigTime;
            if (secsSinceUpdate > intervalSec) {
                await this.getUpdate(onUpdate)
            }
        } catch (error) {
            log.info(ConfigUpdater.name, this.checkUpdate.name, error);
        }
    }

    async getUpdate(onUpdate: ConfigUpdaterCallabck)
    {
        let configUrl = Config.get('config.serviceUrl', 'https://webex.vulcan.weblin.com/Config');
        try {
            let data = await this.fetchJson(configUrl);
            Config.setOnlineTree(data);
            this.gotConfig = true;
            Memory.setSession('config.lastUpdateTime', Date.now() / 1000);
        } catch (error) {
            log.info(ConfigUpdater.name, this.getUpdate.name, 'fetchConfig failed', configUrl, error)
        }

        if (this.gotConfig) {

            let itemProviders = Config.get('itemProviders', {});
            if (itemProviders) {
                for (let providerId in itemProviders) {
                    let itemProvider = itemProviders[providerId];
                    if (itemProvider.configUrl) {
                        let userId = await this.getOrCreateItemProviderUserId(providerId);
                        let providerConfigUrl = as.String(itemProvider.configUrl, 'https://webit.vulcan.weblin.com/Config?id={id}&client={client}');
                        providerConfigUrl = providerConfigUrl
                            .replace('{id}', encodeURIComponent(userId))
                            .replace('{client}', encodeURIComponent(Client.getDetails()))
                            ;
                        try {
                            var providerConfig = await this.fetchJson(providerConfigUrl);
                            Config.setOnline('itemProviders.' + providerId + '.config', providerConfig);
                        } catch (error) {
                            log.info(ConfigUpdater.name, this.getUpdate.name, 'Fetch itemProvider config failed', providerId, providerConfigUrl, error);
                        }
                    }
                }
            }

        }

        if (this.gotConfig) {
            if (onUpdate) { onUpdate(); }
        }
    }

    private async fetchJson(url: string): Promise<any>
    {
        log.debug('ConfigUpdater.fetchConfig', url);

        return new Promise((resolve, reject) =>
        {
            $
                .getJSON(url, data => resolve(data))
                .fail(reason => reject(null));
        });
    }

    async getOrCreateItemProviderUserId(providerId: string): Promise<string>
    {
        let userId = await Memory.getSync(Utils.syncStorageKey_ItemProviderUserId(providerId), '');
        if (userId == '') {
            userId = 'ext' + Utils.randomString(40).toLowerCase();
            await Memory.setSync(Utils.syncStorageKey_ItemProviderUserId(providerId), userId);
        }
        return userId;
    }
}
