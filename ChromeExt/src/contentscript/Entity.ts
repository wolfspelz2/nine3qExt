import log = require('loglevel');
import * as $ from 'jquery';
import 'jqueryui';
import { Room } from './Room';

export class Entity
{
    private elem: HTMLElement;
    private centerElem: HTMLElement;
    private positionX: number = -1;
    private visible: boolean = false;

    constructor(protected room: Room, protected display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-entity" />').get(0);
        this.elem.style.display = 'none';

        // this.centerElem = this.elem;

        this.centerElem = <HTMLDivElement>$('<div class="n3q-base n3q-entity-content" />').get(0);
        this.elem.appendChild(this.centerElem);

        // var e = <HTMLElement>$('<div class="n3q-base n3q-centertable"><div class="n3q-base n3q-centercell"></div></div>').get(0);
        // this.centerElem = $(e).find('div.n3q-centercell').get(0);
        // this.elem.appendChild(e);

        this.display.appendChild(this.elem);
    }

    getRoom(): Room { return this.room; }
    getElem(): HTMLElement { return this.elem; }
    getCenterElem(): HTMLElement { return this.centerElem; }

    show(visible: boolean): void
    {
        if (visible != this.visible) {
            this.elem.style.display = visible ? 'block' : 'none';
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
                () => this.quickSlideDestinationReached(newX)
            );
    }

    private quickSlideDestinationReached(newX: number): void
    {
        this.positionX = newX;
    }

    // Mouse

    onMouseEnterAvatar(ev: JQuery.Event): void
    {
    }

    onMouseLeaveAvatar(ev: JQuery.Event): void
    {
    }

    onMouseClickAvatar(ev: JQuery.Event): void
    {
    }

    onMouseDoubleClickAvatar(ev: JQuery.Event): void
    {
    }

    // Drag

    private dragStartPosition: any;
    onStartDragAvatar(ev: JQueryMouseEventObject, ui: any): void
    {
        this.dragStartPosition = ui.position;
    }

    onDragAvatar(ev: JQueryMouseEventObject, ui: any): void
    {
    }

    onStopDragAvatar(ev: JQueryMouseEventObject, ui: any): void
    {
        this.onDraggedBy((ui.position.left - this.dragStartPosition.left), (ui.position.top - this.dragStartPosition.top));
    }

    protected onDraggedBy(dX: number, dY: number): void
    {
    }

    // 

}
