import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';

export class ConfigUpdater
{
    private timer: number = undefined;

    async startUpdateTimer()
    {
        this.timer = <number><unknown>setTimeout(async () =>
        {
            this.timer = undefined;
            this.startUpdateTimer();
            this.checkUpdate();
        }, Config.get('config.checkUpdateIntervalSec', 600) * 1000);
    }

    stopUpdateTimer(): void
    {
        if (this.timer != undefined) {
            window.clearTimeout(this.timer);
        }
    }

    async checkUpdate()
    {
        let lastUpdateConfigTime: number = as.Int(await Config.getLocal('config.lastUpdateTime', 0), 0);
        if (Date.now() - lastUpdateConfigTime > as.Int(Config.get('config.updateIntervalSec', 86331))) {
            await this.getUpdate()
        }
    }

    async getUpdate()
    {
        try {
            let data = await this.fetchConfig();
            Config.setAllOnline(data);
            await Config.setLocal('config.lastUpdateTime', Date.now());
        } catch (error) {
            log.warn('ConfigUpdater.checkUpdate', 'fetchConfig failed')
        }
    }

    private async fetchConfig(): Promise<any>
    {
        let url = Config.get('config.seviceUrl', 'https://config.weblin.sui.li/');
        log.info('ConfigUpdater.fetchConfig', url);

        return new Promise((resolve, reject) =>
        {
            $
                .getJSON(url, data => resolve(data))
                .fail(reason => reject(null));
        });
    }
}