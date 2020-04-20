const { client, xml, jid } = require('@xmpp/client');
const debug = require('@xmpp/debug');
const $ = require('jquery');
import { as } from './as';
import { Platform } from './Platform';
import { Log } from './Log';
import { Room } from './Room';
import { PropertyStorage } from './PropertyStorage';
import { HelloWorld } from './HelloWorld';

export class App
{
    private display: HTMLElement;
    private xmpp: any;
    private myJid: string = 'test@xmpp.dev.sui.li';
    private myNick: string = 'nick_';
    private rooms: { [id: string]: Room; } = {};
    private storage: PropertyStorage = new PropertyStorage();

    // Getter

    getStorage(): PropertyStorage { return this.storage; }
    getAssetUrl(filePath: string) { return Platform.getAssetUrl(filePath); }

    constructor(private page: HTMLElement)
    {
        this.display = $('<div class="n3q-display" />')[0];
        this.page.append(this.display);

        {
            let controlElem: HTMLElement = $('<div id="n3q-hello" class="n3q-ctrl"></div>')[0];
            this.display.append(controlElem);

            $('#n3q-hello').text(HelloWorld.getText());

            let enterButton: HTMLButtonElement = $('<button>enter</button>')[0];
            controlElem.append(enterButton);
            $(enterButton).click(() =>
            {
                this.enterRoomByJid('d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
            });
        }

        this.xmpp = client({
            service: 'wss://xmpp.dev.sui.li/xmpp-websocket',
            domain: 'xmpp.dev.sui.li',
            resource: 'example',
            username: 'test',
            password: 'testtest',
        });

        this.xmpp.on('error', (err: any) =>
        {
            Log.error(err);
        });

        this.xmpp.on('offline', () =>
        {
            Log.info('offline');
        });

        this.xmpp.on('online', async (address: any) =>
        {
            Log.info('online', address);
        });

        this.xmpp.on('stanza', (stanza: any) =>
        {
            Log.verbose(stanza.name, stanza.attrs.type, stanza);
            if (stanza.is('presence')) {
                this.onPresence(stanza);
            }
        });
    }

    // Connection

    start(): void
    {
        this.xmpp.start().catch(Log.error);
    }

    enterRoomByJid(roomJid: string)
    {
        if (typeof this.rooms[roomJid] === typeof undefined) {
            this.rooms[roomJid] = new Room(this, this.display, roomJid, this.myJid, this.myNick);
        }
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

    send(stanza: any)
    {
        this.xmpp.send(stanza);
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
