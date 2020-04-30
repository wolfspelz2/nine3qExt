import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';

export class ConfigUpdater
{
    private timer: number = undefined;

    public async startUpdateTimer()
    {
        this.timer = <number><unknown>setTimeout(async () =>
        {
            this.timer = undefined;
            this.startUpdateTimer();
            this.checkUpdate();
        }, Config.get('checkUpdateConfigIntervalSec', 600) * 1000);
    }

    public stopUpdateTimer(): void
    {
        if (this.timer != undefined) {
            window.clearTimeout(this.timer);
        }
    }

    public async checkUpdate()
    {
        let lastUpdateConfigTime: number = as.Int(await Config.getLocal('lastUpdateConfigTime', 0), 0);
        if (Date.now() - lastUpdateConfigTime > as.Int(Config.get('updateConfigIntervalSec', 86331))) {
            await this.getUpdate()
        }
    }

    public async getUpdate()
    {
        try {
            let data = await this.fetchConfig();
            Config.setAllOnline(data);
            await Config.setLocal('lastUpdateConfigTime', Date.now());
        } catch (error) {
            log.warn('ConfigUpdater.checkUpdate', 'fetchConfig failed')
        }
    }

    private async fetchConfig(): Promise<any>
    {
        let url = Config.get('configSeviceUrl', 'https://config.weblin.sui.li/');
        log.info('ConfigUpdater.fetchConfig', url);

        return new Promise((resolve, reject) =>
        {
            $
                .getJSON(url, data => resolve(data))
                .fail(reason => reject(null));
        });
    }
}