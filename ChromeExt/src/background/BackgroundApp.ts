import log = require('loglevel');
import { client, xml, jid } from '@xmpp/client';
import { Utils } from '../lib/Utils';
import { ConfigUpdater } from './ConfigUpdater';
import { timingSafeEqual } from 'crypto';
import { Config } from '../lib/Config';
import { Panic } from '../lib/Panic';
import { as } from '../lib/as';

interface ILocationMapperResponse
{
    //    sMessage: string;
    sLocationURL: string;
}

export class BackgroundApp
{
    private xmpp: any;
    private activeTabId: number;
    private roomJid2tabId: Map<string, number> = new Map<string, number>();
    private roomJid2selfNick: Map<string, string> = new Map<string, string>();
    private xmppConnected = false;
    private stanzaQ: Array<xml> = [];
    private configUpdater: ConfigUpdater;

    async start(): Promise<void>
    {
        let devConfig = await Config.getSync('dev.config', '{}');
        try {
            let parsed = JSON.parse(devConfig);
            Config.setAllDev(parsed);
        } catch (error) {
            
        }

        this.configUpdater = new ConfigUpdater();
        await this.configUpdater.getUpdate();
        await this.configUpdater.startUpdateTimer()

        chrome.tabs.onActivated.addListener(activeInfo => { return this.tabsOnActivated(activeInfo); });
        // chrome.tabs.onUpdated.addListener(this.tabsOnUpdated);

        chrome.tabs.query({ active: true }, (result: Array<chrome.tabs.Tab>) => { this.activeTabId = result[0].id; });

        try {
            await this.startXmpp();
        } catch (error) {
            throw error;
        }
    }

    stop(): void
    {
        this.configUpdater.stopUpdateTimer();

        // chrome.tabs.onUpdated.removeListener(this.tabsOnUpdated);
        chrome.tabs.onActivated.removeListener(activeInfo => { this.tabsOnActivated(activeInfo); });

        this.stopXmpp();
    }

    private tabsOnActivated(activeInfo: chrome.tabs.TabActiveInfo): void
    {
        log.debug('BackgroundApp.tabsOnActivated', 'tab=', activeInfo.tabId, 'window=', activeInfo.windowId);
        this.activeTabId = activeInfo.tabId;
    }

    // private tabsOnUpdated(tabId: number, changeInfo: any, tab: chrome.tabs.Tab): void
    // {
    //     if (tabId == this.tabId) {
    //         if (changeInfo.url != undefined) {
    //             if (this.pageUrl != changeInfo.url) {
    //                 this.leaveCurrentRoom();
    //                 this.enterRoomByPageUrl(this.pageUrl);
    //             }
    //         }
    //     }
    // }

    // IPC

    handle_sendStanza(stanza: any, tabId: number, sendResponse: any): any
    {
        log.info('BackgroundApp.handle_sendStanza', stanza, tabId);

        try {
            let xmlStanza = Utils.jsObject2xmlObject(stanza);

            if (stanza.name == 'presence') {
                let to = jid(stanza.attrs.to);
                let room = to.bare().toString();
                let nick = to.getResource();

                if (as.String(stanza.attrs['type'], 'available') == 'available') {
                    if (this.roomJid2tabId[room] == undefined) {
                        log.debug('BackgroundApp.handle_sendStanza', 'adding room2tab mapping', room, '=>', tabId);
                    }
                    this.roomJid2tabId[room] = tabId;
                    this.roomJid2selfNick[room] = nick;
                }
            }

            this.sendStanza(xmlStanza);

        } catch (error) {
            log.error('BackgroundApp.handle_sendStanza', error);
        }
    }

    private recvStanza(stanza: any)
    {
        log.debug('BackgroundApp.recvStanza', stanza);

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
                        log.debug('BackgroundApp.recvStanza', 'removing room2tab mapping', room, '=>', this.roomJid2tabId[room]);
                        delete this.roomJid2tabId[room];
                        delete this.roomJid2selfNick[room];
                    }
                }
            }
        }
    }

    // xmpp

    private async startXmpp()
    {
        try {
            var conf = {
                service: Config.get('xmpp.service', 'wss://xmpp.weblin.sui.li/xmpp-websocket'),// service: 'wss://xmpp.dev.sui.li/xmpp-websocket',
                domain: Config.get('xmpp.domain', 'xmpp.weblin.sui.li'),
                resource: Config.get('xmpp.resource', 'web'),
                username: await Config.getPreferSync('xmpp.user', ''),
                password: await Config.getPreferSync('xmpp.pass', ''),
            };
            if (conf.username == '' || conf.password == '') {
                throw 'Missing xmpp.user or xmpp.pass';
            }
            this.xmpp = client(conf);

            this.xmpp.on('error', (err: any) =>
            {
                log.error('BackgroundApp xmpp.on.error', err);
            });

            this.xmpp.on('offline', () =>
            {
                log.warn('BackgroundApp xmpp.on.offline');
                this.xmppConnected = false;
            });

            this.xmpp.on('online', async (address: any) =>
            {
                log.info('BackgroundApp xmpp.on.online', address);

                this.sendPresence();
                this.keepAlive();

                if (!this.xmppConnected) {
                    this.xmppConnected = true;
                    while (this.stanzaQ.length > 0) {
                        let stanza = this.stanzaQ.shift();
                        this.sendStanzaUnbuffered(stanza);
                    }
                }
            });

            this.xmpp.on('stanza', (stanza: any) => this.recvStanza(stanza));

            this.xmpp.start().catch(log.error);
        } catch (error) {
            log.error(error);
        }
    }

    private stopXmpp()
    {
        //hw todo
    }

    private sendStanzaUnbuffered(stanza: any): void
    {
        try {
            this.xmpp.send(stanza);
        } catch (error) {
            log.warn('BackgroundApp.sendStanza', error.message ?? '');
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

    // keepalive

    private keepAliveSec: number = 180;
    private keepAliveTimer: number = undefined;
    private keepAlive()
    {
        if (this.keepAliveTimer == undefined) {
            this.keepAliveTimer = <number><unknown>setTimeout(() =>
            {
                this.sendPresence();
                this.keepAliveTimer = undefined;
                this.keepAlive();
            }, this.keepAliveSec * 1000);
        }
    }

    private sendPresence()
    {
        this.sendStanza(xml('presence'));
    }
}