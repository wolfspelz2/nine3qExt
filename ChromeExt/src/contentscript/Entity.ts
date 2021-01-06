import log = require('loglevel');
import * as $ from 'jquery';
import 'jqueryui';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { ContentApp } from './ContentApp';

import imgDefaultAvatar from '../assets/DefaultAvatar.png';

export class Entity
{
    protected elem: HTMLElement;
    protected visible: boolean = false;
    protected avatarDisplay: Avatar;
    protected positionX: number = -1;
    protected defaultSpeedPixelPerSec: number = as.Float(Config.get('room.defaultAvatarSpeedPixelPerSec', 100));
    protected inMove: boolean = false;

    constructor(protected app: ContentApp, protected room: Room, protected isSelf: boolean)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-entity" />').get(0);
        this.elem.style.display = 'none';

        $(this.elem).append(this.elem);

        $(app.getDisplay()).append(this.elem);
    }

    getRoom(): Room { return this.room; }
    getElem(): HTMLElement { return this.elem; }
    getDefaultAvatar(): string { return imgDefaultAvatar; }
    getAvatar() { return this.avatarDisplay; }

    show(visible: boolean, durationSec: number = 0.0): void
    {
        if (visible != this.visible) {
            if (visible) {
                if (durationSec > 0) {
                    $(this.elem).fadeIn(durationSec * 1000);
                } else {
                    this.elem.style.display = 'block';
                }
            } else {
                this.elem.style.display = 'none';
            }
            this.visible = visible;
        }
    }

    remove(): void
    {
        this.show(false);
        $(this.elem).remove();
        delete this.elem;
    }

    setPosition(x: number): void
    {
        this.positionX = x;
        if (this.elem != undefined) {
            this.elem.style.left = x + 'px';
        }
    }

    move(newX: number): void
    {
        this.inMove = true;

        if (newX < 0) { newX = 0; }

        this.setPosition(this.getPosition());

        var oldX = this.getPosition();
        var diffX = newX - oldX;
        var absDiffX = diffX < 0 ? -diffX : diffX;

        if (this.avatarDisplay) {
            if (diffX < 0) {
                this.avatarDisplay.setState('moveleft');
            } else {
                this.avatarDisplay.setState('moveright');
            }
        }

        let speedPixelPerSec = as.Float(this.avatarDisplay.getSpeedPixelPerSec(), this.defaultSpeedPixelPerSec);
        var durationSec = absDiffX / speedPixelPerSec;

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
        this.avatarDisplay?.setState('');
    }

    getPosition(): number
    {
        return this.positionX;
    }

    quickSlide(newX: number): void
    {
        if (newX < 0) { newX = 0; }

        $(this.elem)
            .stop(true)
            .animate(
                { left: newX + 'px' },
                100,
                'linear',
                () => this.onQuickSlideReached(newX)
            );
    }

    onQuickSlideReached(newX: number): void
    {
        this.positionX = newX;
    }

    // Xmpp

    onPresenceAvailable(stanza: any)
    {
        log.error('Entity.onPresenceAvailable', 'not implemented', 'you should not be here');
    }

    // Mouse

    onMouseEnterAvatar(ev: JQuery.Event): void
    {
        this.avatarDisplay?.hilite(true);
    }

    onMouseLeaveAvatar(ev: JQuery.Event): void
    {
        this.avatarDisplay?.hilite(false);
    }

    onMouseClickAvatar(ev: JQuery.Event): void
    {
        this.select()
    }

    onMouseDoubleClickAvatar(ev: JQuery.Event): void
    {
    }

    select(): void
    {
        this.app.toFront(this.elem);
    }

    // Drag

    private dragStartPosition: { top: number; left: number; };
    onDragAvatarStart(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
        this.dragStartPosition = ui.position;
        this.app.toFront(this.elem);
    }

    onDragAvatar(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
    }

    onDragAvatarStop(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
        let dX = ui.position.left - this.dragStartPosition.left;
        let newX = this.getPosition() + dX;
        this.onDraggedTo(newX);
    }

    onDraggedTo(newX: number): void
    {
        if (this.getPosition() != newX) {
            this.quickSlide(newX);
        }
    }
}
