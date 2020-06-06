import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Inventory } from './Inventory';

import imgDefaultItem from '../assets/DefaultIcon.png';
import { timingSafeEqual } from 'crypto';

export class InventoryItem
{
    private isFirstPresence: boolean = true;
    private properties: { [pid: string]: string } = {};
    private elem: HTMLDivElement;
    private iconElem: HTMLImageElement;
    private x: number = 100;
    private y: number = 100;
    private size: number = 48;
    private inDrag: boolean = false;

    constructor(app: ContentApp, private inv: Inventory, private id: string)
    {
        let paneElem = this.inv.getDisplay();
        let padding: number = Config.get('inventory.borderPadding', 4);

        let size = Config.get('inventory.iconSize', 32);
        let x = this.getPseudoRandomCoordinate(paneElem.offsetWidth, this.size, padding, id, 11345);
        let y = this.getPseudoRandomCoordinate(paneElem.offsetHeight, this.size, padding, id, 13532);

        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-inventory-item" />').get(0);
        // this.iconElem = <HTMLImageElement>$('<img class="n3q-base n3q-item-icon" />').get(0);
        // $(this.elem).append(this.iconElem);

        // this.iconElem.src = imgDefaultItem;
        this.setImage(imgDefaultItem);
        this.setSize(size);
        this.setPosition(x, y);

        $(paneElem).append(this.elem);

        // let x = $('<div class="n3q-base n3q-item-icon-drag" />');
        // $(x).css({ 'width': this.elem.offsetWidth + 'px', 'height': this.elem.offsetHeight + 'px' });
        // $(x).append($(this.elem).clone());
        // $(app.getDisplay()).append(x);

        $(this.elem).draggable({
            scroll: false,
            stack: '.n3q-item-icon',
            distance: 4,
            //opacity: 0.5,
            helper: function ()
            {
                let elem = $('<div class="n3q-base n3q-inventory-drag" />');
                let item = $(this).clone();
                $(item).css({ 'left': '0', 'top': '0', 'width': this.offsetWidth + 'px', 'height': this.offsetHeight + 'px' });
                $(elem).append(item);
                $(app.getDisplay()).append(elem);
                return elem;
            },
            zIndex: 2000000000,
            containment: '#n3q',
            start: (ev: JQueryMouseEventObject, ui) =>
            {
                this.inDrag = true;
                this.onDragStart(ev);
                $(this.elem).hide();
            },
            drag: (ev: JQueryMouseEventObject, ui) =>
            {
            },
            stop: (ev: JQueryMouseEventObject, ui) =>
            {
                $(this.elem).show();
                this.onDragStop(ev);
                this.inDrag = false;
            }
        });
    }

    getX(): number { return this.x; }
    getY(): number { return this.y; }
    geSize(): number { return this.size; }

    setImage(url: string): void
    {
        $(this.elem).css({ 'background-image': 'url("' + url + '")' });
    }

    setSize(size: number)
    {
        this.size = size;
        $(this.elem).css({ 'width': size + 'px', 'height': size + 'px' });
    }

    setPosition(x: number, y: number)
    {
        this.x = x;
        this.y = y;
        $(this.elem).css({ 'left': (x - this.size / 2) + 'px', 'top': (y - this.size / 2) + 'px' });
    }

    private dragClickOffset: Record<string, number> = { dx: 0, dy: 0 };
    private onDragStart(ev: JQueryMouseEventObject): void
    {
        let offsetX: number = ev.originalEvent['offsetX'];
        let offsetY: number = ev.originalEvent['offsetY'];
        this.dragClickOffset = { 'dx': offsetX - this.size / 2, 'dy': offsetY - this.size / 2 };
    }

    private onDragStop(ev: JQueryMouseEventObject): void
    {
        let x = ev.offsetX;
        let y = ev.offsetY;
        let paneElem = this.inv.getDisplay();

        if (ev.originalEvent['toElement'] == paneElem && x > 0 && x < paneElem.offsetWidth && y > 0 && y < paneElem.offsetHeight) {
            let newX = x - this.dragClickOffset.dx;
            let newY = y - this.dragClickOffset.dy;
            if (newX != this.x || newY != this.y) {
                this.setPosition(newX, newY);
            }
        }
    }

    remove(): void
    {
        $(this.elem).remove();
    }

    getPseudoRandomCoordinate(space: number, size: number, padding: number, id: string, mod: number): number
    {
        let result = 50;
        let hash = Utils.hash(id) % mod;
        let min = size / 2 + padding;
        let max = space - min;
        result = min + (max - min) / mod * hash;
        return result;
    }

    // presence

    onPresenceAvailable(stanza: any): void
    {
        let newProperties: { [pid: string]: string } = {};
        let imgUrl: string;

        let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
        if (vpPropsNode) {
            let attrs = vpPropsNode.attrs;
            if (attrs) {
                let providerId = as.String(attrs.provider, '');
                for (let attrName in attrs) {
                    let attrValue = attrs[attrName];
                    if (attrName.endsWith('Url')) {
                        attrValue = ContentApp.itemProviderUrlFilter(providerId, attrName, attrValue);
                    }
                    newProperties[attrName] = attrValue;
                }
            }
        }

        if (!imgUrl) {
            if (newProperties.Icon32Url) {
                imgUrl = newProperties.Image100Url;
            }
        }

        if (!imgUrl) {
            if (newProperties.Icon32Url) {
                imgUrl = newProperties.Icon32Url;
            }
        }

        if (imgUrl) {
            this.setImage(imgUrl);
        }

        if (this.isFirstPresence) {

        }

        this.properties = newProperties;

        this.isFirstPresence = false;
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();
    }
}
