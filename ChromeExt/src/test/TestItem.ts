import { expect } from 'chai';
import { Utils } from '../lib/Utils';
import { xml } from '@xmpp/client';
import { Item } from '../contentscript/Item';
import { Config } from '../lib/Config';

export class TestItem
{
    itemServiceUrlFilter_replaces()
    {
        initConfig();

        expect(Item.itemServiceUrlFilter('n3q', 'Icon32Url', '{item.nine3q}PageProxy/icon32.png')).to.equal('https://nine3q.dev.sui.li/images/Items/PageProxy/icon32.png');
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
        itemServices: {
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