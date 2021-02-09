import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { IObserver } from '../lib/ObservableProperty';
import { Pid } from '../lib/ItemProperties';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { Nickname } from './Nickname';
import { Chatout } from './Chatout';
import { Chatin } from './Chatin';
import { url } from 'inspector';
import { RoomItem } from './RoomItem';
import log = require('loglevel');
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { SimpleToast, Toast } from './Toast';
import { PrivateChatWindow } from './PrivateChatWindow';
import { ItemChangeOptions } from '../lib/ItemChangeOptions';

export class Participant extends Entity
{
    private nicknameDisplay: Nickname;
    private chatoutDisplay: Chatout;
    private chatinDisplay: Chatin;
    private isFirstPresence: boolean = true;
    private userId: string;
    private privateChatWindow: PrivateChatWindow;

    constructor(app: ContentApp, room: Room, private roomNick: string, isSelf: boolean)
    {
        super(app, room, isSelf);

        $(this.getElem()).addClass('n3q-participant');
        $(this.getElem()).attr('data-nick', roomNick);

        if (isSelf) {
            $(this.getElem()).addClass('n3q-participant-self');
        } else {
            $(this.getElem()).addClass('n3q-participant-other');
        }
    }

    getRoomNick(): string { return this.roomNick; }
    getChatout(): Chatout { return this.chatoutDisplay; }

    getDisplayName(): string
    {
        let name = this.roomNick;
        if (this.nicknameDisplay) {
            this.nicknameDisplay.getNickname();
        }
        return name;
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
        let hasPosition: boolean = false;
        let newX: number = 123;

        let hasCondition: boolean = false;
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
                        hasPosition = true;
                    }
                }
                hasCondition = true;
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
                    case 'chat': newCondition = ''; hasCondition = true; break;
                    case 'available': newCondition = ''; hasCondition = true; break;
                    case 'away': newCondition = 'sleep'; hasCondition = true; break;
                    case 'dnd': newCondition = 'sleep'; hasCondition = true; break;
                    case 'xa': newCondition = 'sleep'; hasCondition = true; break;
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
            this.avatarDisplay = new Avatar(this.app, this, this.isSelf);
            if (Config.get('backpack.enabled', false)) {
                // if (this.isSelf) {
                this.avatarDisplay.makeDroppable();
                // }
            }

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

        let hasAvatar = false;
        if (this.avatarDisplay) {
            if (vpAvatarId != '') {
                let animationsUrl = as.String(Config.get('avatars.animationsUrlTemplate', 'https://webex.vulcan.weblin.com/avatars/gif/{id}/config.xml')).replace('{id}', vpAvatarId);
                let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://webex.vulcan.weblin.com/Avatar/InlineData?url={url}')).replace('{url}', encodeURIComponent(animationsUrl));
                this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
                hasAvatar = true;
            } else if (vpAnimationsUrl != '') {
                let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://webex.vulcan.weblin.com/Avatar/InlineData?url={url}')).replace('{url}', encodeURIComponent(vpAnimationsUrl));
                this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
                hasAvatar = true;
            } else {
                if (vpImageUrl != '') {
                    this.avatarDisplay?.updateObservableProperty('ImageUrl', vpImageUrl);
                    hasAvatar = true;
                }
                if (hasIdentityUrl) {
                    this.app.getPropertyStorage().watch(this.userId, 'AnimationsUrl', this.avatarDisplay);
                    hasAvatar = true;
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

        if (hasCondition) {
            this.avatarDisplay?.setCondition(newCondition);
        }

        if (this.isFirstPresence) {
            if (!hasPosition) {
                newX = this.isSelf ? await this.app.getSavedPosition() : this.app.getDefaultPosition(this.roomNick);
            }
            if (newX < 0) { newX = 100; }
            this.setPosition(newX);
        } else {
            if (hasPosition) {
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
                this.room?.showChatMessage(this.roomNick, 'entered the room');
            } else {
                if (this.room?.iAmAlreadyHere()) {
                    this.room?.showChatMessage(this.roomNick, 'entered the room');
                } else {
                    this.room?.showChatMessage(this.roomNick, 'was already there');
                }
            }
        }

        if (this.isFirstPresence) {
            if (!hasAvatar && Config.get('room.vCardAvatarFallback', false)) {
                this.fetchVcardImage(this.avatarDisplay);
            }
        }

        this.isFirstPresence = false;
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();

        this.room?.showChatMessage(this.roomNick, 'left the room');
    }

    fetchVcardImage(avatarDisplay: IObserver)
    {
        let stanzaId = Utils.randomString(15);
        let iq = xml('iq', { 'type': 'get', 'id': stanzaId, 'to': this.room.getJid() + '/' + this.roomNick })
            .append(xml('vCard', { 'xmlns': 'vcard-temp' }))
            ;
        this.app.sendStanza(iq, stanzaId, (stanza) =>
        {
            let imageUrl = this.decodeVcardImage2DataUrl(stanza);
            if (imageUrl && imageUrl != '') {
                avatarDisplay.updateObservableProperty('VCardImageUrl', imageUrl);
            }
        });
    }

    decodeVcardImage2DataUrl(stanza: xml): string
    {
        let url: string;

        let vCardNode = stanza.getChildren('vCard').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vcard-temp');
        if (vCardNode) {
            let photoNodes = vCardNode.getChildren('PHOTO');
            let photoNode = photoNodes[0];
            if (photoNode) {
                let binvalNodes = photoNode.getChildren('BINVAL');
                let binvalNode = binvalNodes[0];
                let typeNodes = photoNode.getChildren('TYPE');
                let typeNode = typeNodes[0];
                if (binvalNode && typeNode) {
                    let data = binvalNode.text();
                    let type = typeNode.text();
                    if (data && data != '' && type && type != '') {
                        data = data.replace(/(\r\n|\n|\r)/gm, '').replace(/ /g, '');
                        url = 'data:' + type + ';base64,' + data;
                    }
                }
            }
        }

        return url;
    }

    // message

    onMessagePrivateChat(stanza: any): Promise<void>
    {
        let from = jid(stanza.attrs.from);
        let nick = from.getResource();
        let name = this.getDisplayName();
        let isChat = true;

        let pokeNode = stanza.getChildren('x').find(child => (child.attrs == null) ? false : as.String(child.attrs.xmlns, '') == 'vp:poke');
        if (pokeNode) {
            isChat = false;
            this.onReceivePoke(pokeNode);
        }

        let transferNode = stanza.getChildren('x').find(child => (child.attrs == null) ? false : as.String(child.attrs.xmlns, '') == 'vp:transfer');
        if (transferNode) {
            isChat = false;
            this.onReceiveTransfer(transferNode);
        }

        if (!isChat) { return; }

        let text = '';
        let bodyNode = stanza.getChild('body');
        if (bodyNode) {
            text = bodyNode.getText();
        }

        if (text == '') { return; }

        if (this.privateChatWindow == null) {
            this.openPrivateChat(this.elem);
        }
        this.privateChatWindow?.addLine(nick + Date.now(), name, text);
        // if (this.privateChatWindow == null) {
        //     new SimpleToast(this.app, 'PrivateChat', Config.get('room.privateChatToastDurationSec', 60), 'privatechat', name, text).show();
        // } else {
        //     this.privateChatWindow?.addLine(nick + Date.now(), name, text);
        // }
    }

    onReceivePoke(node: any): void
    {
        try {
            let pokeType = node.attrs.type;
            let toast = new SimpleToast(this.app, 'poke-' + pokeType, Config.get('room.pokeToastDurationSec', 10), 'greeting', this.getDisplayName(), pokeType + 's');
            toast.actionButton(pokeType + ' back', () => { this.sendPoke(pokeType); toast.close(); })
            toast.show();
        } catch (error) {
            //
        }
    }

    async onReceiveTransfer(node: any): Promise<void>
    {
        try {
            let type = as.String(node.attrs.type, '');
            let itemId = as.String(node.attrs.item, '');
            if (type != '' && itemId != '') {
                switch (type) {

                    case 'request':
                        if (node.children && node.children.length > 0)
                            for (let i = 0; i < node.children.length; i++) {
                                let body = as.String(node.children[i], '');
                                if (body != '') {
                                    let props = JSON.parse(body);
                                    await BackgroundMessage.addBackpackItem(itemId, props, {});
                                    await BackgroundMessage.derezBackpackItem(itemId, this.room.getJid(), -1, -1, {});
                                    await BackgroundMessage.modifyBackpackItemProperties(itemId, {}, [Pid.TransferState], { skipPresenceUpdate: true });
                                    this.room.confirmItemTransfer(itemId, this.roomNick);
                                }
                            }
                        break;

                    case 'confirm':
                        let props = await BackgroundMessage.getBackpackItemProperties(itemId);
                        if (props[Pid.TransferState] == Pid.TransferState_Source) {
                            await BackgroundMessage.deleteBackpackItem(itemId, {});
                        }
                        break;

                }
            }
        } catch (error) {
            //
        }
    }

    onMessageGroupchat(stanza: any): void
    {
        let from = jid(stanza.attrs.from);
        let nick = from.getResource();
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
        let bodyNode = stanza.getChild('body');
        if (bodyNode) {
            text = bodyNode.getText();
        }

        if (text == '') { return; }

        let name = this.getDisplayName();

        if (timestamp == 0) {
            timestamp = now;
        }
        let delayMSec = now - timestamp;

        // always
        this.room?.showChatMessage(name, text);

        // recent
        if (delayMSec * 1000 < as.Float(Config.get('room.maxChatAgeSec', 60))) {
            if (!this.isChatCommand(text)) {
                this.chatoutDisplay?.setText(text);
                this.app.toFront(this.elem);
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
    private chat_command_apply: string = '/action';
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

    private onMouseEnterAvatarVcardImageFallbackAlreadyTriggered: boolean = false;
    onMouseEnterAvatar(ev: JQuery.Event): void
    {
        super.onMouseEnterAvatar(ev);

        if (!this.onMouseEnterAvatarVcardImageFallbackAlreadyTriggered
            && this.avatarDisplay
            && this.avatarDisplay.isDefaultAvatar()
            && Config.get('room.vCardAvatarFallbackOnHover', false)
        ) {
            this.onMouseEnterAvatarVcardImageFallbackAlreadyTriggered = true;
            this.fetchVcardImage(this.avatarDisplay);
        }
    }

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

    onDraggedTo(newX: number): void
    {
        if (this.getPosition() != newX) {
            if (this.isSelf) {
                this.app.savePosition(newX);
                this.room?.sendMoveMessage(newX);
            } else {
                this.quickSlide(newX);
            }
        }
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

    showVidconfWindow(): void
    {
        this.app.showVidconfWindow();
    }

    showBackpackWindow(): void
    {
        this.app.showBackpackWindow();
    }

    sendPoke(type: string): void
    {
        this.room?.sendPoke(this.roomNick, type);
    }

    openPrivateChat(aboveElem: HTMLElement): void
    {
        if (this.privateChatWindow == null) {
            this.privateChatWindow = new PrivateChatWindow(this.app, this);
            this.privateChatWindow.show({
                'above': aboveElem,
                onClose: () =>
                {
                    this.privateChatWindow = null;
                },
            });
        }

        if (this.privateChatWindow == null) {
        }
    }

    async applyItem(roomItem: RoomItem)
    {
        let itemId = roomItem.getRoomNick();
        let roomJid = this.getRoom().getJid();
        if (this.isSelf) {
            log.debug('Participant.applyItem', 'derez', itemId, 'from', roomJid);
            await BackgroundMessage.derezBackpackItem(itemId, roomJid, -1, -1, {});
        } else {
            log.debug('Participant.applyItem', 'transfer', itemId, 'from', roomJid);
            await this.room?.transferItem(itemId, this.roomNick);
        }
    }

}
