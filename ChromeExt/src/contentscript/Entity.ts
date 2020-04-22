// const $ = require('jquery');
import * as $ from 'jquery';
import { Room } from './Room';

export class Entity
{
    private elem: HTMLElement;
    private centerElem: HTMLElement;
    private positionX: number = -1;
    private visible: boolean = false;

    constructor(protected room: Room, protected display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-entity" />')[0];
        this.elem.style.display = 'none';

        // this.centerElem = <HTMLDivElement>$('<div class="n3q-entity-content" />')[0];
        // this.elem.appendChild(this.centerElem);
        var e = <HTMLElement>$('<div class="n3q-centertable"><div class="n3q-centercell"></div></div>')[0];
        this.centerElem = $(e).find('div.n3q-centercell')[0];
        this.elem.appendChild(e);

        this.display.appendChild(this.elem);
    }

    getElem(): HTMLElement { return this.elem; }
    getCenterElem(): HTMLElement { return this.centerElem; }

    shutdown(): void
    {
        this.show(false);
        this.display.removeChild(this.elem);
        delete this.elem;
    }

    show(visible: boolean): void
    {
        if (visible != this.visible) {
            this.elem.style.display = visible ? 'block' : 'none';
            this.visible = visible;
        }
    }

    setPosition(x: number): void
    {
        this.positionX = x;
        this.elem.style.left = x + 'px';
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

    // Drag

    private dragStartPosition: any;
    public onStartDragAvatar(ev: JQueryMouseEventObject, ui: any): void
    {
        this.dragStartPosition = ui.position;
    }

    public onDragAvatar(ev: JQueryMouseEventObject, ui: any): void
    {
    }

    public onStopDragAvatar(ev: JQueryMouseEventObject, ui: any): void
    {
        this.onDraggedBy((ui.position.left - this.dragStartPosition.left), (ui.position.top - this.dragStartPosition.top));
    }

    protected onDraggedBy(dX: number, dY: number): void
    {
    }

    // 

}
