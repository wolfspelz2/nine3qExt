import log = require('loglevel');
import { client, xml, jid } from '@xmpp/client';
import { Utils } from '../lib/Utils';

interface ILocationMapperResponse
{
    //    sMessage: string;
    sLocationURL: string;
}

export class BackgroundApp
{
    private xmpp: any;
    private activeTabId: number;
    private roomJid2tabId: { [roomJid: string]: number; } = {};

    public start(): void
    {
        chrome.tabs.onActivated.addListener((activeInfo) => { return this.tabsOnActivated(activeInfo); });
        // chrome.tabs.onUpdated.addListener(this.tabsOnUpdated);

        chrome.tabs.query({ active: true }, (result: Array<chrome.tabs.Tab>) => { this.activeTabId = result[0].id; });

        this.startXmpp();
    }

    public stop(): void
    {
        // chrome.tabs.onUpdated.removeListener(this.tabsOnUpdated);
        chrome.tabs.onActivated.removeListener((activeInfo) => { this.tabsOnActivated(activeInfo); });

        this.stopXmpp();
    }

    private tabsOnActivated(activeInfo: chrome.tabs.TabActiveInfo): void
    {
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

    private handle_sendStanza(stanza: any, tabId: number, sendResponse: any): any
    {
        log.info('BackgroundApp.handle_sendStanza', stanza, tabId);

        try {
            let xmlStanza = Utils.jsObject2xmlObject(stanza);
            this.sendStanza(xmlStanza);
        } catch (ex) {
            log.error('BackgroundApp.handle_sendStanza', ex);
        }
    }

    // xmpp

    private startXmpp()
    {
        this.xmpp = client({
            service: 'wss://xmpp.weblin.sui.li/xmpp-websocket',
            // service: 'wss://xmpp.dev.sui.li/xmpp-websocket',
            domain: 'xmpp.weblin.sui.li',
            resource: 'schnuppe',
            username: 'f85bpkavrnp0j2r8jgb079kmsg',
            password: '475167916f52ab832e89386eddc90295e81c5563',
        });

        this.xmpp.on('error', (err: any) =>
        {
            log.error('BackgroundApp. xmpp.on.error', err);
        });

        this.xmpp.on('offline', () =>
        {
            log.warn('BackgroundApp xmpp.on.offline');
        });

        this.xmpp.on('online', async (address: any) =>
        {
            log.info('BackgroundApp xmpp.on.online', address);
            this.sendPresence();
            this.keepAlive();
        });

        this.xmpp.on('stanza', (stanza: any) => this.recvStanza(stanza));

        this.xmpp.start().catch(log.error);
    }

    private stopXmpp()
    {
        //hw todo
    }

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

    private sendStanza(stanza: any): void
    {
        log.debug('BackgroundApp.sendStanza', stanza);
        try {
            this.xmpp.send(stanza);
        } catch (ex) {
            log.warn('BackgroundApp.sendStanza', ex.message ?? '');
        }
    }

    // recv / forward

    private recvStanza(stanza: any)
    {
        log.debug('BackgroundApp.recvStanza', stanza);
        chrome.tabs.sendMessage(this.activeTabId, { 'type': 'recvStanza', 'stanza': stanza });

        // if (stanza.attrs != undefined) {
        //     if (stanza.attrs.from != undefined) {
        //         let from = jid(stanza.attrs.from);
        //         let roomOrUser = from.bare();
        //         if (typeof this.rooms[roomOrUser] != typeof undefined) {
        //             log.info('BackgroundApp.onStanza', stanza);


        //             if (stanza.is('presence')) {
        //                 if (as.String(stanza.attrs.type, 'available') == 'unavailable') {
        //                     let nick = from.getResource();
        //                     let roomNick = this.rooms[roomOrUser].getNick();
        //                     if (nick == roomNick) {
        //                         delete this.rooms[roomOrUser];
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
    }
}