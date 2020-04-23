import * as $ from 'jquery';
const { xml, jid } = require('@xmpp/client');
import { App } from './App';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { Nickname } from './Nickname';
import { Chatout } from './Chatout';
import { LegacyIdentity } from './LegacyIdentity';
import { as } from './as';
import { Log } from './Log';

export class Participant extends Entity
{
    private avatar: Avatar;
    private nickname: Nickname;
    private chatout: Chatout;
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

            {
                this.chatout = new Chatout(this.app, this, this.getElem());
            }

            // this.chatin = new Chatin(this.app, this, this.getElem());

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
}
