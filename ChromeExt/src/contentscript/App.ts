const { client, xml, jid } = require('@xmpp/client');
// import { client, xml, jid } from '@xmpp/client';
const debug = require('@xmpp/debug');
const $ = require('jquery');
import { as } from './as';
import { Platform } from './Platform';
import { Log } from './Log';
import { Room } from './Room';
import { PropertyStorage } from './PropertyStorage';
import { HelloWorld } from './HelloWorld';

interface ILocationMapperResponse
{
    //    sMessage: string;
    sLocationURL: string;
}

export class App
{
    private display: HTMLElement;
    private xmpp: any;
    private myJid: string = 'test@xmpp.dev.sui.li';
    private myNick: string = 'nick_';
    private rooms: { [id: string]: Room; } = {};
    private storage: PropertyStorage = new PropertyStorage();
    private keepAliveSec: number = 180;

    // Getter

    getStorage(): PropertyStorage { return this.storage; }
    getAssetUrl(filePath: string) { return Platform.getAssetUrl(filePath); }

    constructor(private page: HTMLElement)
    {
        // this.display = $('<div class="n3q-base n3q-display" />')[0];
        // this.page.append(this.display);
        this.display = page;

        this.createPageControl();
    }

    createPageControl()
    {
        let controlElem: HTMLElement = $('<div class="n3q-base n3q-ctrl" id="n3q-hello"></div>')[0];
        this.display.append(controlElem);

        $('#n3q-hello').text(HelloWorld.getText());

        let enterButton: HTMLButtonElement = $('<button class="n3q-base">enter</button>')[0];
        controlElem.append(enterButton);
        $(enterButton).click(() =>
        {
            // this.enterRoomByJid('d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
            // this.enterRoomByPageUrl('https://www.galactic-developments.de/');
            this.enterPage();
        });
    }

    // Connection

    start(): void
    {
        this.xmpp = client({
            service: 'wss://xmpp.dev.sui.li/xmpp-websocket',
            domain: 'xmpp.dev.sui.li',
            resource: 'example',
            username: 'test',
            password: 'testtest',
        });

        this.xmpp.on('error', (err: any) =>
        {
            Log.error('App.error', err);
        });

        this.xmpp.on('offline', () =>
        {
            Log.info('App.offline');
        });

        this.xmpp.on('online', async (address: any) =>
        {
            Log.info('App.online', address);
            this.sendPresence();
            this.keepAlive();
            this.enterPage();
        });

        this.xmpp.on('stanza', (stanza: any) =>
        {
            Log.info('App.recv', stanza);
            if (stanza.is('presence')) {
                this.onPresence(stanza);
            } else if (stanza.is('message')) {
                this.onMessage(stanza);
            }

        });

        this.xmpp.start().catch(Log.error);
    }

    enterPage()
    {
        this.enterRoomByPageUrl(Platform.getCurrentPageUrl());
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
                    Log.info('Mapped', pageUrl, ' => ', locationUrl);
                    let roomJid = App.getRoomJidFromLocationUrl(locationUrl);
                    this.enterRoomByJid(roomJid);
                } catch (ex) {
                    Log.error(ex);
                }
            }
        });
    }

    enterRoomByJid(roomJid: string): void
    {
        if (typeof this.rooms[roomJid] === typeof undefined) {
            this.rooms[roomJid] = new Room(this, this.display, roomJid, this.myJid, this.myNick);
        }
        Log.info('enterRoomByJid', roomJid);
        this.rooms[roomJid].enter();
    }

    onPresence(stanza: any): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (typeof this.rooms[roomOrUser] != typeof undefined) {
            this.rooms[roomOrUser].onPresence(stanza);
        }
    }

    onMessage(stanza: any): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (typeof this.rooms[roomOrUser] != typeof undefined) {
            this.rooms[roomOrUser].onMessage(stanza);
        }
    }

    private keepAliveTimer: number = undefined;
    keepAlive()
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

    send(stanza: any): void
    {
        Log.info('App.send', stanza);
        this.xmpp.send(stanza);
    }

    sendPresence()
    {
        this.send(xml('presence'));
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
