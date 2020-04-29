import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Platform } from '../lib/Platform';
import { Unbearable } from '../lib/Unbearable';
import { Room } from './Room';
import { PropertyStorage } from './PropertyStorage';
import { HelloWorld } from './HelloWorld';

interface ILocationMapperResponse
{
    //    sMessage: string;
    sLocationURL: string;
}

export class ContentApp
{
    private display: HTMLElement;
    private xmpp: any;
    private myJid: string = 'test@xmpp.dev.sui.li';
    private myNick: string = 'nick_';
    private rooms: { [roomJid: string]: Room; } = {};
    private storage: PropertyStorage = new PropertyStorage();
    private keepAliveSec: number = 180;

    // Getter

    getStorage(): PropertyStorage { return this.storage; }
    getAssetUrl(filePath: string) { return Platform.getAssetUrl(filePath); }

    constructor(private appendToMe: HTMLElement)
    {
    }

    createPageControl()
    {
        let controlElem: HTMLElement = $('<div class="n3q-base n3q-ctrl" id="n3q-hello"></div>')[0];
        this.display.append(controlElem);

        $('#n3q-hello').text(HelloWorld.getText());

        let enterButton: HTMLElement = $('<button class="n3q-base">enter</button>')[0];
        controlElem.append(enterButton);
        $(enterButton).click(() =>
        {
            // this.enterRoomByJid('d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
            // this.enterRoomByPageUrl('https://www.galactic-developments.de/');
            this.enterPage();
        });
    }

    // Connection

    start()
    {
        this.display = $('<div id="n3q-id-page" class="n3q-base" />')[0];
        this.appendToMe.append(this.display);

        this.createPageControl();

        chrome.runtime.onMessage.addListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });

        this.enterPage();
    }

    stop()
    {
        this.leavePage();

        chrome.runtime.onMessage.removeListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });

        $('#n3q-id-page').remove();
        this.display = null;
    }

    runtimeOnMessage(message, sender: chrome.runtime.MessageSender, sendResponse): any
    {
        switch (message.type) {
            case 'recvStanza': this.handle_recvStanza(message.stanza); break;
            case 'backgroundInstalled': this.handle_backgroundInstalled(); break;
        }
        return true;
    }

    handle_recvStanza(jsStanza: any): void
    {
        let stanza: xml = Utils.jsObject2xmlObject(jsStanza);

        switch (stanza.name) {
            case 'presence': this.onPresence(stanza);
            case 'message': this.onMessage(stanza);
        }
    }

    handle_backgroundInstalled(): void
    {
        Unbearable.problem();
    }

    enterPage()
    {
        this.enterRoomByPageUrl(Platform.getCurrentPageUrl());
    }

    leavePage()
    {
        // Leave all, there should be only one
        for (let roomJid in this.rooms) {
            this.leaveRoomByJid(roomJid);
        }
    }

    static getRoomJidFromLocationUrl(locationUrl: string): string
    {
        let jid = '';
        let url = new URL(locationUrl);
        return url.pathname;
    }

    enterRoomByPageUrl(pageUrl: string): void
    {
        let url = new URL('http://lms.virtual-presence.org/api/');
        url.searchParams.set('Method', 'VPI.Info');
        url.searchParams.set('sDocumentURL', pageUrl);
        url.searchParams.set('Format', 'json');

        Platform.fetchUrl(url.toString(), (ok, status, statusText, data: string) =>
        {
            if (ok) {
                try {
                    let mappingResponse: ILocationMapperResponse = JSON.parse(data);
                    let locationUrl = mappingResponse.sLocationURL;
                    log.info('Mapped', pageUrl, ' => ', locationUrl);
                    let roomJid = ContentApp.getRoomJidFromLocationUrl(locationUrl);
                    this.enterRoomByJid(roomJid);
                } catch (ex) {
                    log.error(ex);
                }
            }
        });
    }

    enterRoomByJid(roomJid: string): void
    {
        if (this.rooms[roomJid] === undefined) {
            this.rooms[roomJid] = new Room(this, this.display, roomJid, this.myJid, this.myNick);
        }
        log.info('ContentApp.enterRoomByJid', roomJid);
        this.rooms[roomJid].enter();
    }

    leaveRoomByJid(roomJid: string): void
    {
        log.info('ContentApp.leaveRoomByJid', roomJid);
        if (this.rooms[roomJid] != undefined) {
            this.rooms[roomJid].leave();
            delete this.rooms[roomJid];
        }
    }

    onPresence(stanza: xml): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (typeof this.rooms[roomOrUser] != typeof undefined) {
            this.rooms[roomOrUser].onPresence(stanza);
        }
    }

    onMessage(stanza: xml): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (typeof this.rooms[roomOrUser] != typeof undefined) {
            this.rooms[roomOrUser].onMessage(stanza);
        }
    }

    sendStanza(stanza: xml): void
    {
        log.debug('ContentApp.sendStanza', stanza);
        try {
            chrome.runtime.sendMessage({ 'type': 'sendStanza', 'stanza': stanza });
        } catch (ex) {
            Unbearable.problem();
            // log.error(ex);
        }
    }

    // Window management

    enableScreen(on: boolean): void
    {
        // if (on) {
        //     this.originalScreenHeight = this.screenElem.style.height;
        //     this.screenElem.style.height = '100%';
        // } else {
        //     this.screenElem.style.height = this.originalScreenHeight;
        // }
    }

    // Local storage

    private readonly localStorage_Key_Avatar_x: string = 'Avatar_x';

    savePosition(x: number): void
    {
        Platform.setStorageString(this.localStorage_Key_Avatar_x, as.String(x));
    }

    getSavedPosition(): number
    {
        let value = Platform.getStorageString(this.localStorage_Key_Avatar_x, '');
        if (value == '') {
            return this.getDefaultPosition();
        } else {
            return as.Int(value);
        }
    }

    getDefaultPosition(): number
    {
        return 100 + Math.floor(Math.random() * 500);
    }

}
