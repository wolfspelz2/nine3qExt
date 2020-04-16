const { client, xml } = require('@xmpp/client');
const debug = require('@xmpp/debug');
import { Log } from './Log';

export class Connection
{
  private xmpp: any;
  private myJid: string;
  private roomJid: string;
  private myNick: string;
  private x: number;

  constructor()
  {
    this.xmpp = client({
      service: 'wss://xmpp.dev.sui.li/xmpp-websocket',
      domain: 'xmpp.dev.sui.li',
      resource: 'example',
      username: 'test',
      password: 'testtest',
    });

    this.myJid = 'test@xmpp.dev.sui.li';
    this.roomJid = '2883fcb56d5ac9d5e7adad03a38bce8a362dbdc2@muc4.virtual-presence.org';
    this.myNick = 'nick_';

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
      this.join(this.roomJid, this.myNick, this.x);
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

  onPresence(stanza: any): void
  {
    if (stanza.attrs.from === this.roomJid + '/' + this.myNick)
    {
      this.onJoined(stanza);
    }
  }

  onJoined(stanza: any): void
  {
    Log.info('joined', stanza);
  }

  join(room: string, nick: string, x: number): void
  {
    let presence = xml('presence', { to: room + '/' + nick })
      .append(xml('x', { xmlns: 'firebat:user:identity', jid: this.myJid, src: 'http://example.com/identity/invalid.xml', }))
      .append(xml('x', { xmlns: 'firebat:avatar:state', jid: this.myJid, }).append(xml('position', { x: x }))
      );
    this.xmpp.send(presence);
  }
}
