import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { ItemProperties } from '../lib/ItemProperties';
import { BackpackAddItemData, ContentMessage } from '../lib/ContentMessage';
import { Projector } from './Projector';
import { BackgroundApp } from './BackgroundApp';

export class BackpackItem
{
    private roomJid: string;

    constructor(private backpack: Backpack, private itemId: string, private properties: ItemProperties)
    {
    }

    getProperties(): ItemProperties { return this.properties; }

    setRezzed(roomJid: string)
    {
        this.roomJid = roomJid;
    }
}

export class Backpack
{
    private items: { [id: string]: BackpackItem; } = {};

    constructor(private app: BackgroundApp, private projector: Projector)
    {
    }

    addItem(itemId: string, props: ItemProperties)
    {
        var item = this.items[itemId];
        if (item == null) {
            item = new BackpackItem(this, itemId, props);
            this.items[itemId] = item;

            let message = new BackpackAddItemData(itemId, props);
            this.app.sendToAllTabs(ContentMessage.type_onBackpackAddItem, message);
        }
    }

    getItems(): { [id: string]: ItemProperties; }
    {
        let itemProperties: { [id: string]: ItemProperties; } = {};
        for (let id in this.items) {
            let item = this.items[id];
            itemProperties[id] = item.getProperties();
        }
        return itemProperties
    }

    rezItem(itemId: string, roomJid: string): void
    {
        var item = this.items[itemId];
        if (item) {
            this.projector.projectItem(itemId, roomJid, item.getProperties());
            this.items[itemId].setRezzed(roomJid);
        }
    }

}
