const { client, xml, jid } = require('@xmpp/client');
const debug = require('@xmpp/debug');
import { Log } from './Log';
import { Room } from './Room';

export class Connection
{
  private xmpp: any;
  private myJid: string = 'test@xmpp.dev.sui.li';
  private myNick: string = 'nick_';
  private rooms: { [id: string]: Room; } = {};

  constructor()
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
      let newRoom = new Room(this, roomJid, this.myJid, this.myNick);
      this.rooms[roomJid] = newRoom;
      newRoom.enter();
    }
  }

  onPresence(stanza: any): void
  {
    let from = jid(stanza.attrs.from);
    let  roomOrUser = from.bare();

    if (typeof this.rooms[roomOrUser] != typeof undefined)
    {
      this.rooms[roomOrUser].onPresence(stanza);
    }
  }

  send(stanza: any)
  {
    this.xmpp.send(stanza);
  }
}
