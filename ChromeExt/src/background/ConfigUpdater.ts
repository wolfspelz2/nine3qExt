import log = require('loglevel');
import * as $ from 'jquery';
import { as } from '../lib/as';
import { Config } from '../lib/Config';

export class ConfigUpdater
{
    async startUpdateTimer()
    {
    }

    stopUpdateTimer(): void
    {
    }

    async checkUpdate()
    {
        try {
            let lastUpdateConfigTime: number = as.Int(Config.get('config.lastUpdateTime', 0), 0);
            if (Date.now() - lastUpdateConfigTime > as.Int(Config.get('config.updateIntervalSec', 86331))) {
                await this.getUpdate()
            }
        } catch (error) {
            log.info(error);
        }
    }

    async getUpdate()
    {
        try {
            let data = await this.fetchConfig();
            Config.setOnlineTree(data);
            Config.set('config.lastUpdateTime', Date.now());
        } catch (error) {
            log.info('ConfigUpdater.checkUpdate', 'fetchConfig failed')
        }
    }

    private async fetchConfig(): Promise<any>
    {
        let url = Config.get('config.serviceUrl', 'https://config.weblin.sui.li/');
        log.info('ConfigUpdater.fetchConfig', url);

        return new Promise((resolve, reject) =>
        {
            $
                .getJSON(url, data => resolve(data))
                .fail(reason => reject(null));
        });
    }
}