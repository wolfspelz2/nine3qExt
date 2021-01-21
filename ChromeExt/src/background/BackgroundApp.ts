import log = require('loglevel');
import { client, xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { BackgroundErrorResponse, BackgroundItemExceptionResponse, BackgroundMessage, BackgroundResponse, BackgroundSuccessResponse, GetBackpackStateResponse, IsBackpackItemResponse } from '../lib/BackgroundMessage';
import { Client } from '../lib/Client';
import { ItemProperties } from '../lib/ItemProperties';
import { ContentMessage } from '../lib/ContentMessage';
import { ItemException } from '../lib/ItemExcption';
import { ItemChangeOptions } from '../lib/ItemChangeOptions';
import { Memory } from '../lib/Memory';
import { ConfigUpdater } from './ConfigUpdater';
import { Backpack } from './Backpack';

interface ILocationMapperResponse
{
    //    sMessage: string;
    sLocationURL: string;
}

export class BackgroundApp
{
    private xmpp: any;
    private xmppConnected = false;
    private configUpdater: ConfigUpdater;
    private resource: string;
    private isReady: boolean = false;
    private clientDetails: string = '"weblin.io"';

    private readonly stanzaQ: Array<xml> = [];
    private readonly roomJid2tabId: Map<string, Array<number>> = new Map<string, Array<number>>();
    private readonly fullJid2TabWhichSentUnavailable: Map<string, number> = new Map<string, number>();
    private readonly iqStanzaTabId: Map<string, number> = new Map<string, number>();
    private readonly httpCacheData: Map<string, string> = new Map<string, string>();
    private readonly httpCacheTime: Map<string, number> = new Map<string, number>();

    private backpack: Backpack;

    async start(): Promise<void>
    {
        this.isReady = false;

        {
            let devConfig = await Memory.getLocal('dev.config', '{}');
            try {
                let parsed = JSON.parse(devConfig);
                Config.setDevTree(parsed);
            } catch (error) {
                log.error('Parse dev config failed', error);
            }
        }

        {
            let uniqueId = await Memory.getSync('me.id', '');
            if (uniqueId == '') {
                await Memory.setSync('me.id', 'mid' + Utils.randomString(20).toLowerCase());
            }
        }

        if (chrome.runtime && chrome.runtime.onMessage) {
            chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
                return this.onRuntimeMessage(message, sender, sendResponse);
            });
        }

        this.configUpdater = new ConfigUpdater();
        await this.configUpdater.getUpdate();
        await this.configUpdater.startUpdateTimer()

        if (Config.get('backpack.enabled', false)) {
            this.backpack = new Backpack(this);
            await this.backpack.init();
        }

        {
            let itemProviders = Config.get('itemProviders', {});
            if (itemProviders) {
                for (let providerId in itemProviders) {
                    let itemProvider = itemProviders[providerId];
                    if (itemProvider.configUrl) {
                        try {

                            let userId = await this.getOrCreateItemProviderUserId(providerId);
                            let url = itemProvider.configUrl;
                            url = url
                                .replace('{id}', encodeURIComponent(userId))
                                .replace('{client}', encodeURIComponent(Client.getDetails()))
                                ;

                            var providerConfig = await this.fetchJSON(url);
                            Config.setOnline('itemProviders.' + providerId + '.config', providerConfig);

                        } catch (error) {
                            log.info('Fetch itemProvider config failed:', providerId, itemProvider.configUrl, error);
                        }
                    }
                }
            }
        }

        try {
            await this.startXmpp();
        } catch (error) {
            throw error;
        }

        this.isReady = true;
    }

    stop(): void
    {
        this.configUpdater.stopUpdateTimer();

        // Does not work that way:
        // chrome.runtime?.onMessage.removeListener((message, sender, sendResponse) => { return this.onRuntimeMessage(message, sender, sendResponse); });

        // this.unsubscribeItemInventories();
        this.stopXmpp();
    }

    async getOrCreateItemProviderUserId(providerId: string): Promise<string>
    {
        let userId = await Memory.getSync(Utils.syncStorageKey_ItemProviderUserId(providerId), '');
        if (userId == '') {
            userId = 'ext' + Utils.randomString(40).toLowerCase();
            await Memory.setSync(Utils.syncStorageKey_ItemProviderUserId(providerId), userId);
        }
        return userId;
    }

    // IPC

    onDirectRuntimeMessage(message: any, sendResponse: (response?: any) => void) {
        const sender = {tab: {id: 0}};
        this.onRuntimeMessage(message, sender, sendResponse);
    }

    private onRuntimeMessage(message, sender/*: chrome.runtime.MessageSender*/, sendResponse: (response?: any) => void): boolean
    {
        switch (message.type) {
            case BackgroundMessage.test.name: {
                sendResponse(this.handle_test());
                return false;
            } break;

            case BackgroundMessage.fetchUrl.name: {
                return this.handle_fetchUrl(message.url, message.version, sendResponse);
            } break;

            case BackgroundMessage.jsonRpc.name: {
                return this.handle_jsonRpc(message.url, message.json, sendResponse);
            } break;

            case BackgroundMessage.waitReady.name: {
                return this.handle_waitReady(sendResponse);
            } break;

            case BackgroundMessage.getConfigTree.name: {
                sendResponse(this.handle_getConfigTree(message.name));
                return false;
            } break;

            case BackgroundMessage.sendStanza.name: {
                sendResponse(this.handle_sendStanza(message.stanza, sender.tab.id));
                return false;
            } break;

            case BackgroundMessage.pingBackground.name: {
                sendResponse(this.handle_pingBackground());
                return false;
            } break;

            case BackgroundMessage.userSettingsChanged.name: {
                sendResponse(this.handle_userSettingsChanged());
                return false;
            } break;

            case BackgroundMessage.getBackpackState.name: {
                return this.handle_getBackpackState(sendResponse);
            } break;

            case BackgroundMessage.addBackpackItem.name: {
                return this.handle_addBackpackItem(message.itemId, message.properties, message.options, sendResponse);
            } break;

            case BackgroundMessage.setBackpackItemProperties.name: {
                return this.handle_setBackpackItemProperties(message.itemId, message.properties, message.options, sendResponse);
            } break;

            case BackgroundMessage.modifyBackpackItemProperties.name: {
                return this.handle_modifyBackpackItemProperties(message.itemId, message.changed, message.deleted, message.options, sendResponse);
            } break;

            case BackgroundMessage.rezBackpackItem.name: {
                return this.handle_rezBackpackItem(message.itemId, message.roomJid, message.x, message.destination, message.options, sendResponse);
            } break;

            case BackgroundMessage.derezBackpackItem.name: {
                return this.handle_derezBackpackItem(message.itemId, message.roomJid, message.x, message.y, message.options, sendResponse);
            } break;

            case BackgroundMessage.isBackpackItem.name: {
                return this.handle_isBackpackItem(message.itemId, sendResponse);
            } break;

            case BackgroundMessage.executeBackpackItemAction.name: {
                return this.handle_executeBackpackItemAction(message.itemId, message.action, message.args, message.involvedIds, sendResponse);
            } break;

            default: {
                log.debug('BackgroundApp.onRuntimeMessage unhandled', message);
                sendResponse(new BackgroundErrorResponse('error', 'unhandled message type=' + message.type));
                return false;
            } break;
        }
    }

    maintainHttpCache(): void
    {
        log.debug('BackgroundApp.maintainHttpCache');
        let cacheTimeout = Config.get('httpCache.maxAgeSec', 3600);
        let now = Date.now();
        let deleteKeys = new Array<string>();
        for (let key in this.httpCacheTime) {
            if (now - this.httpCacheTime[key] > cacheTimeout * 1000) {
                deleteKeys.push(key);
            }
        }

        deleteKeys.forEach(key =>
        {
            log.debug('BackgroundApp.maintainHttpCache', (now - this.httpCacheTime[key]) / 1000, 'sec', 'delete', key);
            delete this.httpCacheData[key];
            delete this.httpCacheTime[key];
        });
    }

    private async fetchJSON(url: string): Promise<any>
    {
        log.info('BackgroundApp.fetchJSON', url);

        return new Promise((resolve, reject) =>
        {
            $
                .getJSON(url, data => resolve(data))
                .fail(reason => reject(null));
        });
    }

    lastCacheMaintenanceTime: number = 0;
    handle_fetchUrl(url: any, version: any, sendResponse: (response?: any) => void): boolean
    {
        let now = Date.now();
        let maintenanceIntervalSec = Config.get('httpCache.maintenanceIntervalSec', 60);
        if (now - this.lastCacheMaintenanceTime > maintenanceIntervalSec * 1000) {
            this.maintainHttpCache();
            this.lastCacheMaintenanceTime = now;
        }

        let key = version + url;
        let isCached = version != '_nocache' && this.httpCacheData[key] != undefined;

        if (isCached) {
            log.debug('BackgroundApp.handle_fetchUrl', 'cache-age', (now - this.httpCacheTime[key]) / 1000, url, 'version=', version);
        } else {
            log.debug('BackgroundApp.handle_fetchUrl', 'not-cached', url, 'version=', version);
        }

        if (isCached) {
            sendResponse({ 'ok': true, 'data': this.httpCacheData[key] });
        } else {
            try {
                fetch(url, { cache: 'reload' })
                    .then(httpResponse =>
                    {
                        // log.debug('BackgroundApp.handle_fetchUrl', 'httpResponse', url, httpResponse);
                        if (httpResponse.ok) {
                            return httpResponse.text();
                        } else {
                            throw { 'ok': false, 'status': httpResponse.status, 'statusText': httpResponse.statusText };
                        }
                    })
                    .then(text =>
                    {
                        if (version == '_nocache') {
                            //dont cache
                        } else {
                            this.httpCacheData[key] = text;
                            this.httpCacheTime[key] = now;
                        }
                        let response = { 'ok': true, 'data': text };
                        log.debug('BackgroundApp.handle_fetchUrl', 'response', url, text.length, response);
                        sendResponse(response);
                    })
                    .catch(ex =>
                    {
                        log.debug('BackgroundApp.handle_fetchUrl', 'catch', url, ex);
                        sendResponse({ 'ok': false, 'status': ex.name, 'statusText': ex.message });
                    });
                return true;
            } catch (error) {
                log.debug('BackgroundApp.handle_fetchUrl', 'exception', url, error);
                sendResponse({ 'ok': false, 'status': error.status, 'statusText': error.statusText });
            }
        }
        return false;
    }

    handle_jsonRpc(url: string, postBody: any, sendResponse: (response?: any) => void): boolean
    {
        try {
            fetch(url, {
                method: 'POST',
                cache: 'reload',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(postBody),
                redirect: 'error'
            })
                .then(httpResponse =>
                {
                    log.debug('BackgroundApp.handle_jsonRpc', 'httpResponse', url, postBody, httpResponse);
                    if (httpResponse.ok) {
                        return httpResponse.text();
                    } else {
                        throw { 'ok': false, 'status': httpResponse.status, 'statusText': httpResponse.statusText };
                    }
                })
                .then(text =>
                {
                    let response = { 'ok': true, 'data': text };
                    log.debug('BackgroundApp.handle_jsonRpc', 'response', url, text.length, response);
                    sendResponse(response);
                })
                .catch(ex =>
                {
                    log.debug('BackgroundApp.handle_jsonRpc', 'catch', url, ex);
                    let status = ex.status;
                    if (!status) { status = ex.name; }
                    if (!status) { status = 'Error'; }
                    let statusText = ex.statusText;
                    if (!statusText) { status = ex.message; }
                    if (!statusText) { status = ex; }
                    sendResponse({ 'ok': false, 'status': status, 'statusText': statusText });
                });
            return true;
        } catch (error) {
            log.debug('BackgroundApp.handle_jsonRpc', 'exception', url, error);
            sendResponse({ 'ok': false, 'status': error.status, 'statusText': error.statusText });
        }
        return false;
    }

    handle_waitReady(sendResponse: (response?: any) => void)
    {
        log.debug('BackgroundApp.handle_waitReady');
        let sendResponseIsAsync = false;

        if (this.isReady) {
            sendResponse({});
            return sendResponseIsAsync;
        }

        sendResponseIsAsync = true;
        let pollTimer = setInterval(() =>
        {
            if (this.isReady) {
                clearInterval(pollTimer);
                sendResponse({});
            }
        }, 100);
        return sendResponseIsAsync;
    }

    handle_getConfigTree(name: any)
    {
        log.debug('BackgroundApp.handle_getConfigTree', name, this.isReady);
        switch (as.String(name, Config.onlineConfigName)) {
            case Config.devConfigName: return Config.getDevTree();
            case Config.onlineConfigName: return Config.getOnlineTree();
            case Config.staticConfigName: return Config.getStaticTree();
        }
        return Config.getOnlineTree();
    }

    handle_getSessionConfig(key: string): any
    {
        let response = {};
        try {
            let value = Config.get(key, undefined);
            if (value != undefined) {
                response[key] = value;
            }
        } catch (error) {
            log.info('BackgroundApp.handle_getSessionConfig', error);
        }

        log.debug('BackgroundApp.handle_getSessionConfig', key, 'response', response);
        return response;
    }

    handle_setSessionConfig(key: string, value: string): any
    {
        log.debug('BackgroundApp.handle_setSessionConfig', key, value);
        try {
            Memory.setSession(key, value);
        } catch (error) {
            log.info('BackgroundApp.handle_setSessionConfig', error);
        }
    }

    handle_getBackpackState(sendResponse: (response?: any) => void): boolean
    {
        if (this.backpack) {
            let items = this.backpack.getItems();
            sendResponse(new GetBackpackStateResponse(items));
        } else {
            sendResponse(new BackgroundItemExceptionResponse(new ItemException(ItemException.Fact.NoItemsReceived, ItemException.Reason.ItemsNotAvailable)));
        }
        return false;
    }

    handle_addBackpackItem(itemId: string, properties: ItemProperties, options: ItemChangeOptions, sendResponse: (response?: any) => void): boolean
    {
        if (this.backpack) {
            this.backpack.addItem(itemId, properties, options)
                .then(() => { sendResponse(new BackgroundSuccessResponse()); })
                .catch(ex => { sendResponse(new BackgroundItemExceptionResponse(ex)); });
            return true;
        } else {
            sendResponse(new BackgroundItemExceptionResponse(new ItemException(ItemException.Fact.NotAdded, ItemException.Reason.ItemsNotAvailable)));
        }
        return false;
    }

    handle_setBackpackItemProperties(itemId: string, properties: ItemProperties, options: ItemChangeOptions, sendResponse: (response?: any) => void): boolean
    {
        if (this.backpack) {
            this.backpack.setItemProperties(itemId, properties, options)
                .then(() => { sendResponse(new BackgroundSuccessResponse()); })
                .catch(ex => { sendResponse(new BackgroundItemExceptionResponse(ex)); });
            return true;
        } else {
            sendResponse(new BackgroundItemExceptionResponse(new ItemException(ItemException.Fact.NotChanged, ItemException.Reason.ItemsNotAvailable)));
        }
        return false;
    }

    handle_modifyBackpackItemProperties(itemId: string, changed: ItemProperties, deleted: Array<string>, options: ItemChangeOptions, sendResponse: (response?: any) => void): boolean
    {
        if (this.backpack) {
            this.backpack.modifyItemProperties(itemId, changed, deleted, options)
                .then(() => { sendResponse(new BackgroundSuccessResponse()); })
                .catch(ex => { sendResponse(new BackgroundItemExceptionResponse(ex)); });
            return true;
        } else {
            sendResponse(new BackgroundItemExceptionResponse(new ItemException(ItemException.Fact.NotChanged, ItemException.Reason.ItemsNotAvailable)));
        }
        return false;
    }

    handle_rezBackpackItem(itemId: string, room: string, x: number, destination: string, options: ItemChangeOptions, sendResponse: (response?: any) => void): boolean
    {
        if (this.backpack) {
            this.backpack.rezItem(itemId, room, x, destination, options)
                .then(() => { sendResponse(new BackgroundSuccessResponse()); })
                .catch(ex => { sendResponse(new BackgroundItemExceptionResponse(ex)); });
            return true;
        } else {
            sendResponse(new BackgroundItemExceptionResponse(new ItemException(ItemException.Fact.NotRezzed, ItemException.Reason.ItemsNotAvailable)));
        }
        return false;
    }

    handle_derezBackpackItem(itemId: string, roomJid: string, x: number, y: number, options: ItemChangeOptions, sendResponse: (response?: any) => void): boolean
    {
        if (this.backpack) {
            this.backpack.derezItem(itemId, roomJid, x, y, options)
                .then(() => { sendResponse(new BackgroundSuccessResponse()); })
                .catch(ex => { sendResponse(new BackgroundItemExceptionResponse(ex)); });
            return true;
        } else {
            sendResponse(new BackgroundItemExceptionResponse(new ItemException(ItemException.Fact.NotRezzed, ItemException.Reason.ItemsNotAvailable)));
        }
        return false;
    }

    handle_isBackpackItem(itemId: string, sendResponse: (response?: any) => void): boolean
    {
        if (this.backpack) {
            let isItem = this.backpack.isItem(itemId);
            sendResponse(new IsBackpackItemResponse(isItem));
        } else {
            sendResponse(new IsBackpackItemResponse(false));
        }
        return false;
    }

    handle_executeBackpackItemAction(itemId: string, action: string, args: any, involvedIds: Array<string>, sendResponse: (response?: any) => void): boolean
    {
        if (this.backpack) {
            this.backpack.executeItemAction(itemId, action, args, involvedIds)
                .then(() => { sendResponse(new BackgroundSuccessResponse()); })
                .catch(ex => { sendResponse(new BackgroundItemExceptionResponse(ex)); });
            return true;
        } else {
            sendResponse(new BackgroundItemExceptionResponse(new ItemException(ItemException.Fact.NotChanged, ItemException.Reason.ItemsNotAvailable)));
        }
        return false;
    }

    // manage stanza from 2 tabId mappings

    addRoomJid2TabId(room: string, tabId: number): void
    {
        let tabIds = this.getRoomJid2TabIds(room);
        if (!tabIds) {
            tabIds = new Array<number>();
            this.roomJid2tabId[room] = tabIds;
        }
        if (!tabIds.includes(tabId)) {
            tabIds.push(tabId);
        }
    }

    removeRoomJid2TabId(room: string, tabId: number): void
    {
        let tabIds = this.getRoomJid2TabIds(room);
        if (tabIds) {
            const index = tabIds.indexOf(tabId, 0);
            if (index > -1) {
                tabIds.splice(index, 1);
                if (tabIds.length == 0) {
                    delete this.roomJid2tabId[room];
                }
            }
        }
    }

    getRoomJid2TabIds(room: string): Array<number>
    {
        return this.roomJid2tabId[room];
    }

    hasRoomJid2TabId(room: string, tabId: number): boolean
    {
        var tabIds = this.getRoomJid2TabIds(room);
        if (tabIds) {
            return tabIds.includes(tabId);
        }
        return false;
    }

    getAllTabIds(): Array<number>
    {
        let tabIds = [];
        for (let room in this.roomJid2tabId) {
            let roomTabIds = this.roomJid2tabId[room];
            for (let i = 0; i < roomTabIds.length; i++) {
                let tabId = roomTabIds[i];
                if (!tabIds.includes(tabId)) {
                    tabIds.push(tabId);
                }
            }
        }
        return tabIds;
    }

    // send/recv stanza

    handle_sendStanza(stanza: any, tabId: number): void
    {
        // log.debug('BackgroundApp.handle_sendStanza', stanza, tabId);

        try {
            let xmlStanza: xml = Utils.jsObject2xmlObject(stanza);

            if (this.backpack) {
                xmlStanza = this.backpack.stanzaOutFilter(xmlStanza);
                if (xmlStanza == null) { return; }
            }

            if (stanza.name == 'presence') {
                let to = jid(stanza.attrs.to);
                let room = to.bare().toString();
                let nick = to.getResource();

                if (as.String(stanza.attrs['type'], 'available') == 'available') {
                    if (!this.hasRoomJid2TabId(room, tabId)) {
                        this.addRoomJid2TabId(room, tabId);
                        log.debug('BackgroundApp.handle_sendStanza', 'adding room2tab mapping', room, '=>', tabId, 'now:', this.roomJid2tabId);
                    }
                } else {
                    this.fullJid2TabWhichSentUnavailable[to] = tabId;
                }
            }

            if (stanza.name == 'iq') {
                if (stanza.attrs) {
                    let stanzaType = stanza.attrs.type;
                    let stanzaId = stanza.attrs.id;
                    if ((stanzaType == 'get' || stanzaType == 'set') && stanzaId) {
                        this.iqStanzaTabId[stanzaId] = tabId;
                    }
                }
            }

            this.sendStanza(xmlStanza);

        } catch (error) {
            log.info('BackgroundApp.handle_sendStanza', error);
        }
    }

    public sendStanza(stanza: xml): void
    {
        if (!this.xmppConnected) {
            this.stanzaQ.push(stanza);
        } else {
            this.sendStanzaUnbuffered(stanza);
        }
    }

    private sendStanzaUnbuffered(stanza: xml): void
    {
        try {
            this.logStanzaButNotBasicConnectionPresence(stanza);

            this.xmpp.send(stanza);
        } catch (error) {
            log.info('BackgroundApp.sendStanza', error.message ?? '');
        }
    }

    private logStanzaButNotBasicConnectionPresence(stanza: xml)
    {
        let isConnectionPresence = false;
        if (stanza.name == 'presence') {
            isConnectionPresence = (!stanza.attrs || !stanza.attrs.to || jid(stanza.attrs.to).getResource() == this.resource);
        }
        if (!isConnectionPresence) {
            // log.info('BackgroundApp.sendStanza', stanza);
            log.info('BackgroundApp.sendStanza', stanza, as.String(stanza.attrs.type, stanza.name == 'presence' ? 'available' : 'normal'), 'to=', stanza.attrs.to);
        }
    }

    private sendPresence()
    {
        this.sendStanza(xml('presence'));
    }

    private recvStanza(stanza: any)
    {
        {
            let isConnectionPresence = false;
            if (stanza.name == 'presence') {
                isConnectionPresence = stanza.attrs.from && (jid(stanza.attrs.from).getResource() == this.resource);
            }
            if (!isConnectionPresence) {
                // log.info('BackgroundApp.recvStanza', stanza);
                log.info('BackgroundApp.recvStanza', stanza, as.String(stanza.attrs.type, stanza.name == 'presence' ? 'available' : 'normal'), 'to=', stanza.attrs.to, 'from=', stanza.attrs.from);
            }
        }

        if (stanza.name == 'iq') {
            if (stanza.attrs) {
                let stanzaType = stanza.attrs.type;
                let stanzaId = stanza.attrs.id;
                if (stanzaType == 'result' && stanzaId) {
                    let tabId = this.iqStanzaTabId[stanzaId];
                    if (tabId) {
                        delete this.iqStanzaTabId[stanzaId];
                        ContentMessage.sendMessage(tabId, { 'type': ContentMessage.Type[ContentMessage.Type.recvStanza], 'stanza': stanza });
                    }
                }
            }
        }

        if (stanza.name == 'message') {
            let from = jid(stanza.attrs.from);
            let room = from.bare().toString();

            let tabIds = this.getRoomJid2TabIds(room);
            if (tabIds) {
                for (let i = 0; i < tabIds.length; i++) {
                    let tabId = tabIds[i];
                    ContentMessage.sendMessage(tabId, { 'type': ContentMessage.Type[ContentMessage.Type.recvStanza], 'stanza': stanza });
                }
            }
        }

        if (stanza.name == 'presence') {
            let from = jid(stanza.attrs.from);
            let room = from.bare().toString();
            let nick = from.getResource();

            let unavailableTabId: number = -1;
            if (stanza.attrs && stanza.attrs['type'] == 'unavailable') {
                unavailableTabId = this.fullJid2TabWhichSentUnavailable[from];
                if (unavailableTabId) {
                    delete this.fullJid2TabWhichSentUnavailable[from];
                }
            }

            if (unavailableTabId >= 0) {
                ContentMessage.sendMessage(unavailableTabId, { 'type': ContentMessage.Type.recvStanza, 'stanza': stanza });
                this.removeRoomJid2TabId(room, unavailableTabId);
                log.debug('BackgroundApp.recvStanza', 'removing room2tab mapping', room, '=>', unavailableTabId, 'now:', this.roomJid2tabId);
            } else {
                let tabIds = this.getRoomJid2TabIds(room);
                if (tabIds) {
                    for (let i = 0; i < tabIds.length; i++) {
                        let tabId = tabIds[i];
                        ContentMessage.sendMessage(tabId, { 'type': ContentMessage.Type[ContentMessage.Type.recvStanza], 'stanza': stanza });
                    }
                }
            }
        }
    }

    // xmpp

    private async startXmpp()
    {
        this.resource = Utils.randomString(15);

        let xmppUser = await Memory.getSync('xmpp.user', undefined);
        if (xmppUser == undefined) { xmppUser = Config.get('xmpp.user', ''); }

        let xmppPass = await Memory.getSync('xmpp.pass', undefined);
        if (xmppPass == undefined) { xmppPass = Config.get('xmpp.pass', ''); }

        try {
            var conf = {
                service: Config.get('xmpp.service', 'wss://xmpp.k8s.sui.li/xmpp-websocket'),
                domain: Config.get('xmpp.domain', 'xmpp.weblin.sui.li'),
                resource: this.resource,
                username: xmppUser,
                password: xmppPass,
            };
            if (conf.username == '' || conf.password == '') {
                throw 'Missing xmpp.user or xmpp.pass';
            }
            this.xmpp = client(conf);

            this.xmpp.on('error', (err: any) =>
            {
                log.info('BackgroundApp xmpp.on.error', err);
            });

            this.xmpp.on('offline', () =>
            {
                log.info('BackgroundApp xmpp.on.offline');
                this.xmppConnected = false;
            });

            this.xmpp.on('online', (address: any) =>
            {
                log.info('BackgroundApp xmpp.on.online', address);

                this.sendPresence();

                this.xmppConnected = true;
                while (this.stanzaQ.length > 0) {
                    let stanza = this.stanzaQ.shift();
                    this.sendStanzaUnbuffered(stanza);
                }
            });

            this.xmpp.on('stanza', (stanza: any) => this.recvStanza(stanza));

            this.xmpp.start().catch(log.info);
        } catch (error) {
            log.info(error);
        }
    }

    private stopXmpp()
    {
        //hw todo
    }

    // Message to all tabs

    sendToAllTabs(type: string, data: any)
    {
        let tabIds = this.getAllTabIds();
        if (tabIds) {
            for (let i = 0; i < tabIds.length; i++) {
                let tabId = tabIds[i];
                ContentMessage.sendMessage(tabId, { 'type': type, 'data': data });
            }
        }
    }

    sendToTabsForRoom(room: string, type: string)
    {
        let tabIds = this.getRoomJid2TabIds(room);
        if (tabIds) {
            for (let i = 0; i < tabIds.length; i++) {
                let tabId = tabIds[i];
                ContentMessage.sendMessage(tabId, { 'type': type });
            }
        }
    }

    // Keep connection alive

    private lastPingTime: number = 0;
    handle_pingBackground(): void
    {
        log.debug('BackgroundApp.handle_pingBackground');
        try {
            let now = Date.now();
            if (now - this.lastPingTime > 10000) {
                this.lastPingTime = now;
                this.sendPresence();
            }
        } catch (error) {
            //
        }
    }

    // 

    handle_userSettingsChanged(): void
    {
        log.debug('BackgroundApp.handle_userSettingsChanged');
        try {
            for (let room in this.roomJid2tabId) {
                let tabIds = this.roomJid2tabId[room];
                if (tabIds) {
                    tabIds.forEach(tabId =>
                    {
                        ContentMessage.sendMessage(tabId, { 'type': 'userSettingsChanged' });
                    });
                }
            }

        } catch (error) {
            //
        }
    }

    handle_test(): void
    {
    }
}
