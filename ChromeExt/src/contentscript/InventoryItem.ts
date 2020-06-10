import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Inventory } from './Inventory';

import imgDefaultItem from '../assets/DefaultIcon.png';

export class InventoryItem
{
    private isFirstPresence: boolean = true;
    private properties: { [pid: string]: string } = {};
    private elem: HTMLDivElement;
    private iconElem: HTMLImageElement;
    private x: number = 100;
    private y: number = 100;
    private w: number = 64;
    private h: number = 64;
    private inDrag: boolean = false;

    constructor(private app: ContentApp, private inv: Inventory, private itemId: string)
    {
        let paneElem = this.inv.getPane();
        let padding: number = Config.get('inventory.borderPadding', 4);

        let size = Config.get('inventory.iconSize', 64);
        let x = this.getPseudoRandomCoordinate(paneElem.offsetWidth, this.w, padding, itemId, 11345);
        let y = this.getPseudoRandomCoordinate(paneElem.offsetHeight, this.w, padding, itemId, 13532);

        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-inventory-item" data-id="' + this.itemId + '" />').get(0);
        // this.iconElem = <HTMLImageElement>$('<img class="n3q-base n3q-item-icon" />').get(0);
        // $(this.elem).append(this.iconElem);

        // this.iconElem.src = imgDefaultItem;
        this.setImage(imgDefaultItem);
        this.setSize(size, size);
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
                item.css({ 'left': '0', 'top': '0' });
                $(elem).append(item);
                $(app.getDisplay()).append(elem);
                return elem;
            },
            zIndex: 2000000000,
            containment: '#n3q',
            start: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                this.inDrag = true;
                this.onDragStart(ev, ui);
                $(this.elem).hide();
            },
            drag: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                this.onDrag(ev, ui);
            },
            stop: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                var itemUnchanged = this.onDragStop(ev, ui);
                if (itemUnchanged) {
                    $(this.elem).show(0);
                } else {
                    $(this.elem).delay(1000).show(0);
                }
                this.inDrag = false;
            }
        });
    }

    getX(): number { return this.x; }
    getY(): number { return this.y; }
    geSize(): number { return this.w; }

    setImage(url: string): void
    {
        $(this.elem).css({ 'background-image': 'url("' + url + '")' });
    }

    setSize(w: number, h: number)
    {
        this.w = w;
        this.h = h;
        $(this.elem).css({ 'width': w + 'px', 'height': h + 'px' });
    }

    setPosition(x: number, y: number)
    {
        this.x = x;
        this.y = y;
        $(this.elem).css({ 'left': (x - this.w / 2) + 'px', 'top': (y - this.w / 2) + 'px' });
    }

    private dragClickOffset: Record<string, number> = { dx: 0, dy: 0 };
    private dragIsRezable: boolean = false;
    private onDragStart(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
        let offsetX: number = ev.originalEvent['offsetX'];
        let offsetY: number = ev.originalEvent['offsetY'];
        this.dragClickOffset = { 'dx': offsetX - this.w / 2, 'dy': offsetY - this.w / 2 };
        this.dragIsRezable = as.Bool(this.properties.RezableAspect, true);
    }

    private onDrag(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
        if (this.dragIsRezable) {
            // this.isPositionInInventory(ev, ui);
            // this.isPositionInDropzone(ev, ui);
        }
    }

    private onDragStop(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        if (this.isPositionInInventory(ev, ui)) {
            let newX = ev.offsetX - this.dragClickOffset.dx;
            let newY = ev.offsetY - this.dragClickOffset.dy;
            if (newX != this.x || newY != this.y) {
                this.setPosition(newX, newY);
                this.moveItem(newX, newY);
            }
        } else if (this.isPositionInDropzone(ev, ui)) {
            let dropX = ev.pageX - $(this.app.getDisplay()).offset().left;
            this.rezItem(dropX);
            return false;
        }
        return true;
    }

    private isPositionInInventory(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        let mouseX = ev.offsetX;
        let mouseY = ev.offsetY;
        let paneElem = this.inv.getPane();

        let inInventory = ev.originalEvent['toElement'] == paneElem && mouseX > 0 && mouseX < paneElem.offsetWidth && mouseY > 0 && mouseY < paneElem.offsetHeight;
        log.info('InInventory', inInventory);
        return inInventory;
    }

    private isPositionInDropzone(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        let displayElem = this.app.getDisplay();
        let dropZoneHeight: number = Config.get('inventory.dropZoneHeight', 100);
        let dragHelperElem = ui.helper.get(0);
        let dragItemElem = dragHelperElem.children[0];

        let draggedLeft = $(dragHelperElem).position().left;
        let draggedTop = $(dragHelperElem).position().top;
        let draggedWidth = $(dragItemElem).width();
        let draggedHeight = $(dragItemElem).height();
        let dropzoneBottom = $(displayElem).height();
        let dropzoneTop = dropzoneBottom - dropZoneHeight;
        let x = draggedLeft + draggedWidth / 2;
        let y = draggedTop + draggedHeight;

        let inDropzone = x > 0 && y > dropzoneTop && y < dropzoneBottom;
        log.info('InDropzone', inDropzone);
        return inDropzone;
    }

    moveItem(x: number, y: number)
    {
        log.info('InventoryItem', 'move', x, y);

        this.inv.sendCommand(this.itemId, 'SetCoordinate', { 'x': x, 'y': y });
    }

    rezItem(x: number)
    {
        log.info('InventoryItem', 'rez', this.itemId, x);

        let to = this.app.getRoom().getJid();
        let destination = this.app.getRoom().getDestination();

        let params = {
            'to': to,
            'x': x,
            'destination': ''
        };
        this.inv.sendCommand(this.itemId, 'Rez', params);
    }

    remove(): void
    {
        $(this.elem).remove();
    }

    getPseudoRandomCoordinate(space: number, size: number, padding: number, id: string, mod: number): number
    {
        let min = size / 2 + padding;
        let max = space - min;
        return Utils.pseudoRandomInt(min, max, id, '', mod);
    }

    // presence

    onPresenceAvailable(stanza: any): void
    {
        let newProperties: { [pid: string]: string } = {};

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

        if (newProperties.ImageUrl) {
            this.setImage(newProperties.ImageUrl);
        }

        if (newProperties.Width && newProperties.Height) {
            var w = as.Int(newProperties.Width, -1);
            var h = as.Int(newProperties.Height, -1);
            if (w > 0 && h > 0 && (w != this.w || h != this.h)) {
                this.setSize(w, h);
            }
        }

        if (newProperties.InventoryX && newProperties.InventoryY) {
            var x = as.Int(newProperties.InventoryX, -1);
            var y = as.Int(newProperties.InventoryY, -1);
            if (x >= 0 && y >= 0 && (x != this.x || y != this.y)) {
                this.setPosition(x, y);
            }
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
