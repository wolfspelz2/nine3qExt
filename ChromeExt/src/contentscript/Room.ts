const { xml, jid } = require('@xmpp/client');
import { as } from './as';
import { Log } from './Log';
import { App } from './App';
import { Participant } from './Participant';

export class Room
{
    private nick: string;
    private enterRetryCount: number = 0;
    private maxEnterRetries: number = 4;
    private x: number = 300;
    private participants: { [nick: string]: Participant; } = {};

    constructor(private app: App, private display: HTMLElement, private jid: string, private userJid: string, private proposedNick: string) 
    {
        this.nick = this.proposedNick;
    }

    enter(): void
    {
        this.enterRetryCount = 0;
        this.sendPresence();
    }

    sendPresence(): void
    {
        let presence = xml('presence', { to: this.jid + '/' + this.nick })
            .append(xml('x', { xmlns: 'firebat:user:identity', id: 'id:n3q:test', jid: this.userJid, src: 'https://storage.zweitgeist.com/index.php/12344151', digest: 'bf167285ccfec3cd3f0141e6de77fed1418fcbae' }))
            .append(xml('x', { xmlns: 'firebat:avatar:state', jid: this.userJid, }).append(xml('position', { x: this.x }))
            );
        this.app.send(presence);
    }

    onPresence(stanza: any)
    {
        let from = jid(stanza.attrs.from);
        let nick = from.getResource();

        let type = as.String(stanza.attrs.type, '');
        if (type == '') {
            type = 'available';
        }

        switch (type) {
            case 'available':
                if (typeof this.participants[nick] === typeof undefined) {
                    this.participants[nick] = new Participant(this.app, this, this.display, nick, nick == this.nick);
                }
                this.participants[nick].onPresenceAvailable(stanza);
                break;

            case 'unavailable':
                if (typeof this.participants[nick] != typeof undefined) {
                    this.participants[nick].onPresenceUnavailable(stanza);
                    delete this.participants[nick];
                }
                break;

            case 'error':
                let code = as.Int(stanza.getChildren('error')[0].attrs.code, -1);
                if (code == 409) {
                    this.reEnterDifferentNick();
                }
                break;
        }
    }

    reEnterDifferentNick()
    {
        this.enterRetryCount++;
        if (this.enterRetryCount > this.maxEnterRetries) {
            Log.error('Too many retries ', this.enterRetryCount, 'giving up on room', this.jid);
            return;
        } else {
            this.nick = this.getNextNick(this.nick);
            this.sendPresence();
        }
    }

    getNextNick(nick: string): string
    {
        return nick + '_';
    }

    sendMoveMessage(newX: number): void
    {
        this.x = newX;
        this.sendPresence();
    }
}
