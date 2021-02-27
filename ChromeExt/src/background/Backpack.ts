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
import { Utils } from '../lib/Utils';
import { BackgroundApp } from './BackgroundApp';
import { Item } from './Item';
const Web3 = require('web3');
import { AbiItem } from 'web3-utils';
import { Environment } from '../lib/Environment';

export class Backpack
{
    private static BackpackIdsKey = 'BackpackIds';
    private static BackpackPropsPrefix = 'BackpackItem-';
    private items: { [id: string]: Item; } = {};
    private rooms: { [jid: string]: Array<string>; } = {};
    private rpcClient: RpcClient = new RpcClient();

    getBackpackIdsKey(): string
    {
        if (Config.get('config.clusterName', 'prod') == 'dev') {
            return Backpack.BackpackIdsKey + '-dev';
        }
        return Backpack.BackpackIdsKey;
    }

    constructor(private app: BackgroundApp, rpcClient: RpcClient = null)
    {
        if (rpcClient) { this.rpcClient = rpcClient; }
    }

    async init(): Promise<void>
    {
        await this.loadLocalItems();
        await this.loadWeb3Items();
    }

    async loadLocalItems()
    {
        let itemIds = await Memory.getLocal(this.getBackpackIdsKey(), []);
        if (itemIds == null || !Array.isArray(itemIds)) {
            log.warn('Local storage', this.getBackpackIdsKey(), 'not an array');
            return;
        }

        for (let i = 0; i < itemIds.length; i++) {
            let itemId = itemIds[i];

            let props = await Memory.getLocal(Backpack.BackpackPropsPrefix + itemId, null);
            if (props == null || typeof props != 'object') {
                log.warn('Local storage', Backpack.BackpackPropsPrefix + itemId, 'not an object, skipping');
                continue;
            }

            let item = await this.createRepositoryItem(itemId, props);
            if (item.isRezzed()) {
                let roomJid = item.getProperties()[Pid.RezzedLocation];
                if (roomJid) {
                    this.addToRoom(itemId, roomJid);
                }
            }
        }
    }

    async loadWeb3Items(): Promise<void>
    {
        const web3 = new Web3(new Web3.providers.HttpProvider('https://kovan.infura.io/v3/8f39aa5fb9fb402e8e65a9c810e6cdb1'));
        const abi = <Array<AbiItem>>[
            {
                'inputs': [],
                'stateMutability': 'nonpayable',
                'type': 'constructor'
            },
            {
                'anonymous': false,
                'inputs': [
                    {
                        'indexed': true,
                        'internalType': 'address',
                        'name': '_owner',
                        'type': 'address'
                    },
                    {
                        'indexed': true,
                        'internalType': 'address',
                        'name': '_approved',
                        'type': 'address'
                    },
                    {
                        'indexed': true,
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    }
                ],
                'name': 'Approval',
                'type': 'event'
            },
            {
                'anonymous': false,
                'inputs': [
                    {
                        'indexed': true,
                        'internalType': 'address',
                        'name': '_owner',
                        'type': 'address'
                    },
                    {
                        'indexed': true,
                        'internalType': 'address',
                        'name': '_operator',
                        'type': 'address'
                    },
                    {
                        'indexed': false,
                        'internalType': 'bool',
                        'name': '_approved',
                        'type': 'bool'
                    }
                ],
                'name': 'ApprovalForAll',
                'type': 'event'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_approved',
                        'type': 'address'
                    },
                    {
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    }
                ],
                'name': 'approve',
                'outputs': [],
                'stateMutability': 'nonpayable',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_to',
                        'type': 'address'
                    },
                    {
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    },
                    {
                        'internalType': 'string',
                        'name': '_data',
                        'type': 'string'
                    }
                ],
                'name': 'mint',
                'outputs': [],
                'stateMutability': 'nonpayable',
                'type': 'function'
            },
            {
                'anonymous': false,
                'inputs': [
                    {
                        'indexed': true,
                        'internalType': 'address',
                        'name': 'previousOwner',
                        'type': 'address'
                    },
                    {
                        'indexed': true,
                        'internalType': 'address',
                        'name': 'newOwner',
                        'type': 'address'
                    }
                ],
                'name': 'OwnershipTransferred',
                'type': 'event'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_from',
                        'type': 'address'
                    },
                    {
                        'internalType': 'address',
                        'name': '_to',
                        'type': 'address'
                    },
                    {
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    }
                ],
                'name': 'safeTransferFrom',
                'outputs': [],
                'stateMutability': 'nonpayable',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_from',
                        'type': 'address'
                    },
                    {
                        'internalType': 'address',
                        'name': '_to',
                        'type': 'address'
                    },
                    {
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    },
                    {
                        'internalType': 'bytes',
                        'name': '_data',
                        'type': 'bytes'
                    }
                ],
                'name': 'safeTransferFrom',
                'outputs': [],
                'stateMutability': 'nonpayable',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_operator',
                        'type': 'address'
                    },
                    {
                        'internalType': 'bool',
                        'name': '_approved',
                        'type': 'bool'
                    }
                ],
                'name': 'setApprovalForAll',
                'outputs': [],
                'stateMutability': 'nonpayable',
                'type': 'function'
            },
            {
                'anonymous': false,
                'inputs': [
                    {
                        'indexed': true,
                        'internalType': 'address',
                        'name': '_from',
                        'type': 'address'
                    },
                    {
                        'indexed': true,
                        'internalType': 'address',
                        'name': '_to',
                        'type': 'address'
                    },
                    {
                        'indexed': true,
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    }
                ],
                'name': 'Transfer',
                'type': 'event'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_from',
                        'type': 'address'
                    },
                    {
                        'internalType': 'address',
                        'name': '_to',
                        'type': 'address'
                    },
                    {
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    }
                ],
                'name': 'transferFrom',
                'outputs': [],
                'stateMutability': 'nonpayable',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_newOwner',
                        'type': 'address'
                    }
                ],
                'name': 'transferOwnership',
                'outputs': [],
                'stateMutability': 'nonpayable',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_owner',
                        'type': 'address'
                    }
                ],
                'name': 'balanceOf',
                'outputs': [
                    {
                        'internalType': 'uint256',
                        'name': '',
                        'type': 'uint256'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [],
                'name': 'CANNOT_TRANSFER_TO_ZERO_ADDRESS',
                'outputs': [
                    {
                        'internalType': 'string',
                        'name': '',
                        'type': 'string'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    }
                ],
                'name': 'getApproved',
                'outputs': [
                    {
                        'internalType': 'address',
                        'name': '',
                        'type': 'address'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    }
                ],
                'name': 'getTokenData',
                'outputs': [
                    {
                        'internalType': 'string',
                        'name': '',
                        'type': 'string'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_owner',
                        'type': 'address'
                    },
                    {
                        'internalType': 'uint64',
                        'name': '_index',
                        'type': 'uint64'
                    }
                ],
                'name': 'getTokenIdByOwnerAndIndex',
                'outputs': [
                    {
                        'internalType': 'uint256',
                        'name': '',
                        'type': 'uint256'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'address',
                        'name': '_owner',
                        'type': 'address'
                    },
                    {
                        'internalType': 'address',
                        'name': '_operator',
                        'type': 'address'
                    }
                ],
                'name': 'isApprovedForAll',
                'outputs': [
                    {
                        'internalType': 'bool',
                        'name': '',
                        'type': 'bool'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [],
                'name': 'name',
                'outputs': [
                    {
                        'internalType': 'string',
                        'name': '_name',
                        'type': 'string'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [],
                'name': 'NOT_CURRENT_OWNER',
                'outputs': [
                    {
                        'internalType': 'string',
                        'name': '',
                        'type': 'string'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [],
                'name': 'owner',
                'outputs': [
                    {
                        'internalType': 'address',
                        'name': '',
                        'type': 'address'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'uint256',
                        'name': '_tokenId',
                        'type': 'uint256'
                    }
                ],
                'name': 'ownerOf',
                'outputs': [
                    {
                        'internalType': 'address',
                        'name': '_owner',
                        'type': 'address'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [
                    {
                        'internalType': 'bytes4',
                        'name': '_interfaceID',
                        'type': 'bytes4'
                    }
                ],
                'name': 'supportsInterface',
                'outputs': [
                    {
                        'internalType': 'bool',
                        'name': '',
                        'type': 'bool'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [],
                'name': 'symbol',
                'outputs': [
                    {
                        'internalType': 'string',
                        'name': '_symbol',
                        'type': 'string'
                    }
                ],
                'stateMutability': 'view',
                'type': 'function'
            }
        ];
        const ownerAddress = '0xFE3aFc544D6098379061a8833c175E603c267fa4';
        const contractAddress = '0x637f0918F39E4e82fa66512318096Dd1bab49177';
        const contract = new web3.eth.Contract(abi, contractAddress);
        let numberOfItems = await contract.methods.balanceOf(ownerAddress).call();
        for (let i = 0; i < numberOfItems; i++) {
            let tokenId = await contract.methods.getTokenIdByOwnerAndIndex(ownerAddress, i).call();
            let tokenData = await contract.methods.getTokenData(tokenId).call();
            let existingItems = this.findItems(props => as.Bool(props[Pid.ClaimAspect], false) && as.String(props[Pid.ClaimName], '') == tokenData);
            if (existingItems.length == 0) {
                let item = await this.createItemByTemplate('PirateFlag', { [Pid.ClaimName]: tokenData });
            }
        }
    }

    async getOrCreatePointsItem(): Promise<Item>
    {
        let pointsItems = this.findItems(props => as.Bool(props[Pid.PointsAspect], false));

        if (pointsItems.length > 1) {
            log.debug('Backpack.getOrCreatePointsItem', 'Too many points items: ' + pointsItems.length);
        } else if (pointsItems.length == 0) {
            let item = await this.createItemByTemplate('Points', {});
            return item;
        } else if (pointsItems.length == 1) {
            return pointsItems[0]
        }
    }

    async addItem(itemId: string, props: ItemProperties, options: ItemChangeOptions): Promise<void>
    {
        let item = await this.createRepositoryItem(itemId, props);

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
            this.app.sendToAllTabs(ContentMessage.type_onBackpackShowItem, data);
        }
    }

    async deleteItem(itemId: string, options: ItemChangeOptions): Promise<void>
    {
        let item = this.items[itemId];
        if (item) {
            if (item.isRezzed()) {
                let roomJid = item.getProperties()[Pid.RezzedLocation];
                if (roomJid) {
                    await this.derezItem(itemId, roomJid, -1, -1, options);
                }
            }

            if (!options.skipPersistentStorage) {
                await this.persistentDeleteItem(itemId);
            }

            if (!options.skipContentNotification) {
                let data = new BackpackRemoveItemData(itemId);
                this.app.sendToAllTabs(ContentMessage.type_onBackpackHideItem, data);
            }

            this.deleteRepositoryItem(itemId);
        }
    }

    findItems(filter: (props: ItemProperties) => boolean): Array<Item>
    {
        let found: Array<Item> = [];

        for (let itemId in this.items) {
            let item = this.items[itemId];
            if (item) {
                if (filter(item.getProperties())) {
                    found.push(item);
                }
            }
        }

        return found;
    }

    private async createRepositoryItem(itemId: string, props: ItemProperties): Promise<Item>
    {
        if (props[Pid.OwnerId] == null) {
            props[Pid.OwnerId] = await Memory.getSync(Utils.syncStorageKey_Id(), '');
        }

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
            let itemIds = await Memory.getLocal(this.getBackpackIdsKey(), []);
            if (itemIds && Array.isArray(itemIds)) {
                await Memory.setLocal(Backpack.BackpackPropsPrefix + itemId, props);
                if (!itemIds.includes(itemId)) {
                    itemIds.push(itemId);
                    await Memory.setLocal(this.getBackpackIdsKey(), itemIds);
                }
            }
        }
    }

    private async persistentDeleteItem(itemId: string): Promise<void>
    {
        let itemIds = await Memory.getLocal(this.getBackpackIdsKey(), []);
        if (itemIds && Array.isArray(itemIds)) {
            await Memory.deleteLocal(Backpack.BackpackPropsPrefix + itemId);
            if (itemIds.includes(itemId)) {
                const index = itemIds.indexOf(itemId, 0);
                if (index > -1) {
                    itemIds.splice(index, 1);
                    await Memory.setLocal(this.getBackpackIdsKey(), itemIds);
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

    createItemByTemplate(templateName: string, args: ItemProperties): Promise<Item>
    {
        return new Promise(async (resolve, reject) =>
        {
            try {

                let userId = await Memory.getSync(Utils.syncStorageKey_Id(), '');
                if (userId == null || userId == '') { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.NoUserId); }

                let providerId = 'nine3q';
                let apiUrl = Config.get('itemProviders.' + providerId + '.config.backpackApiUrl', '');
                if (apiUrl == null || apiUrl == '') { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.SeeDetail, 'Missing backpackApi for ' + providerId); }

                let request = new RpcProtocol.BackpackCreateRequest();
                request.method = RpcProtocol.BackpackCreateRequest.method;
                request.user = userId;
                request.template = templateName;
                request.args = args;

                let response = <RpcProtocol.BackpackCreateResponse>await this.rpcClient.call(apiUrl, request);

                let props = response.properties;
                let itemId = props.Id;
                await this.addItem(itemId, props, {});
                let item = this.items[itemId];

                resolve(item);
            } catch (error) {
                reject(error);
            }
        });
    }

    executeItemAction(itemId: string, action: string, args: any, involvedIds: Array<string>, allowUnrezzed: boolean): Promise<void>
    {
        return new Promise(async (resolve, reject) =>
        {
            try {

                let item = this.items[itemId];
                if (item == null) { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.ItemDoesNotExist, itemId); }

                let userId = await Memory.getSync(Utils.syncStorageKey_Id(), '');
                if (userId == null || userId == '') { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.NoUserId); }

                let providerId = 'nine3q';
                let apiUrl = Config.get('itemProviders.' + providerId + '.config.backpackApiUrl', '');
                if (apiUrl == null || apiUrl == '') { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.SeeDetail, 'Missing backpackApi for ' + providerId); }

                let roomJid = null;
                if (!allowUnrezzed) {
                    roomJid = item.getProperties()[Pid.RezzedLocation];
                    if (roomJid == null || roomJid == '') { throw new ItemException(ItemException.Fact.NotExecuted, ItemException.Reason.SeeDetail, 'Item ' + itemId + ' missing RezzedLocation'); }
                }

                let items: { [id: string]: ItemProperties } = {};
                for (let i = 0; i < involvedIds.length; i++) {
                    items[involvedIds[i]] = this.getItemProperties(involvedIds[i]);
                }

                let request = new RpcProtocol.BackpackActionRequest();
                request.method = RpcProtocol.BackpackActionRequest.method;
                request.user = userId;
                request.item = itemId;
                if (roomJid) { request.room = roomJid; }
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
        if (rezzedX >= 0) {
            props[Pid.RezzedX] = '' + rezzedX;
        }
        if (as.Int(props[Pid.RezzedX], -1) < 0) {
            props[Pid.RezzedX] = '' + Utils.randomInt(100, 400);
        }
        props[Pid.RezzedDestination] = destinationUrl;
        props[Pid.RezzedLocation] = roomJid;
        props[Pid.OwnerName] = await Memory.getSync(Utils.syncStorageKey_Nickname(), as.String(props[Pid.OwnerName]));
        item.setProperties(props, options);

        if (!options.skipPersistentStorage) {
            await this.persistentSaveItem(itemId);
        }
    }

    async derezItem(itemId: string, roomJid: string, inventoryX: number, inventoryY: number, options: ItemChangeOptions): Promise<void>
    {
        let item = this.items[itemId];
        if (item == null) { throw new ItemException(ItemException.Fact.NotDerezzed, ItemException.Reason.ItemDoesNotExist, itemId); }
        if (!item.isRezzed()) { return; }
        if (!item.isRezzedTo(roomJid)) { throw new ItemException(ItemException.Fact.NotDerezzed, ItemException.Reason.ItemNotRezzedHere); }

        this.removeFromRoom(itemId, roomJid);

        let props = item.getProperties();
        delete props[Pid.IsRezzed];
        if (inventoryX > 0 && inventoryY > 0) {
            props[Pid.InventoryX] = '' + inventoryX;
            props[Pid.InventoryY] = '' + inventoryY;
        }
        // delete props[Pid.RezzedX]; // preserve for rez by button
        delete props[Pid.RezzedDestination];
        delete props[Pid.RezzedLocation];

        options.skipPresenceUpdate = true;
        item.setProperties(props, options);

        if (!options.skipPersistentStorage) {
            await this.persistentSaveItem(itemId);
        }

        if (!options.skipContentNotification) {
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
