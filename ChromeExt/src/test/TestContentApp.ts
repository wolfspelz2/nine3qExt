import { expect } from 'chai';
import { ContentApp } from '../contentscript/ContentApp';
import { Config } from '../lib/Config';

export class TestContentApp
{
    getRoomJidFromLocationUrl()
    {
        const jid = ContentApp.getRoomJidFromLocationUrl('xmpp:d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
        expect(jid).to.equal('d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
    }

    itemProviderUrlFilter_replaces()
    {
        Config.setOnlineTree(
            {
                itemProviders: {
                    nine3q: {
                        name: 'weblin Items',
                        description: 'Things on web pages',
                        configUrl: 'https://webit.vulcan.weblin.com/Config?id={id}',
                        config: {
                            itemPropertyUrlFilter: {
                                '{image.item.nine3q}': 'http://localhost:5000/images/Items/',
                                '{iframe.item.nine3q}': 'http://localhost:5000/ItemFrame/'
                            }
                        }
                    }
                }
            });
        expect(ContentApp.itemProviderUrlFilter('nine3q', 'ImageUrl', '{image.item.nine3q}PageProxy/icon32.png')).to.equal('http://localhost:5000/images/Items/PageProxy/icon32.png');
    }
}
