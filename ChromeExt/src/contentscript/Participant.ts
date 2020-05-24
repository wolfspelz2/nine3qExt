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
    private nicknameDisplay: Nickname;
    private chatoutDisplay: Chatout;
    private chatinDisplay: Chatin;
    private isFirstPresence: boolean = true;
    private userId: string;

    constructor(app: ContentApp, room: Room, display: HTMLElement, private nick: string, isSelf: boolean)
    {
        super(app, room, display, isSelf);

        $(this.getElem()).addClass('n3q-participant');
        if (isSelf) {
            $(this.getElem()).addClass('n3q-participant-self');
        }
        else {
            $(this.getElem()).addClass('n3q-participant-other');
        }
    }

    getChatout(): Chatout { return this.chatoutDisplay; }

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

        let presenceHasCondition: boolean = false;
        let newCondition: string = '';

        let xmppNickname = '';

        let vpNickname = '';
        let vpAvatarId = '';
        let vpAnimationsUrl = '';
        let vpImageUrl = '';

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
            if (stateNode) {
                let positionNode = stateNode.getChild('position');
                if (positionNode) {
                    newX = as.Int(positionNode.attrs.x, -1);
                    if (newX != -1) {
                        presenceHasPosition = true;
                    }
                }
                presenceHasCondition = true;
                let conditionNode = stateNode.getChild('condition');
                if (conditionNode) {
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
            let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
            if (vpPropsNode) {
                let attrs = vpPropsNode.attrs;
                if (attrs) {
                    vpNickname = as.String(attrs.Nickname, '');
                    if (vpNickname == '') { vpNickname = as.String(attrs.nickname, ''); }
                    vpAvatarId = as.String(attrs.AvatarId, '');
                    if (vpAvatarId == '') { vpAvatarId = as.String(attrs.avatar, ''); }
                    vpAnimationsUrl = as.String(attrs.AnimationsUrl, '');
                    vpImageUrl = as.String(attrs.ImageUrl, '');
                }
            }
        }

        { // <show>: dnd, away, xa
            let showAvailability: string = 'available';
            let showNode = stanza.getChild('show');
            if (showNode != null) {
                showAvailability = showNode.getText();
                switch (showAvailability) {
                    case 'chat': newCondition = ''; presenceHasCondition = true; break;
                    case 'available': newCondition = ''; presenceHasCondition = true; break;
                    case 'away': newCondition = 'sleep'; presenceHasCondition = true; break;
                    case 'dnd': newCondition = 'sleep'; presenceHasCondition = true; break;
                    case 'xa': newCondition = 'sleep'; presenceHasCondition = true; break;
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

        // hasIdentityUrl = false;
        // vpAvatar = '004/pinguin'; 
        // vpAvatar = ''; 
        // vpAnimationsUrl = 'https://weblin-avatar.dev.sui.li/items/baum/avatar.xml';
        // vpAnimationsUrl = '';
        // vpImageUrl = 'https://weblin-avatar.dev.sui.li/items/baum/idle.png';
        // vpImageUrl = '';

        if (this.isFirstPresence) {
            this.avatarDisplay = new Avatar(this.app, this, this.getCenterElem(), this.isSelf);

            this.nicknameDisplay = new Nickname(this.app, this, this.isSelf, this.getElem());
            if (!this.isSelf) {
                if (Config.get('room.nicknameOnHover', true)) {
                    let nicknameElem = this.nicknameDisplay.getElem();
                    nicknameElem.style.display = 'none';
                    $(this.getElem()).hover(function ()
                    {
                        $(this).find(nicknameElem).stop().fadeIn('fast');
                    }, function ()
                    {
                        $(this).find(nicknameElem).stop().fadeOut();
                    });
                }
            }

            this.chatoutDisplay = new Chatout(this.app, this, this.getElem());

            if (this.isSelf) {
                this.chatinDisplay = new Chatin(this.app, this, this.getElem());
            }
        }

        if (this.avatarDisplay) {
            if (vpAvatarId != '') {
                let animationsUrl = as.String(Config.get('avatars.animationsUrlTemplate', 'http://avatar.zweitgeist.com/gif/{id}/config.xml')).replace('{id}', vpAvatarId);
                let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://avatar.weblin.sui.li/avatar/?url={url}')).replace('{url}', encodeURIComponent(animationsUrl));
                this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
            } else if (vpAnimationsUrl != '') {
                let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://avatar.weblin.sui.li/avatar/?url={url}')).replace('{url}', encodeURIComponent(vpAnimationsUrl));
                this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
            } else {
                if (vpImageUrl != '') {
                    this.avatarDisplay?.updateObservableProperty('ImageUrl', vpImageUrl);
                }
                if (hasIdentityUrl) {
                    this.app.getPropertyStorage().watch(this.userId, 'AnimationsUrl', this.avatarDisplay);
                }
            }
        }

        if (this.nicknameDisplay) {
            if (vpNickname != '') {
                if (vpNickname != this.nicknameDisplay.getNickname()) {
                    this.nicknameDisplay.setNickname(vpNickname);
                }
            } else {
                if (xmppNickname != this.nicknameDisplay.getNickname()) {
                    this.nicknameDisplay.setNickname(xmppNickname);
                }
                if (hasIdentityUrl && this.isFirstPresence) {
                    this.app.getPropertyStorage().watch(this.userId, 'Nickname', this.nicknameDisplay);
                }
            }
        }

        if (presenceHasCondition) {
            this.avatarDisplay?.setCondition(newCondition);
        }

        if (this.isFirstPresence) {
            if (!presenceHasPosition) {
                newX = this.isSelf ? await this.app.getSavedPosition() : this.app.getDefaultPosition();
            }
            if (newX < 0) { newX = 100; }
            this.setPosition(newX);
        } else {
            if (presenceHasPosition) {
                if (this.getPosition() != newX) {
                    this.move(newX);
                }
            }
        }

        if (this.isFirstPresence) {
            if (this.isSelf) {
                this.show(true, Config.get('room.fadeInSec', 0.3));
            } else {
                this.show(true);
            }
        }

        if (this.isFirstPresence) {
            // if (this.isSelf && Environment.isDevelopment()) { this.showChatWindow(); }
            if (this.isSelf) {
                this.room?.showChatMessage(this.nick, 'entered the room');
            } else {
                if (this.room?.iAmAlreadyHere()) {
                    this.room?.showChatMessage(this.nick, 'entered the room');
                } else {
                    this.room?.showChatMessage(this.nick, 'was already there');
                }
            }
        }

        this.isFirstPresence = false;
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

    // Mouse

    onMouseClickAvatar(ev: JQuery.Event): void
    {
        super.onMouseClickAvatar(ev)
        if (this.isSelf) {
            this.toggleChatin();
        } else {
            this.toggleChatout();
        }
    }

    onMouseDoubleClickAvatar(ev: JQuery.Event): void
    {
        super.onMouseClickAvatar(ev)
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
