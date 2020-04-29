import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';

export class Room
{
    private nick: string;
    private enterRetryCount: number = 0;
    private maxEnterRetries: number = 4;
    private x: number = 200;
    private participants: { [nick: string]: Participant; } = {};
    private isEntered = false;

    constructor(private app: ContentApp, private display: HTMLElement, private jid: string, private userJid: string, private proposedNick: string) 
    {
        this.nick = this.proposedNick;

        this.participants['dummy'] = new Participant(this.app, this, this.display, 'dummy', false);
        this.participants['dummy'].onPresenceAvailable(xml('presence', { from: jid + '/dummy' }).append(xml('x', { xmlns: 'firebat:avatar:state' }).append(xml('position', { x: 100 }))));
    }

    //#region presence

    public enter(): void
    {
        this.enterRetryCount = 0;
        this.sendPresence();
    }

    public leave(): void
    {
        this.sendPresenceUnavailable();
        this.removeAllParticipants();
    }
    
    private sendPresence(): void
    {
        let presence = xml('presence', { to: this.jid + '/' + this.nick })
            .append(
                xml('x', { xmlns: 'firebat:user:identity', id: 'id:n3q:test', jid: this.userJid, src: 'https://storage.zweitgeist.com/index.php/12344151', digest: 'bf167285ccfec3cd3f0141e6de77fed1418fcbae' }))
            .append(
                xml('x', { xmlns: 'firebat:avatar:state', jid: this.userJid, })
                    .append(xml('position', { x: this.x }))
            );

        if (!this.isEntered) {
            presence.append(
                xml('x', { xmlns: 'http://jabber.org/protocol/muc' })
                    .append(xml('history', { seconds: '60', maxchars: '1000', maxstanzas: '1' }))
            );
        }

        this.app.sendStanza(presence);
    }

    private sendPresenceUnavailable(): void
    {
        let presence = xml('presence', { type: 'unavailable', to: this.jid + '/' + this.nick });

        this.app.sendStanza(presence);
    }

    public onPresence(stanza: any): void
    {
        let from = jid(stanza.attrs.from);
        let nick = from.getResource();

        let type = as.String(stanza.attrs.type, '');
        if (type == '') {
            type = 'available';
        }

        let isSelf = nick == this.nick;

        switch (type) {
            case 'available':
                if (typeof this.participants[nick] === typeof undefined) {
                    this.participants[nick] = new Participant(this.app, this, this.display, nick, isSelf);
                }
                this.participants[nick].onPresenceAvailable(stanza);

                if (isSelf && !this.isEntered) {
                    this.isEntered = true;
                    this.keepAlive();
                }

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

    private reEnterDifferentNick(): void
    {
        this.enterRetryCount++;
        if (this.enterRetryCount > this.maxEnterRetries) {
            log.error('Too many retries ', this.enterRetryCount, 'giving up on room', this.jid);
            return;
        } else {
            this.nick = this.getNextNick(this.nick);
            this.sendPresence();
        }
    }

    private getNextNick(nick: string): string
    {
        return nick + '_';
    }

    private removeAllParticipants()
    {
    }

    // Keepalive

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

    // message

    public onMessage(stanza: any)
    {
        let from = jid(stanza.attrs.from);
        let nick = from.getResource();
        let type = as.String(stanza.attrs.type, 'groupchat');

        switch (type) {
            case 'groupchat':
                if (typeof this.participants[nick] != typeof undefined) {
                    this.participants[nick].onMessageGroupchat(stanza);
                }
                break;

            case 'error':
                //hw todo
                break;
        }
    }

    // Send stuff

    /*
    <message
        from='hag66@shakespeare.lit/pda'
        id='hysf1v37'
        to='coven@chat.shakespeare.lit'
        type='groupchat'>
      <body>Harpier cries: 'tis time, 'tis time.</body>
    </message>
    */
   public sendGroupChat(text: string, fromNick: string)
    {
        let message = xml('message', { type: 'groupchat', to: this.jid, from: this.jid + '/' + fromNick })
            .append(xml('body', {}, text))
            ;
        this.app.sendStanza(message);
    }

    public sendMoveMessage(newX: number): void
    {
        this.x = newX;
        this.sendPresence();
    }
}
