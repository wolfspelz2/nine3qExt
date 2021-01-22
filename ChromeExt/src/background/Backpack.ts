import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { ItemProperties, Pid } from '../lib/ItemProperties';
import { BackpackShowItemData, BackpackRemoveItemData, BackpackSetItemData, ContentMessage } from '../lib/ContentMessage';
import { ItemException } from '../lib/ItemExcption';
import { ItemChangeOptions } from '../lib/ItemChangeOptions';
import { RpcProtocol } from '../lib/RpcProtocol';
import { RpcClient } from '../lib/RpcClient';
import { Memory } from '../lib/Memory';
import { BackgroundApp } from './BackgroundApp';
import { Item } from './Item';

export class Backpack
{
    private static BackpackIdsKey = 'BackpackIds';
    private static BackpackPropsPrefix = 'BackpackItem-';
    private items: { [id: string]: Item; } = {};
    private rooms: { [jid: string]: Array<string>; } = {};
    private rpcClient: RpcClient = new RpcClient();

    constructor(private app: BackgroundApp, rpcClient: RpcClient = null)
    {
        if (rpcClient) { this.rpcClient = rpcClient; }
    }

    async init(): Promise<void>
    {
        let itemIds = await Memory.getLocal(Backpack.BackpackIdsKey, []);
        if (itemIds == null || !Array.isArray(itemIds)) {
            log.warn('Local storage', Backpack.BackpackIdsKey, 'not an array');
            return;
        }

        for (let i = 0; i < itemIds.length; i++) {
            let itemId = itemIds[i];

            let props = await Memory.getLocal(Backpack.BackpackPropsPrefix + itemId, null);
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

    async addItem(itemId: string, props: ItemProperties, options: ItemChangeOptions)
    {
        let item = this.createRepositoryItem(itemId, props);

        if (item.isRezzed()) {
            if (!options.skipPresenceUpdate) {
                item.sendPresence();
            }
        }

        if (!options.skipPersistentStorage) {
            await this.persistentSaveItem(itemId);
        }

        if (!options.skipContentNotification) {
            let data = new BackpackShowItemData(itemId, props);
            this.app.sendToAllTabs(ContentMessage.Type[ContentMessage.Type.onBackpackShowItem], data);
        }
    }

    async deleteItem(itemId: string, options: ItemChangeOptions)
    {
        let item = this.items[itemId];
        if (item) {
            if (item.isRezzed()) {
                let roomJid = item.getProperties()[Pid.RezzedLocation];
                await this.derezItem(itemId, roomJid, -1, -1, options);
            }

            if (!options.skipPersistentStorage) {
                await this.persistentDeleteItem(itemId);
            }

            if (!options.skipContentNotification) {
                let data = new BackpackRemoveItemData(itemId);
                this.app.sendToAllTabs(ContentMessage.Type[ContentMessage.Type.onBackpackHideItem], data);
            }

            this.deleteRepositoryItem(itemId);
        }
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

    private deleteRepositoryItem(itemId: string): void
    {
        if (this.items[itemId]) {
            delete this.items[itemId];
        }
    }

    private async persistentSaveItem(itemId: string): Promise<void>
    {
        let item = this.items[itemId];
        if (item) {
            let props = item.getProperties();
            let itemIds = await Memory.getLocal(Backpack.BackpackIdsKey, []);
            if (itemIds && Array.isArray(itemIds)) {
                await Memory.setLocal(Backpack.BackpackPropsPrefix + itemId, props);
                if (!itemIds.includes(itemId)) {
                    itemIds.push(itemId);
                    await Memory.setLocal(Backpack.BackpackIdsKey, itemIds);
                }
            }
        }
    }

    private async persistentDeleteItem(itemId: string): Promise<void>
    {
        let itemIds = await Memory.getLocal(Backpack.BackpackIdsKey, []);
        if (itemIds && Array.isArray(itemIds)) {
            await Memory.deleteLocal(Backpack.BackpackPropsPrefix + itemId);
            if (itemIds.includes(itemId)) {
                const index = itemIds.indexOf(itemId, 0);
                if (index > -1) {
                    itemIds.splice(index, 1);
                    await Memory.setLocal(Backpack.BackpackIdsKey, itemIds);
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
        if (item == null) { throw new ItemException(ItemException.Fact.Error, ItemException.Reason.ItemDoesNotExist, itemId); }

        item.setProperties(props, options);
        await this.persistentSaveItem(itemId);
    }

    getItemProperties(itemId: string): ItemProperties
    {
        let item = this.items[itemId];
        if (item == null) { throw new ItemException(ItemException.Fact.Error, ItemException.Reason.ItemDoesNotExist, itemId); }

        return item.getProperties();
    }

    async modifyItemProperties(itemId: string, changed: ItemProperties, deleted: Array<string>, options: ItemChangeOptions): Promise<void>
    {
        let item = this.items[itemId];
        if (item == null) { throw new ItemException(ItemException.Fact.Error, ItemException.Reason.ItemDoesNotExist, itemId); }

        let props = item.getProperties();
        for (let key in changed) {
            props[key] = changed[key];
        }
        for (let i = 0; i < deleted.length; i++) {
            delete props[deleted[i]];
        }
        item.setProperties(props, options);
        await this.persistentSaveItem(itemId);
    }

    executeItemAction(itemId: string, action: string, args: any, involvedIds: Array<string>): Promise<void>
    {
        return new Promise(async (resolve, reject) =>
        {
            try {

                let item = this.items[itemId];
                if (item == null) { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.ItemDoesNotExist, itemId); }

                let providerId = 'nine3q';

                let userToken = Config.get('itemProviders.' + providerId + '.config.userToken', '');
                if (userToken == null || userToken == '') { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.NoUserToken); }

                let apiUrl = Config.get('itemProviders.' + providerId + '.config.backpackApiUrl', '');
                if (apiUrl == null || apiUrl == '') { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.SeeDetail, 'Missing backpackApi for ' + providerId); }

                let roomJid = item.getProperties()[Pid.RezzedLocation];
                if (roomJid == null || roomJid == '') { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.SeeDetail, 'Item ' + itemId + ' missing RezzedLocation'); }

                let items: { [id: string]: ItemProperties } = {};
                for (let i = 0; i < involvedIds.length; i++) {
                    items[involvedIds[i]] = this.getItemProperties(involvedIds[i]);
                }

                let request = new RpcProtocol.BackpackActionRequest();
                request.method = RpcProtocol.BackpackActionRequest.method;
                request.user = userToken;
                request.item = itemId;
                request.room = roomJid;
                request.action = action;
                request.args = args;
                request.items = items;

                let response = <RpcProtocol.BackpackActionResponse>await this.rpcClient.call(apiUrl, request);

                if (response.changed) {
                    for (let id in response.changed) {
                        let props = response.changed[id];
                        await this.setItemProperties(id, props, {});
                    }
                }

                if (response.created) {
                    for (let id in response.created) {
                        let props = response.created[id];
                        await this.addItem(id, props, {});
                    }
                }

                if (response.deleted) {
                    for (let i = 0; i < response.deleted.length; i++) {
                        let id = response.deleted[i];
                        await this.deleteItem(id, {});
                    }
                }

                resolve();
            } catch (error) {
                reject(error);
            }
        });
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

    async rezItem(itemId: string, roomJid: string, rezzedX: number, destinationUrl: string, options: ItemChangeOptions): Promise<void>
    {
        let item = this.items[itemId];
        if (item == null) { throw new ItemException(ItemException.Fact.NotRezzed, ItemException.Reason.ItemDoesNotExist, itemId); }
        if (item.isRezzed()) { throw new ItemException(ItemException.Fact.NotRezzed, ItemException.Reason.ItemAlreadyRezzed); }

        this.addToRoom(itemId, roomJid);

        let props = item.getProperties();
        props[Pid.IsRezzed] = 'true';
        props[Pid.RezzedX] = '' + rezzedX;
        props[Pid.RezzedDestination] = destinationUrl;
        props[Pid.RezzedLocation] = roomJid;
        item.setProperties(props, options);

        if (!options.skipPersistentStorage) {
            await this.persistentSaveItem(itemId);
        }
    }

    async derezItem(itemId: string, roomJid: string, inventoryX: number, inventoryY: number, options: ItemChangeOptions): Promise<void>
    {
        let item = this.items[itemId];
        if (item == null) { throw new ItemException(ItemException.Fact.NotDerezzed, ItemException.Reason.ItemDoesNotExist, itemId); }
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

        options.skipPresenceUpdate = true;
        item.setProperties(props, options);

        if (!options.skipPersistentStorage) {
            await this.persistentSaveItem(itemId);
        }

        if (!options.skipContentNotification) {
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
