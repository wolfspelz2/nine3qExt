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

    //#region presence

    enter(): void
    {
        this.enterRetryCount = 0;
        this.sendPresence();
    }

    /*
    <x xmlns='http://jabber.org/protocol/muc'>
        <history maxchars='65000'/>
        <history maxstanzas='20'/>
        <history seconds='180'/>
    </x>
    */
    sendPresence(): void
    {
        let presence = xml('presence', { to: this.jid + '/' + this.nick })
            .append(
                xml('x', { xmlns: 'firebat:user:identity', id: 'id:n3q:test', jid: this.userJid, src: 'https://storage.zweitgeist.com/index.php/12344151', digest: 'bf167285ccfec3cd3f0141e6de77fed1418fcbae' }))
            .append(
                xml('x', { xmlns: 'firebat:avatar:state', jid: this.userJid, })
                    .append(xml('position', { x: this.x }))
            )
            .append(
                xml('x', { xmlns: 'http://jabber.org/protocol/muc' })
                    .append(xml('history', { seconds: '60', maxchars: '1000', maxstanzas: '1' }))
            )
            ;
        this.app.send(presence);
    }

    onPresence(stanza: any): void
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

    reEnterDifferentNick(): void
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

    //#endregion
    //#region message

    onMessage(stanza: any)
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

    /*
    <message
        from='hag66@shakespeare.lit/pda'
        id='hysf1v37'
        to='coven@chat.shakespeare.lit'
        type='groupchat'>
      <body>Harpier cries: 'tis time, 'tis time.</body>
    </message>
    */
    sendGroupChat(text: string, fromNick: string)
    {
        let message = xml('message', { type: 'groupchat', to: this.jid, from: this.jid + '/' + fromNick })
            .append(xml('body', {}, text))
            ;
        this.app.send(message);
    }

    //#endregion
    //#region Send stuff

    sendMoveMessage(newX: number): void
    {
        this.x = newX;
        this.sendPresence();
    }

    //#endregion
}
