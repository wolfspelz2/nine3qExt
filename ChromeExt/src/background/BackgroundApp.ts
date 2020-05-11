import log = require('loglevel');
import { client, xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { ConfigUpdater } from './ConfigUpdater';
import { Config } from '../lib/Config';
import { BackgroundMessage } from '../lib/BackgroundMessage';

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

    private readonly stanzaQ: Array<xml> = [];
    private readonly roomJid2tabId: Map<string, number> = new Map<string, number>();
    private readonly roomJid2selfNick: Map<string, string> = new Map<string, string>();
    private readonly httpCacheData: Map<string, string> = new Map<string, string>();
    private readonly httpCacheTime: Map<string, number> = new Map<string, number>();

    async start(): Promise<void>
    {
        let devConfig = await Config.getSync('dev.config', '{}');
        try {
            let parsed = JSON.parse(devConfig);
            Config.setDevTree(parsed);
        } catch (error) {

        }

        chrome.runtime?.onMessage.addListener((message, sender, sendResponse) => { return this.onRuntimeMessage(message, sender, sendResponse); });

        this.configUpdater = new ConfigUpdater();
        await this.configUpdater.getUpdate();
        await this.configUpdater.startUpdateTimer()

        try {
            await this.startXmpp();
        } catch (error) {
            throw error;
        }
    }

    stop(): void
    {
        this.configUpdater.stopUpdateTimer();

        // Does not work that way
        // chrome.runtime?.onMessage.removeListener((message, sender, sendResponse) => { return this.onRuntimeMessage(message, sender, sendResponse); });

        this.stopXmpp();
    }

    // IPC

    private onRuntimeMessage(message, sender: chrome.runtime.MessageSender, sendResponse: (response?: any) => void): boolean
    {
        switch (message.type) {
            case BackgroundMessage.type_fetchUrl: {
                return this.handle_fetchUrl(message.url, message.version, sendResponse);
            } break;

            case BackgroundMessage.type_getConfigTree: {
                sendResponse(this.handle_getConfigTree(message.name));
                return false;
            } break;

            case BackgroundMessage.type_getSessionConfig: {
                sendResponse(this.handle_getSessionConfig(message.key));
                return false; // true if async
            } break;

            case BackgroundMessage.type_setSessionConfig: {
                sendResponse(this.handle_setSessionConfig(message.key, message.value));
                return false; // true if async
            } break;

            case BackgroundMessage.type_sendStanza: {
                sendResponse(this.handle_sendStanza(message.stanza, sender.tab.id, sendResponse));
                return false;
            } break;

            case BackgroundMessage.type_pingBackground: {
                sendResponse(this.handle_pingBackground());
                return false;
            } break;

            case BackgroundMessage.type_userSettingsChanged: {
                sendResponse(this.handle_userSettingsChanged());
                return false;
            } break;

            default: {
                log.debug('BackgroundApp.onRuntimeMessage unhandled', message);
                sendResponse({});
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
        let isCached = (this.httpCacheData[key] != undefined);

        if (isCached) {
            log.debug('BackgroundApp.handle_fetchUrl', 'cache-age', (now - this.httpCacheTime[key]) / 1000, url, 'version=', version);
        } else {
            log.debug('BackgroundApp.handle_fetchUrl', 'not-cached', url, 'version=', version);
        }

        if (isCached) {
            sendResponse({ 'ok': true, 'data': this.httpCacheData[key] });
        } else {
            try {
                fetch(url)
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
                        this.httpCacheData[key] = text;
                        this.httpCacheTime[key] = now;
                        let response = { 'ok': true, 'data': text };
                        log.debug('BackgroundApp.handle_fetchUrl', 'response', url, text.length, response);
                        sendResponse(response);
                    })
                    .catch(ex =>
                    {
                        log.debug('BackgroundApp.handle_fetchUrl', 'catch', url, ex);
                        sendResponse({ 'ok': false, 'status': ex.status, 'statusText': ex.statusText });
                    });
                return true;
            } catch (error) {
                log.debug('BackgroundApp.handle_fetchUrl', 'exception', url, error);
                sendResponse({ 'ok': false, 'status': error.status, 'statusText': error.statusText });
            }
        }
        return false;
    }

    handle_getConfigTree(name: any)
    {
        log.debug('BackgroundApp.handle_getConfigTree', name);
        switch (as.String(name, Config.onlineConfigName)) {
            case Config.devConfigName:
                return Config.getDevTree();
                break;
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
            Config.set(key, value);
        } catch (error) {
            log.info('BackgroundApp.handle_setSessionConfig', error);
        }
    }

    // send/recv stanza

    private recvStanza(stanza: any)
    {
        {
            let isConnectionPresence = false;
            if (stanza.name == 'presence') {
                isConnectionPresence = (jid(stanza.attrs.from).getResource() == this.resource);
            }
            if (!isConnectionPresence) {
                log.info('BackgroundApp.recvStanza', stanza);
            }
        }

        if (stanza.name == 'presence' || stanza.name == 'message') {
            let from = jid(stanza.attrs.from);
            let room = from.bare().toString();
            let tabId = this.roomJid2tabId[room];

            if (tabId != undefined) {
                chrome.tabs.sendMessage(tabId, { 'type': 'recvStanza', 'stanza': stanza });
            }

            if (stanza.name == 'presence') {
                if (stanza.attrs['type'] == 'unavailable') {
                    let nick = from.getResource();
                    let isSelf = this.roomJid2selfNick[room] == nick;
                    if (isSelf) {
                        delete this.roomJid2tabId[room];
                        delete this.roomJid2selfNick[room];
                        log.debug('BackgroundApp.recvStanza', 'removing room2tab mapping', room, '=>', this.roomJid2tabId[room], 'now:', this.roomJid2tabId);
                    }
                }
            }
        }
    }

    handle_sendStanza(stanza: any, tabId: number, sendResponse: any): void
    {
        // log.debug('BackgroundApp.handle_sendStanza', stanza, tabId);

        try {
            let xmlStanza = Utils.jsObject2xmlObject(stanza);

            if (stanza.name == 'presence') {
                let to = jid(stanza.attrs.to);
                let room = to.bare().toString();
                let nick = to.getResource();

                if (as.String(stanza.attrs['type'], 'available') == 'available') {
                    let thisIsNew = false;
                    if (this.roomJid2tabId[room] == undefined) { thisIsNew = true; }
                    this.roomJid2tabId[room] = tabId;
                    this.roomJid2selfNick[room] = nick;
                    if (thisIsNew) { log.debug('BackgroundApp.handle_sendStanza', 'adding room2tab mapping', room, '=>', tabId, 'now:', this.roomJid2tabId); }
                }
            }

            this.sendStanza(xmlStanza);

        } catch (error) {
            log.info('BackgroundApp.handle_sendStanza', error);
        }
    }

    private sendStanza(stanza: any): void
    {
        if (!this.xmppConnected) {
            this.stanzaQ.push(stanza);
        } else {
            this.sendStanzaUnbuffered(stanza);
        }
    }

    private sendStanzaUnbuffered(stanza: any): void
    {
        try {
            {
                let isConnectionPresence = false;
                if (stanza.name == 'presence') {
                    isConnectionPresence = (stanza.attrs == undefined || stanza.attrs.to == undefined || jid(stanza.attrs.to).getResource() == this.resource);
                }
                if (!isConnectionPresence) {
                    log.info('BackgroundApp.sendStanza', stanza);
                }
            }

            this.xmpp.send(stanza);
        } catch (error) {
            log.info('BackgroundApp.sendStanza', error.message ?? '');
        }
    }

    private sendPresence()
    {
        this.sendStanza(xml('presence'));
    }

    // xmpp

    private async startXmpp()
    {
        this.resource = Utils.randomString(10);

        try {
            var conf = {
                service: Config.get('xmpp.service', 'wss://xmpp.weblin.sui.li/xmpp-websocket'),// service: 'wss://xmpp.dev.sui.li/xmpp-websocket',
                domain: Config.get('xmpp.domain', 'xmpp.weblin.sui.li'),
                resource: Config.get('xmpp.resource', this.resource),
                username: await Config.getPreferSync('xmpp.user', ''),
                password: await Config.getPreferSync('xmpp.pass', ''),
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

                if (!this.xmppConnected) {
                    this.xmppConnected = true;
                    while (this.stanzaQ.length > 0) {
                        let stanza = this.stanzaQ.shift();
                        this.sendStanzaUnbuffered(stanza);
                    }
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
                let tabId = this.roomJid2tabId[room];
                if (tabId != undefined) {
                    chrome.tabs.sendMessage(tabId, { 'type': 'userSettingsChanged' });
                }
            }

        } catch (error) {
            //
        }
    }
}