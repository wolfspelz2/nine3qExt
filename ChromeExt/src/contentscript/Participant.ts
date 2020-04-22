import * as $ from 'jquery';
import { App } from './App';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { LegacyIdentity } from './LegacyIdentity';
import { as } from './as';
import { Log } from './Log';

export class Participant extends Entity
{
    private avatar: Avatar;
    private firstPresence: boolean = true;
    private defaultSpeedInPixelPerMsec: number = 0.1;
    private identityUrl: string;
    private userId: string;

    constructor(private app: App, room: Room, display: HTMLElement, private nick: string, private isSelf: boolean)
    {
        super(room, display);
        $(this.getElem()).addClass('n3q-participant');
    }

    onPresenceAvailable(stanza: any)
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
                    Log.info(newX);
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

            this.avatar = new Avatar(this.app, this, this.getCenterElem());
            if (this.isSelf) {
                this.app.getStorage().watch(this.userId, 'ImageUrl', this.avatar);
                this.app.getStorage().watch(this.userId, 'AnimationsUrl', this.avatar);
            }

            // this.nickname = new Nickname(this.app, this, this.getElem());
            // this.chatout = new Chatout(this.app, this, this.getElem());
            // this.chatin = new Chatin(this.app, this, this.getElem());

            this.show(true);

            // this.app.sendGetUserAttributesMessage(this.id, msg => this.onAttributes(msg));
        } else {

            if (presenceHasPosition) {
                if (this.getPosition() != newX) {
                    this.move(newX);
                }
            }
        }

        this.avatar.setState(newCondition);
    }

    onPresenceUnavailable(stanza: any)
    {
        this.shutdown();
    }

    move(newX: number): void
    {
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
        this.setPosition(newX);
        this.avatar.setState('');
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
