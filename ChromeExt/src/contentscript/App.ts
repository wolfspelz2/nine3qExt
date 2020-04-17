const { client, xml, jid } = require('@xmpp/client');
const debug = require('@xmpp/debug');
const $ = require('jquery');
import { Log } from './Log';
import { Room } from './Room';

export class App
{
  private display: HTMLElement;
  private xmpp: any;
  private myJid: string = 'test@xmpp.dev.sui.li';
  private myNick: string = 'nick_';
  private rooms: { [id: string]: Room; } = {};

  constructor(private page: HTMLElement)
  {
    this.display = $('<div class="n3q-display" />')[0];
    this.page.append(this.display);

    {
      let controlElem: HTMLElement = $('<div class="n3q-ctrl n3q-hello">Hello World</div>')[0];
      this.display.append(controlElem);

      let enterButton: HTMLButtonElement = $('<button>enter</button>')[0];
      controlElem.append(enterButton);
      $(enterButton).click(() =>
      {
        this.enterRoomByJid('2883fcb56d5ac9d5e7adad03a38bce8a362dbdc2@muc4.virtual-presence.org');
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
      Log.verbose('stanza', stanza);
      if (stanza.is('presence'))
      {
        this.onPresence(stanza);
      }
    });
  }

  start(): void
  {
    this.xmpp.start().catch(Log.error);
  }

  enterRoomByJid(roomJid: string)
  {
    if (typeof this.rooms[roomJid] === typeof undefined)
    {
      let newRoom = new Room(this, this.display, roomJid, this.myJid, this.myNick);
      this.rooms[roomJid] = newRoom;
      newRoom.enter();
    }
  }

  onPresence(stanza: any): void
  {
    let from = jid(stanza.attrs.from);
    let roomOrUser = from.bare();

    if (typeof this.rooms[roomOrUser] != typeof undefined)
    {
      this.rooms[roomOrUser].onPresence(stanza);
    }
  }

  send(stanza: any)
  {
    this.xmpp.send(stanza);
  }

  getAssetUrl(filePath: string)
  {
    return chrome.runtime.getURL('assets/' + filePath);
  }
}
