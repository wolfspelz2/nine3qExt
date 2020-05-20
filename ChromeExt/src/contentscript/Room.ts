import log = require('loglevel');
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { Panic } from '../lib/Panic';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';
import { ChatWindow } from './ChatWindow';
import { stringify } from 'querystring';

export interface IRoomInfoLine extends Array<string | string> { 0: string, 1: string }
export interface IRoomInfo extends Array<IRoomInfoLine> { }

export class Room
{
    private userJid: string;
    private nickname: string = '';
    private avatar: string = '';
    private enterRetryCount: number = 0;
    private maxEnterRetries: number = as.Int(Config.get('xmpp.maxMucEnterRetries', 4));
    private participants: { [nick: string]: Participant; } = {};
    private isEntered = false; // iAmAlreadyHere() needs isEntered=true to be after onPresenceAvailable
    private chatWindow: ChatWindow;
    private myNick: any;

    constructor(private app: ContentApp, private display: HTMLElement, private jid: string, private posX: number) 
    {
        let user = Config.get('xmpp.user', Utils.randomString(0));
        let domain = Config.get('xmpp.domain', '');
        if (domain == '') {
            Panic.now();
        }
        this.userJid = user + '@' + domain;

        this.chatWindow = new ChatWindow(app, display, this);

        // this.participants['dummy'] = new Participant(this.app, this, this.display, 'dummy', false);
        // this.participants['dummy'].onPresenceAvailable(xml('presence', { from: jid + '/dummy' }).append(xml('x', { xmlns: 'firebat:avatar:state' }).append(xml('position', { x: 100 }))));
    }

    getInfo(): IRoomInfo
    {
        return [
            ['jid', this.jid]
        ];
    }

    iAmAlreadyHere()
    {
        return this.isEntered;
    }
    
    // presence

    async enter(): Promise<void>
    {
        try {
            this.nickname = await this.app.getUserNickname();
            this.avatar = await this.app.getUserAvatar();
        } catch (error) {
            log.info(error);
            this.nickname = 'new-user';
            this.avatar = '004/pinguin';
        }

        this.enterRetryCount = 0;
        this.sendPresence();
    }

    leave(): void
    {
        this.sendPresenceUnavailable();
        this.removeAllParticipants();
        this.kill();
    }

    kill()
    {
        this.stopKeepAlive();
    }

    private async sendPresence(): Promise<void>
    {
        let identityUrl = Config.get('identity.url', '');
        let identityDigest = Config.get('identity.digest', '1');
        if (identityUrl == '') {
            let avatarUrl = as.String(Config.get('avatars.animationsUrlTemplate', 'http://avatar.zweitgeist.com/gif/{id}/config.xml')).replace('{id}', this.avatar);
            identityDigest = as.String(Utils.hash(this.nickname + avatarUrl));
            identityUrl = as.String(Config.get('identity.identificatorUrlTemplate', 'https://avatar.weblin.sui.li/identity/?nickname={nickname}&avatarUrl={avatarUrl}&digest={digest}'))
                .replace('{nickname}', encodeURIComponent(this.nickname))
                .replace('{avatarUrl}', encodeURIComponent(avatarUrl))
                .replace('{digest}', encodeURIComponent(identityDigest))
                ;
        }

        let presence = xml('presence', { to: this.jid + '/' + this.nickname })
            .append(
                xml('x', { xmlns: 'vp:props', nickname: this.nickname, avatar: this.avatar }))
            .append(
                xml('x', { xmlns: 'firebat:avatar:state', })
                    .append(xml('position', { x: as.Int(this.posX) }))
            )
            ;

        if (identityUrl != '') {
            presence.append(
                xml('x', { xmlns: 'firebat:user:identity', 'jid': this.userJid, 'src': identityUrl, 'digest': identityDigest })
            );
        }

        if (!this.isEntered) {
            presence.append(
                xml('x', { xmlns: 'http://jabber.org/protocol/muc' })
                    .append(xml('history', { seconds: '180', maxchars: '3000', maxstanzas: '10' }))
            );
        }

        this.app.sendStanza(presence);
    }

    private sendPresenceUnavailable(): void
    {
        let presence = xml('presence', { type: 'unavailable', to: this.jid + '/' + this.nickname });

        this.app.sendStanza(presence);
    }

    onPresence(stanza: any): void
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
                if (this.participants[nick] === undefined) {
                    this.participants[nick] = new Participant(this.app, this, this.display, nick, isSelf);
                }
                this.participants[nick].onPresenceAvailable(stanza);

                if (isSelf && !this.isEntered) {
                    this.isEntered = true;
                    this.myNick = nick;
                    this.keepAlive();
                }
                break;

            case 'unavailable':
                if (this.participants[nick] != undefined) {
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
            log.info('Too many retries ', this.enterRetryCount, 'giving up on room', this.jid);
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

    private keepAliveSec: number = Config.get('room.keepAliveSec', 180);
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

    private stopKeepAlive()
    {
        if (this.keepAliveTimer != undefined) {
            clearTimeout(this.keepAliveTimer);
            this.keepAliveTimer = undefined;
        }
    }

    // message

    onMessage(stanza: any)
    {
        let from = jid(stanza.attrs.from);
        let nick = from.getResource();
        let type = as.String(stanza.attrs.type, 'groupchat');

        switch (type) {
            case 'groupchat':
                if (this.participants[nick] != undefined) {
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
    sendGroupChat(text: string)
    {
        let message = xml('message', { type: 'groupchat', to: this.jid, from: this.jid + '/' + this.myNick })
            .append(xml('body', {}, text))
            ;
        this.app.sendStanza(message);
    }

    showChatWindow(aboveElem: HTMLElement): void
    {
        if (this.chatWindow) {
            if (!this.chatWindow.isOpen()) {
                this.chatWindow.show({ 'above': aboveElem });
            }
        }
    }

    toggleChatWindow(relativeToElem: HTMLElement): void
    {
        if (this.chatWindow) {
            if (this.chatWindow.isOpen()) {
                this.chatWindow.close();
            } else {
                this.showChatWindow(relativeToElem);
            }
        }
    }

    showChatMessage(nick: string, text: string)
    {
        this.chatWindow.addLine(nick + Date.now(), nick, text);
    }

    sendMoveMessage(newX: number): void
    {
        this.posX = newX;
        this.sendPresence();
    }
}
