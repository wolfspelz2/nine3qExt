import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { ItemProperties, Pid } from '../lib/ItemProperties';
import { BackpackShowItemData, BackpackRemoveItemData, BackpackSetItemData, ContentMessage } from '../lib/ContentMessage';
import { BackgroundApp } from './BackgroundApp';
import { Item } from './Item';
import { ItemException } from '../lib/ItemExcption';
import { ItemChangeOptions } from '../lib/ItemChangeOptions';

export class Backpack
{
    private static BackpackIdsKey = 'BackpackIds';
    private static BackpackPropsPrefix = 'BackpackItem-';
    private items: { [id: string]: Item; } = {};
    private rooms: { [jid: string]: Array<string>; } = {};

    constructor(private app: BackgroundApp)
    {
    }

    async init(): Promise<void>
    {
        let itemIds = await Config.getLocal(Backpack.BackpackIdsKey, []);
        if (itemIds == null || !Array.isArray(itemIds)) {
            log.warn('Local storage', Backpack.BackpackIdsKey, 'not an array');
            return;
        }

        for (let i = 0; i < itemIds.length; i++) {
            let itemId = itemIds[i];

            let props = await Config.getLocal(Backpack.BackpackPropsPrefix + itemId, null);
            if (props == null || typeof props != 'object') {
                log.warn('Local storage', Backpack.BackpackPropsPrefix + itemId, 'not an object, skipping');
                continue;
            }

            let item = this.createRepositoryItem(itemId, props);
            if (item.isRezzed()) {
                let roomJid = item.getProperties()[Pid.RezzedLocation];
                if (roomJid) {
                    this.addToRoom(itemId, roomJid);
                }
            }
        }
    }

    async addItem(itemId: string, props: ItemProperties)
    {
        let item = this.createRepositoryItem(itemId, props);
        await this.saveItem(itemId);

        let data = new BackpackShowItemData(itemId, props);
        this.app.sendToAllTabs(ContentMessage.Type[ContentMessage.Type.onBackpackShowItem], data);
    }

    private createRepositoryItem(itemId: string, props: ItemProperties): Item
    {
        let item = this.items[itemId];
        if (item == null) {
            item = new Item(this.app, this, itemId, props);
            this.items[itemId] = item;
        }
        return item;
    }

    private async saveItem(itemId: string): Promise<void>
    {
        let item = this.items[itemId];
        if (item) {
            let props = item.getProperties();
            let itemIds = await Config.getLocal(Backpack.BackpackIdsKey, []);
            if (itemIds && Array.isArray(itemIds)) {
                await Config.setLocal(Backpack.BackpackPropsPrefix + itemId, props);
                if (!itemIds.includes(itemId)) {
                    itemIds.push(itemId);
                    await Config.setLocal(Backpack.BackpackIdsKey, itemIds);
                }
            }
        }
    }

    private addToRoom(itemId: string, roomJid: string): void
    {
        let rezzedIds = this.rooms[roomJid];
        if (rezzedIds == null) {
            rezzedIds = new Array<string>();
            this.rooms[roomJid] = rezzedIds;
        }
        rezzedIds.push(itemId);
    }

    private removeFromRoom(itemId: string, roomJid: string): void
    {
        let rezzedIds = this.rooms[roomJid];
        if (rezzedIds) {
            const index = rezzedIds.indexOf(itemId, 0);
            if (index > -1) {
                rezzedIds.splice(index, 1);
                if (rezzedIds.length == 0) {
                    delete this.rooms[roomJid];
                }
            }
        }
    }

    isItem(itemId: string): boolean
    {
        let item = this.items[itemId];
        if (item) {
            return true;
        }
        return false;
    }

    async setItemProperties(itemId: string, props: ItemProperties, options: ItemChangeOptions): Promise<void>
    {
        let item = this.items[itemId];
        if (item) {
            item.setProperties(props, options);
            await this.saveItem(itemId);
        }
    }

    async modifyItemProperties(itemId: string, changed: ItemProperties, deleted: Array<string>, options: ItemChangeOptions): Promise<void>
    {
        let item = this.items[itemId];
        if (item) {
            let props = item.getProperties();
            for (let key in changed) {
                props[key] = changed[key];
            }
            for (let i = 0; i < deleted.length; i++) {
                delete props[deleted[i]];
            }
            item.setProperties(props, options);
            await this.saveItem(itemId);
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

    async rezItem(itemId: string, roomJid: string, rezzedX: number, destinationUrl: string): Promise<void>
    {
        let item = this.items[itemId];
        if (item) {
            if (item.isRezzed()) { throw new ItemException(ItemException.Fact.NotRezzed, ItemException.Reason.ItemAlreadyRezzed); }

            this.addToRoom(itemId, roomJid);

            let props = item.getProperties();
            props[Pid.IsRezzed] = 'true';
            props[Pid.RezzedX] = '' + rezzedX;
            props[Pid.RezzedDestination] = destinationUrl;
            props[Pid.RezzedLocation] = roomJid;
            item.setProperties(props, ItemChangeOptions.empty);
            await this.saveItem(itemId);
        }
    }

    async derezItem(itemId: string, roomJid: string, inventoryX: number, inventoryY: number): Promise<void>
    {
        let item = this.items[itemId];
        if (item) {
            if (!item.isRezzedTo(roomJid)) { throw new ItemException(ItemException.Fact.NotDerezzed, ItemException.Reason.ItemNotRezzedHere); }

            this.removeFromRoom(itemId, roomJid);

            let props = item.getProperties();
            delete props[Pid.IsRezzed];
            if (inventoryX > 0 && inventoryY > 0) {
                props[Pid.InventoryX] = '' + inventoryX;
                props[Pid.InventoryY] = '' + inventoryY;
            }
            delete props[Pid.RezzedX];
            delete props[Pid.RezzedDestination];
            delete props[Pid.RezzedLocation];
            item.setProperties(props, { skipPresenceUpdate: true });
            await this.saveItem(itemId);

            this.app.sendToTabsForRoom(roomJid, ContentMessage.Type[ContentMessage.Type.sendPresence]);
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

    private getDependentPresence(roomJid: string): xml
    {
        let result = xml('x', { 'xmlns': 'vp:dependent' });

        for (let id in this.items) {
            if (this.items[id].isRezzedTo(roomJid)) {
                let itemPresence: xml = this.items[id].getDependentPresence(roomJid);
                result.append(itemPresence);
            }
        }

        return result;
    }

}
