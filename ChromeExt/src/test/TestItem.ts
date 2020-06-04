import { expect } from 'chai';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { ContentApp } from '../contentscript/ContentApp';

export class TestItem
{
    itemProviderUrlFilter_replaces()
    {
        initConfig();

        expect(ContentApp.itemProviderUrlFilter('n3q', 'Icon32Url', '{item.nine3q}PageProxy/icon32.png')).to.equal('https://nine3q.dev.sui.li/images/Items/PageProxy/icon32.png');
    }
}

function initConfig()
{
    let tree = {
        config: {
            serviceUrl: 'https://config.weblin.sui.li/',
            updateIntervalSec: Utils.randomInt(60000, 80000),
            checkUpdateIntervalSec: 600,
        },
        httpCache: {
            maxAgeSec: 3600,
            maintenanceIntervalSec: 60,
        },
        itemProviders: {
            'n3q':
            {
                name: 'weblin Items',
                description: 'Things on web pages',
                configUrl: 'https://avatar.weblin.sui.li/item/config',
                config: {
                    itemPropertyUrlFilter: [
                        { key: '{item.nine3q}', value: 'https://nine3q.dev.sui.li/images/Items/' },
                    ]
                }
            }
        },

        _last: 0
    }

    Config.setStaticTree(tree);
}