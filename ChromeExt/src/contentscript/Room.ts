const { xml, jid } = require('@xmpp/client');
import { Log } from './Log';
import { App } from './App';
import { Participant } from './Participant';

export class Room
{
  private nick: string;
  private x: number = 300;
  private participants: { [id: string]: Participant; } = {};

  constructor(private app: App, private display: HTMLElement, private jid: string, private userJid: string, private proposedNick: string) 
  {
    this.nick = this.proposedNick; 
  }

  enter(): void
  {
    this.sendPresence();
  }

  sendPresence(): void
  {
    let presence = xml('presence', { to: this.jid + '/' + this.nick })
      .append(xml('x', { xmlns: 'firebat:user:identity', jid: this.userJid, src: 'http://example.com/identity/invalid.xml' }))
      .append(xml('x', { xmlns: 'firebat:avatar:state', jid: this.userJid, }).append(xml('position', { x: this.x }))
      );
    this.app.send(presence);
  }

  onPresence(stanza: any)
  {
    let from = jid(stanza.attrs.from);
    let nick = from.getResource();

    if (typeof this.participants[nick] === typeof undefined)
    {
      let newParticipant = new Participant(this.app, this, this.display, nick, nick == this.nick);
      this.participants[nick] = newParticipant;
      newParticipant.onPresence(stanza);
    }
  }
}
