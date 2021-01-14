import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { ItemProperties } from '../lib/ItemProperties';
import { BackpackShowItemData, BackpackRemoveItemData, BackpackSetItemData, ContentMessage } from '../lib/ContentMessage';
import { Projector } from './Projector';
import { BackgroundApp } from './BackgroundApp';

export class BackpackItem
{
    constructor(private app: BackgroundApp, private backpack: Backpack, private itemId: string, private properties: ItemProperties)
    {
    }

    getProperties(): ItemProperties { return this.properties; }

    setProperties(props: ItemProperties)
    {
        this.properties = props;

        let data = new BackpackSetItemData(this.itemId, props);
        this.app.sendToAllTabs(ContentMessage.type_onBackpackSetItem, data);
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
            item = new BackpackItem(this.app, this, itemId, props);
            this.items[itemId] = item;

            let data = new BackpackShowItemData(itemId, props);
            this.app.sendToAllTabs(ContentMessage.type_onBackpackShowItem, data);
        }
    }

    isItem(itemId: string): boolean
    {
        var item = this.items[itemId];
        if (item) {
            return true;
        }
        return false;
    }

    setItemProperties(itemId: string, properties: ItemProperties)
    {
        var item = this.items[itemId];
        if (item) {
            item.setProperties(properties);
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

    rezItem(itemId: string, roomJid: string, rezzedX: number, destinationUrl: string): void
    {
        var item = this.items[itemId];
        if (item) {
            let props = item.getProperties();
            props.IsRezzed = 'true';
            props.RezzedX = '' + rezzedX;
            props.RezzedDestination = destinationUrl;
            props.RezzedLoation = roomJid;
            item.setProperties(props);

            this.projector.projectItem(roomJid, itemId, props);
        }
    }

    derezItem(itemId: string, roomJid: string, inventoryX: number, inventoryY: number): void
    {
        var item = this.items[itemId];
        if (item) {
            let props = item.getProperties();
            delete props.IsRezzed;
            if (inventoryX > 0 && inventoryY > 0) {
                props.InventoryX = '' + inventoryX;
                props.InventoryY = '' + inventoryY;
            }
            delete props.RezzedX;
            delete props.RezzedDestination;
            delete props.RezzedLoation;
            item.setProperties(props);

            this.projector.retractItem(roomJid, itemId);
        }
    }

}
