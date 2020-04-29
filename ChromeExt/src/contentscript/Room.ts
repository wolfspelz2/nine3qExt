import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { Panic } from '../lib/Panic';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';

export class Room
{
    private userJid: string;
    private nickname: string = Config.get('nickname', 'user' + Utils.randomString(3));
    private enterRetryCount: number = 0;
    private maxEnterRetries: number = as.Int(Config.get('maxMucEnterRetries', 4));
    private posX: number = Utils.randomInt(as.Int(Config.get('roomEnterPosXMin', 400)), as.Int(Config.get('roomEnterPosXMax', 700)));
    private participants: { [nick: string]: Participant; } = {};
    private isEntered = false;

    constructor(private app: ContentApp, private display: HTMLElement, private jid: string) 
    {
        let user = Config.get('xmpp.user', Utils.randomString(0));
        let domain = Config.get('xmpp.domain', '');
        if (domain == '') {
            Panic.now();
        }
        this.userJid = user + '@' + domain;

        // this.participants['dummy'] = new Participant(this.app, this, this.display, 'dummy', false);
        // this.participants['dummy'].onPresenceAvailable(xml('presence', { from: jid + '/dummy' }).append(xml('x', { xmlns: 'firebat:avatar:state' }).append(xml('position', { x: 100 }))));
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
        let presence = xml('presence', { to: this.jid + '/' + this.nickname })
            .append(
                xml('x', { xmlns: 'vp:props', nickname: Config.get('nickname', 'new-user'), avatar: Config.get('avatar', '002/sportive03_m') }))
            .append(
                xml('x', { xmlns: 'firebat:user:identity', jid: this.userJid, src: 'https://storage.zweitgeist.com/index.php/12344151', digest: 'bf167285ccfec3cd3f0141e6de77fed1418fcbae' }))
            .append(
                xml('x', { xmlns: 'firebat:avatar:state', jid: this.userJid, })
                    .append(xml('position', { x: this.posX }))
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
        let presence = xml('presence', { type: 'unavailable', to: this.jid + '/' + this.nickname });

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

        let isSelf = nick == this.nickname;

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
            this.nickname = this.getNextNick(this.nickname);
            this.sendPresence();
        }
    }

    private getNextNick(nick: string): string
    {
        return nick + '_';
    }

    private removeAllParticipants()
    {
        let nicks = Array<string>();
        for (let nick in this.participants) {
            nicks.push(nick);
        }
        nicks.forEach(nick =>
        {
            this.participants[nick].remove();
        });
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
        this.posX = newX;
        this.sendPresence();
    }
}
