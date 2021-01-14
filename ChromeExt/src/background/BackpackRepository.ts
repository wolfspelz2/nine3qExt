import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { ItemProperties, Pid } from '../lib/ItemProperties';
import { BackpackShowItemData, BackpackRemoveItemData, BackpackSetItemData, ContentMessage } from '../lib/ContentMessage';
import { BackgroundApp } from './BackgroundApp';
import { BackpackItem } from './BackpackItem';

export class BackpackRepository
{
    private items: { [id: string]: BackpackItem; } = {};
    private rooms: { [jid: string]: Array<string>; } = {};

    constructor(private app: BackgroundApp)
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

    modifyItemProperties(itemId: string, changed: ItemProperties, deleted: Array<string>)
    {
        var item = this.items[itemId];
        if (item) {
            let props = item.getProperties();
            for (let key in changed) {
                props[key] = changed[key];
            }
            for (let i = 0; i < deleted.length; i++) {
                delete props[deleted[i]];
            }
            item.setProperties(props);
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
            var rezzedIds = this.rooms[roomJid];
            if (rezzedIds == null) {
                rezzedIds = new Array<string>();
                this.rooms[roomJid] = rezzedIds;
            }
            rezzedIds.push(itemId);

            let props = item.getProperties();
            props[Pid.IsRezzed] = 'true';
            props[Pid.RezzedX] = '' + rezzedX;
            props[Pid.RezzedDestination] = destinationUrl;
            props[Pid.RezzedLocation] = roomJid;
            item.setProperties(props);
        }
    }

    derezItem(itemId: string, roomJid: string, inventoryX: number, inventoryY: number): void
    {
        var item = this.items[itemId];
        if (item) {
            var rezzedIds = this.rooms[roomJid];
            if (rezzedIds) {
                const index = rezzedIds.indexOf(itemId, 0);
                if (index > -1) {
                    rezzedIds.splice(index, 1);
                    if (rezzedIds.length == 0) {
                        delete this.rooms[roomJid];
                    }
                }
            }

            let props = item.getProperties();
            delete props[Pid.IsRezzed];
            if (inventoryX > 0 && inventoryY > 0) {
                props[Pid.InventoryX] = '' + inventoryX;
                props[Pid.InventoryY] = '' + inventoryY;
            }
            delete props[Pid.RezzedX];
            delete props[Pid.RezzedDestination];
            delete props[Pid.RezzedLocation];
            item.setProperties(props);

            this.app.sendToTabsForRoom(roomJid, ContentMessage.type_sendPresence);
        }
    }

    stanzaOutFilter(stanza: xml): any
    {
        let toJid = new jid(stanza.attrs.to);
        let roomJid = toJid.bare().toString();
        let itemNick = toJid.getResource();

        if (stanza.name == 'presence') {
            if (as.String(stanza.attrs['type'], 'available') == 'available') {

                var rezzedIds = this.rooms[roomJid];
                if (rezzedIds && rezzedIds.length > 0) {
                    let dependentExtension = this.getDependentPresence(roomJid);
                    if (dependentExtension) {
                        stanza.append(dependentExtension);
                    }
                }

            }
        }

        // if (stanza.name == 'message') {
        //     if (as.String(stanza.attrs['type'], 'normal') == 'chat') {
        //         if (this.isItem(itemNick)) {
        //             stanza = null;
        //         }
        //     }
        // }

        return stanza;
    }

    getDependentPresence(roomJid: string): xml
    {
        var result = xml('x', { 'xmlns': 'vp:dependent' });

        for (let id in this.items) {
            if (this.items[id].isRezzed()) {
                let itemPresence: xml = this.items[id].getDependentPresence(roomJid);
                result.append(itemPresence);
            }
        }

        return result;
    }

}
