import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { Nickname } from './Nickname';
import { Chatout } from './Chatout';
import { Chatin } from './Chatin';

export class Participant extends Entity
{
    private avatarDisplay: Avatar;
    private nicknameDisplay: Nickname;
    private chatoutDisplay: Chatout;
    private chatinDisplay: Chatin;
    private isFirstPresence: boolean = true;
    private defaultSpeedPixelPerSec: number = as.Float(Config.get('room.defaultAvatarSpeedPixelPerSec', 100));
    private userId: string;
    private inMove: boolean = false;
    private condition_: string = '';

    constructor(private app: ContentApp, room: Room, display: HTMLElement, private nick: string, private isSelf: boolean)
    {
        super(room, display);

        $(this.getElem()).addClass('n3q-participant');
        if (isSelf) {
            $(this.getElem()).addClass('n3q-participant-self');
        }
        else {
            $(this.getElem()).addClass('n3q-participant-other');
        }
    }

    remove(): void
    {
        this.avatarDisplay?.stop();
        this.nicknameDisplay?.stop();
        this.chatoutDisplay?.stop();
        this.chatinDisplay?.stop();
        super.remove();
    }

    // presence

    async onPresenceAvailable(stanza: any): Promise<void>
    {
        let presenceHasPosition: boolean = false;
        let newX: number = 123;
        let newCondition: string = '';
        let xmppNickname = '';
        let vpNickname = '';
        let vpAvatar = '';
        let hasIdentityUrl = false;

        {
            let from = stanza.attrs.from
            if (from != undefined) {
                let fromJid = new jid(from);
                let nickname = as.String(fromJid.getResource(), '');
                if (nickname != '') {
                    xmppNickname = nickname;
                }
            }
        }

        {
            let stateNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'firebat:avatar:state');
            if (stateNode != null) {
                let positionNode = stateNode.getChild('position');
                if (positionNode != undefined) {
                    newX = as.Int(positionNode.attrs.x, -1);
                    if (newX != -1) {
                        presenceHasPosition = true;
                    }
                }
                let conditionNode = stateNode.getChild('condition');
                if (conditionNode != undefined) {
                    newCondition = as.String(conditionNode.attrs.status, '');
                }
            }
        }

        {
            let identityNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'firebat:user:identity');
            if (identityNode != null) {
                let attrs = identityNode.attrs;
                let url = as.String(attrs.src, '');
                let digest = as.String(attrs.digest, '');
                let jid = as.String(attrs.jid, url);
                this.userId = as.String(attrs.id, jid);

                if (url != '') {
                    hasIdentityUrl = true;
                    this.app.getPropertyStorage().setIdentity(this.userId, url, digest);
                }
            }
        }

        {
            let vpNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
            if (vpNode != null) {
                let attrs = vpNode.attrs;
                let nickname = as.String(attrs.nickname, '');
                if (nickname != '') { vpNickname = nickname; }
                let avatar = as.String(attrs.avatar, '');
                if (avatar != '') { vpAvatar = avatar; }
            }
        }

        { // <show>: dnd, away, xa
            let showAvailability: string = 'available';
            let showNode = stanza.getChild('show');
            if (showNode != null) {
                showAvailability = showNode.getText();
                switch (showAvailability) {
                    case 'chat': newCondition = ''; break;
                    case 'available': newCondition = ''; break;
                    case 'away': newCondition = 'sleep'; break;
                    case 'dnd': newCondition = 'sleep'; break;
                    case 'xa': newCondition = 'sleep'; break;
                    default: break;
                }
            }
        }

        { // <status>: Status message (text)
            let statusMessage: string = '';
            let statusNode = stanza.getChild('status');
            if (statusNode != undefined) {
                statusMessage = statusNode.getText();
            }
        }

        if (this.isFirstPresence) {
            this.isFirstPresence = false;

            if (!presenceHasPosition) {
                newX = this.isSelf ? await this.app.getSavedPosition() : this.app.getDefaultPosition();
            }
            if (newX < 0) { newX = 100; }
            this.setPosition(newX);

            {
                this.avatarDisplay = new Avatar(this.app, this, this.getCenterElem(), this.isSelf);
                if (hasIdentityUrl) {
                    //this.app.getStorage().watch(this.userId, 'ImageUrl', this.avatarDisplay);
                    this.app.getPropertyStorage().watch(this.userId, 'AnimationsUrl', this.avatarDisplay);
                } else {
                    if (vpAvatar != '') {
                        let animationsUrl = as.String(Config.get('avatars.animationsUrlTemplate', 'http://avatar.zweitgeist.com/gif/{id}/config.xml')).replace('{id}', vpAvatar);
                        let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://avatar.weblin.sui.li/avatar/?url={url}')).replace('{url}', encodeURIComponent(animationsUrl));
                        this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
                    }
                }
            }

            {
                this.nicknameDisplay = new Nickname(this.app, this, this.isSelf, this.getElem());
                var shownNickname = xmppNickname;
                if (hasIdentityUrl) {
                    this.app.getPropertyStorage().watch(this.userId, 'Nickname', this.nicknameDisplay);
                } else {
                    if (vpNickname != '') {
                        shownNickname = vpNickname;
                    }
                }
                this.nicknameDisplay?.setNickname(shownNickname);
            }

            this.chatoutDisplay = new Chatout(this.app, this, this.getElem());

            if (this.isSelf) {
                this.chatinDisplay = new Chatin(this.app, this, this.getElem());
            }

            this.show(true);
            // if (this.isSelf) { this.showChatWindow(); }
            this.room?.showChatMessage(this.nick, 'entered the room');

        } else {

            if (presenceHasPosition) {
                if (this.getPosition() != newX) {
                    this.move(newX);
                }
            }
        }

        this.condition_ = newCondition;
        if (!this.inMove) {
            this.avatarDisplay?.setState(this.condition_);
        }
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();

        this.room?.showChatMessage(this.nick, 'left the room');
    }

    // message

    onMessageGroupchat(stanza: any): void
    {
        let now = Date.now();
        let timestamp = 0;

        {
            let node = stanza.getChildren('delay').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'urn:xmpp:delay');
            if (node != undefined) {
                let dateStr = as.String(node.attrs.stamp, ''); // 2020-04-24T06:53:46Z
                if (dateStr != '') {
                    try {
                        var date = new Date(dateStr);
                        let time = date.getTime();
                        if (!isNaN(time)) {
                            timestamp = time;
                        }
                    } catch (error) {
                        //
                    }
                }
            }
        }

        {
            let node = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'jabber:x:delay');
            if (node != undefined) {
                let dateStr = as.String(node.attrs.stamp, ''); // 20200424T06:53:46
                try {
                    var date = new Date(dateStr);
                    let time = date.getTime();
                    if (!isNaN(time)) {
                        timestamp = time;
                    }
                } catch (error) {
                    //
                }
            }
        }

        let text = '';
        let nick = '';

        let bodyNode = stanza.getChild('body');
        if (bodyNode != undefined) {
            text = bodyNode.getText();
        }

        if (stanza.attrs != undefined) {
            let from = jid(stanza.attrs.from);
            let room = from.bare().toString();
            nick = from.getResource();
            if (this.nicknameDisplay != undefined) {
                nick = this.nicknameDisplay?.getNickname();
            }
        }

        if (text == '') { return; }

        if (timestamp == 0) {
            timestamp = now;
        }
        let delayMSec = now - timestamp;

        // always
        this.room?.showChatMessage(nick, text);

        // recent
        if (delayMSec * 1000 < as.Float(Config.get('room.maxChatAgeSec', 60))) {
            if (!this.isChatCommand(text)) {
                this.chatoutDisplay?.setText(text);
            }
        }

        // new only
        if (delayMSec <= 100) {
            this.avatarDisplay?.setAction('chat');
            if (this.isChatCommand(text)) {
                return this.onChatCommand(text);
            }
        }
    }

    isChatCommand(text: string) { return text.substring(0, 1) == '/'; }

    onChatCommand(text: string): void
    {
        var parts: string[] = text.split(' ');
        if (parts.length < 1) { return; }
        var cmd: string = parts[0];

        switch (cmd) {
            case '/do':
                if (parts.length < 2) { return; }
                // this.chatoutDisplay?.setText(text);
                this.avatarDisplay?.setAction(parts[1]);
                break;
        }
    }

    // /do WaterBottle ApplyTo WaterCan
    chat_command_apply: string = '/action';
    sendGroupChat(text: string, handler?: (IMessage: any) => any): void
    {
        //hw later
        // if (text.substr(0, this.chat_command_apply.length) == this.chat_command_apply) {
        //     var parts = text.split(' ');
        //     if (parts.length == 4) {
        //         var activeName = parts[1];
        //         var action = parts[2];
        //         var passiveName = parts[3];
        //         var activeId: string = '';
        //         var passiveId: string = '';
        //         var activeUndecided: boolean = false;
        //         var passiveUndecided: boolean = false;
        //         for (var id in this.things) {
        //             if (this.things[id].isIdentifiedBy(activeName)) {
        //                 activeUndecided = (activeId != '');
        //                 if (!activeUndecided) {
        //                     activeId = id;
        //                 }
        //                 break;
        //             }
        //         }
        //         for (var id in this.things) {
        //             if (this.things[id].isIdentifiedBy(passiveName)) {
        //                 passiveUndecided = (passiveId != '');
        //                 if (!passiveUndecided) {
        //                     passiveId = id;
        //                 }
        //                 break;
        //             }
        //         }
        //         if (activeUndecided || passiveUndecided) {
        //             new SimpleNotice(this, 'ActionAmbiguous', 10, 'glyphicon glyphicon-ban-circle', 'Not Executed', (activeUndecided ? activeName : passiveName) + ' is ambiguous');
        //         } else {
        //             if (activeId != '' && passiveId != '') {
        //                 this.sendItemActionMessage(this.getRoomName(), activeId, action, { Item: passiveId });
        //             }
        //         }
        //     }
        // }

        this.room?.sendGroupChat(text);
    }

    // drag/move

    move(newX: number): void
    {
        this.inMove = true;

        if (newX < 0) { newX = 0; }

        this.setPosition(this.getPosition());

        var oldX = this.getPosition();
        var diffX = newX - oldX;
        if (diffX < 0) {
            diffX = -diffX;
            this.avatarDisplay?.setState('moveleft');
        } else {
            this.avatarDisplay?.setState('moveright');
        }

        let speedPixelPerSec = as.Float(this.avatarDisplay?.getSpeedPixelPerSec(), this.defaultSpeedPixelPerSec);
        var durationSec = diffX / speedPixelPerSec;

        $(this.getElem())
            .stop(true)
            .animate(
                { left: newX + 'px' },
                durationSec * 1000,
                'linear',
                () => this.onMoveDestinationReached(newX)
            );
    }

    onMoveDestinationReached(newX: number): void
    {
        this.inMove = false;
        this.setPosition(newX);
        this.avatarDisplay?.setState(this.condition_);
    }

    onDraggedBy(dX: number, dY: number): void
    {
        var newX = this.getPosition() + dX;

        if (this.getPosition() != newX) {
            if (this.isSelf) {
                this.app.savePosition(newX);
                this.room?.sendMoveMessage(newX);
            } else {
                this.quickSlide(newX);
            }
        }
    }

    quickSlide(newX: number): void
    {
        this.avatarDisplay?.setState('');
        super.quickSlide(newX);
    }

    // Mouse

    select(): void
    {
        //$(this.elem).siblings().zIndex(1);
        //$(this.elem).zIndex(100);
    }

    onMouseEnterAvatar(ev: JQuery.Event): void
    {
        this.avatarDisplay?.hilite(true);
        //this.nickname.setVisible(true);
    }

    onMouseLeaveAvatar(ev: JQuery.Event): void
    {
        this.avatarDisplay?.hilite(false);
        //this.nickname.setVisible(false);
    }

    onMouseClickAvatar(ev: JQuery.Event): void
    {
        this.select()
        this.toggleChatin();
    }

    onMouseDoubleClickAvatar(ev: JQuery.Event): void
    {
        this.toggleChatWindow();
    }

    do(what: string): void
    {
        this.room?.sendGroupChat('/do ' + what);
    }

    toggleChatin(): void
    {
        this.chatinDisplay?.toggleVisibility();
    }

    toggleChatout(): void
    {
        this.chatoutDisplay?.toggleVisibility();
    }

    toggleChatWindow(): void
    {
        this.room?.toggleChatWindow(this.getElem());
    }

    showChatWindow(): void
    {
        this.room?.showChatWindow(this.getElem());
    }
}
