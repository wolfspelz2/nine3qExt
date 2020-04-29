import { client, xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Log } from '../lib/Log';
import { Utils } from '../lib/Utils';
import { Platform } from '../lib/Platform';

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
        // chrome.runtime.onMessage.addListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });
        chrome.tabs.onActivated.addListener((activeInfo) => { return this.tabsOnActivated(activeInfo); });
        // chrome.tabs.onUpdated.addListener(this.tabsOnUpdated);

        chrome.tabs.query({ active: true }, (result: Array<chrome.tabs.Tab>) => { this.activeTabId = result[0].id; });

        this.startXmpp();
    }

    public stop(): void
    {
        // chrome.tabs.onUpdated.removeListener(this.tabsOnUpdated);
        chrome.tabs.onActivated.removeListener((activeInfo) => { this.tabsOnActivated(activeInfo); });
        // chrome.runtime.onMessage.removeListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });

        this.stopXmpp();
    }

    public runtimeOnMessage(message, sender: chrome.runtime.MessageSender, sendResponse): boolean
    {
        switch (message.type) {
            case 'fetchUrl': return this.handle_fetchUrl(message.url, sender.tab.id, sendResponse); break;
            case 'sendStanza': return this.handle_sendStanza(message.stanza, sender.tab.id, sendResponse); break;
        }
        return true;
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

    private handle_fetchUrl(url: string, tabId: number, sendResponse: any): any
    {
        Log.info('BackgroundApp.handle_fetchUrl', url, tabId);

        try {
            fetch(url)
                .then(httpResponse =>
                {
                    console.log('BackgroundApp.handle_fetchUrl response', httpResponse);
                    if (httpResponse.ok) {
                        return httpResponse.text();
                    } else {
                        throw { 'ok': false, 'status': httpResponse.status, 'statusText': httpResponse.statusText };
                    }
                })
                .then(text =>
                {
                    console.log('BackgroundApp.handle_fetchUrl text', text);
                    return sendResponse({ 'ok': true, 'data': text });
                })
                .catch(ex =>
                {
                    console.log('BackgroundApp.handle_fetchUrl catch', ex);
                    return sendResponse({ 'ok': false, 'status': ex.status, 'statusText': ex.statusText });
                }
                );
        } catch (ex) {
            console.log('BackgroundApp.handle_fetchUrl ex', ex);
            return sendResponse({ 'ok': false, 'status': 0, 'statusText': ex.message });
        }
    }

    private handle_sendStanza(stanza: any, tabId: number, sendResponse: any): any
    {
        Log.info('BackgroundApp.handle_sendStanza', stanza, tabId);

        try {
            let xmlStanza = Utils.jsObject2xmlObject(stanza);
            this.sendStanza(xmlStanza);
        } catch (ex) {
            Log.error('BackgroundApp.handle_sendStanza', ex);
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
            Log.error('BackgroundApp. xmpp.on.error', err);
        });

        this.xmpp.on('offline', () =>
        {
            Log.info('BackgroundApp xmpp.on.offline');
        });

        this.xmpp.on('online', async (address: any) =>
        {
            Log.info('BackgroundApp xmpp.on.online', address);
            this.sendPresence();
            this.keepAlive();
        });

        this.xmpp.on('stanza', (stanza: any) => this.recvStanza(stanza));

        this.xmpp.start().catch(Log.error);
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
        Log.info('BackgroundApp.sendStanza', stanza);
        this.xmpp.send(stanza);
    }

    // recv / forward

    private recvStanza(stanza: any)
    {
        Log.info('BackgroundApp.recvStanza', stanza);
        chrome.tabs.sendMessage(this.activeTabId, { 'type': 'recvStanza', 'stanza': stanza });

        // if (stanza.attrs != undefined) {
        //     if (stanza.attrs.from != undefined) {
        //         let from = jid(stanza.attrs.from);
        //         let roomOrUser = from.bare();
        //         if (typeof this.rooms[roomOrUser] != typeof undefined) {
        //             Log.info('BackgroundApp.onStanza', stanza);


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