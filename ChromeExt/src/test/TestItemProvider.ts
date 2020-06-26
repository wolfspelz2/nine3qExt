import { expect } from 'chai';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { ItemProvider } from '../contentscript/ItemProvider';

export class TestItemProvider
{
    itemProviderUrlFilter_replaces()
    {
        let itemProvider = new ItemProvider({
            itemPropertyUrlFilter:
            {
                '{item.nine3q}': 'https://nine3q.dev.sui.li/images/Items/'
            }
        });

        expect(itemProvider.propertyUrlFilter('{item.nine3q}PageProxy/icon32.png')).to.equal('https://nine3q.dev.sui.li/images/Items/PageProxy/icon32.png');
    }
}