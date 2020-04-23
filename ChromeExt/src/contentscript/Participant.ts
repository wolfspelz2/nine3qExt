import * as $ from 'jquery';
const { xml, jid } = require('@xmpp/client');
import { App } from './App';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { Nickname } from './Nickname';
import { Chatout } from './Chatout';
import { Chatin } from './Chatin';
import { LegacyIdentity } from './LegacyIdentity';
import { as } from './as';
import { Log } from './Log';

export class Participant extends Entity
{
    private avatar: Avatar;
    private nickname: Nickname;
    private chatout: Chatout;
    private chatin: Chatin;
    private firstPresence: boolean = true;
    private defaultSpeedInPixelPerMsec: number = 0.1;
    private identityUrl: string;
    private userId: string;
    private inMove: boolean = false;
    private condition_: string = '';

    constructor(private app: App, room: Room, display: HTMLElement, private nick: string, private isSelf: boolean)
    {
        super(room, display);
        $(this.getElem()).addClass('n3q-participant');
    }

    //#region presence

    onPresenceAvailable(stanza: any): void
    {
        var presenceHasPosition: boolean = false;
        var newX: number = 123;
        var newCondition: string = '';

        {
            let stateNode = stanza.getChildren('x').find(stanzaChild => stanzaChild.attrs.xmlns === 'firebat:avatar:state');
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

        {
            let identityAttrs = stanza
                .getChildren('x')
                .find(stanzaChild => stanzaChild.attrs.xmlns === 'firebat:user:identity')
                .attrs
                ;

            let url = as.String(identityAttrs.src, '');
            let digest = as.String(identityAttrs.digest, '');
            let jid = as.String(identityAttrs.jid, url);
            this.userId = as.String(identityAttrs.id, jid);

            if (url != '' && digest != '') {
                this.identityUrl = url;
                this.app.getStorage().setIdentity(this.userId, url, digest);
            }
        }

        { // <show>: dnd, away, xa
            let showAvailability: string = 'available';
            let showNode = stanza.getChild('show');
            if (showNode != undefined) {
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

        if (this.firstPresence) {
            this.firstPresence = false;

            if (!presenceHasPosition) {
                newX = this.isSelf ? this.app.getSavedPosition() : this.app.getDefaultPosition();
            }
            if (newX < 0) { newX = 100; }
            this.setPosition(newX);

            {
                this.avatar = new Avatar(this.app, this, this.getCenterElem(), this.isSelf);
                this.app.getStorage().watch(this.userId, 'ImageUrl', this.avatar);
                this.app.getStorage().watch(this.userId, 'AnimationsUrl', this.avatar);
            }

            {
                this.nickname = new Nickname(this.app, this, this.getElem());
                let from = jid(stanza.attrs.from);
                let xmppNickname = as.String(from.getResource(), '');
                if (xmppNickname != '') {
                    this.nickname.setNickname(xmppNickname);
                }
                this.app.getStorage().watch(this.userId, 'Nickname', this.nickname);
            }

            this.chatout = new Chatout(this.app, this, this.getElem());
            this.chatin = new Chatin(this.app, this, this.getElem());

            this.show(true);

        } else {

            if (presenceHasPosition) {
                if (this.getPosition() != newX) {
                    this.move(newX);
                }
            }
        }

        this.condition_ = newCondition;
        if (!this.inMove) {
            this.avatar.setState(this.condition_);
        }
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.shutdown();
    }

    //#endregion
    //#region message

    onMessageGroupchat(stanza: any): void
    {
        let bodyNode = stanza.getChild('body');
        if (bodyNode != undefined) {
            let text = bodyNode.getText();

            if (text.substring(0, 1) == '/') {
                return this.onChatCommand(text);
            }

            this.chatout.setText(text);
        }
    }

    onChatCommand(text: string): void
    {
        var parts: string[] = text.split(' ');
        if (parts.length < 1) { return; }
        var cmd: string = parts[0];

        switch (cmd) {
            case '/do':
                if (parts.length < 2) { return; }
                this.chatout.setText(text);
                this.avatar.setAction(parts[1]);
                break;
        }
    }

    // /do WaterBottle ApplyTo WaterCan
    chat_command_apply: string = '/action';
    sendGroupChat(text: string, handler?: (IMessage: any) => any): void
    {
        //hw notyet
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

        this.room.sendGroupChat(text, this.nick);
    }

    //#endregion
    //#region Do stuff

    move(newX: number): void
    {
        this.inMove = true;

        if (newX < 0) { newX = 0; }

        if (this.isSelf) {
            this.app.savePosition(newX);
        }

        this.setPosition(this.getPosition());

        var oldX = this.getPosition();
        var diffX = newX - oldX;
        if (diffX < 0) {
            diffX = -diffX;
            this.avatar.setState('moveleft');
        } else {
            this.avatar.setState('moveright');
        }

        let speedPixelPerMsec = as.Float(this.avatar.getSpeedInPixelPerMsec(), this.defaultSpeedInPixelPerMsec);
        var durationMsec = diffX / speedPixelPerMsec;

        $(this.getElem())
            .stop(true)
            .animate(
                { left: newX + 'px' },
                durationMsec,
                'linear',
                () => this.onMoveDestinationReached(newX)
            );
    }

    onMoveDestinationReached(newX: number): void
    {
        this.inMove = false;
        this.setPosition(newX);
        this.avatar.setState(this.condition_);
    }

    onDraggedBy(dX: number, dY: number): void
    {
        var newX = this.getPosition() + dX;

        if (this.getPosition() != newX) {
            if (this.isSelf) {
                this.room.sendMoveMessage(newX);
            } else {
                this.quickSlide(newX);
            }
        }
    }

    quickSlide(newX: number): void
    {
        this.avatar.setState('');
        super.quickSlide(newX);
    }

    //#endregion
    //#region Mouse

    //hw notyet
    // menu: Menu = null;
    // onMouseClickAvatar(ev: JQueryMouseEventObject): void
    // {
    //     this.select()

    //     if (this.isSelf) {
    //         var elem = $(''
    //             + '<ul data-translate="children">'
    //             + '<li data-menuid="chat" data-translate="children"><span class="glyphicon glyphicon-font" /><span data-translate="text:Client">Chat</span></li>'
    //             + '<li data-menuid="inventory" data-translate="children"><span class="glyphicon glyphicon-folder-open" /><span data-translate="text:Client">Inventory</span></li>'
    //             + '<li data-menuid="nickname" data-translate="children"><span class="glyphicon glyphicon-pencil" /><span data-translate="text:Client">Change Nickname</span></li>'
    //             + '<li data-menuid="avatar" data-translate="children"><span class="glyphicon glyphicon-user" /><span data-translate="text:Client">Change Avatar</span></li>'
    //             + '<li data-translate="children"><span class="glyphicon glyphicon-asterisk" /><span data-translate="text:Client">Emotion</span>'
    //                 + '<ul data-translate="children">'
    //                 + '<li data-menuid="do-wave" data-translate="text:Client">wave</li>'
    //                 + '<li data-menuid="do-dance" data-translate="text:Client">dance</li>'
    //                 + '<li data-menuid="do-cheer" data-translate="text:Client">cheer</li>'
    //                 + '<li data-menuid="do-kiss" data-translate="text:Client">kiss</li>'
    //                 + '<li data-menuid="do-clap" data-translate="text:Client">clap</li>'
    //                 + '<li data-menuid="do-laugh" data-translate="text:Client">laugh</li>'
    //                 + '<li data-menuid="do-angry" data-translate="text:Client">angry</li>'
    //                 + '<li data-menuid="do-deny" data-translate="text:Client">deny</li>'
    //                 + '<li data-menuid="do-yawn" data-translate="text:Client">yawn</li>'
    //                 + '</ul>'
    //             + '</li>'
    //             + '</ul>'
    //         )[0];
    //         var actions: IMenuEvents = {
    //             'chat': (ev: JQueryMouseEventObject) => { this.toggleChatin(); },
    //             'inventory': (ev: JQueryMouseEventObject) => { this.app.toggleInventory(ev.clientX); },
    //             'nickname': (ev: JQueryMouseEventObject) => { this.changeNickname(ev.clientX); },
    //             'avatar': (ev: JQueryMouseEventObject) => { this.changeAvatar(ev.clientX); },
    //             'do-wave': (ev: JQueryMouseEventObject) => { this.do('wave'); },
    //             'do-dance': (ev: JQueryMouseEventObject) => { this.do('dance'); },
    //             'do-cheer': (ev: JQueryMouseEventObject) => { this.do('cheer'); },
    //             'do-kiss': (ev: JQueryMouseEventObject) => { this.do('kiss'); },
    //             'do-clap': (ev: JQueryMouseEventObject) => { this.do('clap'); },
    //             'do-laugh': (ev: JQueryMouseEventObject) => { this.do('laugh'); },
    //             'do-angry': (ev: JQueryMouseEventObject) => { this.do('angry'); },
    //             'do-deny': (ev: JQueryMouseEventObject) => { this.do('deny'); },
    //             'do-yawn': (ev: JQueryMouseEventObject) => { this.do('yawn'); },
    //         };
    //         if (this.menu == null) {
    //             this.menu = new Menu(this.app, this, elem, actions, ev, () => this.menu = null);
    //         }
    //     } else {
    //         var elem = $(''
    //             + '<ul data-translate="children">'
    //             + '<li data-menuid="chatout" data-translate="children"><span class="glyphicon glyphicon-font" /><span data-translate="text:Client">Chat</span></li>'
    //             + '</ul>'
    //         )[0];
    //         var actions: IMenuEvents = {
    //             'chatout': (ev: JQueryMouseEventObject) => { this.toggleChatout(); },
    //         };
    //         if (this.menu == null) {
    //             this.menu = new Menu(this.app, this, elem, actions, ev, () => this.menu = null);
    //         }
    //     }
    // }
}
