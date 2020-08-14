import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils, Point2D } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Inventory } from './Inventory';
import { Item } from './Item';

import imgDefaultItem from '../assets/DefaultItem.png';

export class InventoryItem
{
    private item: Item;
    private isFirstPresence: boolean = true;
    private elem: HTMLDivElement;
    private iconElem: HTMLImageElement;
    private x: number = 100;
    private y: number = 100;
    private w: number = 64;
    private h: number = 64;
    private inDrag: boolean = false;

    getProperties(): { [pid: string]: string } { return this.item.getProperties(); }

    constructor(protected app: ContentApp, private inv: Inventory, private itemId: string)
    {
        this.item = new Item(app, itemId);

        let paneElem = this.inv.getPane();
        let padding: number = Config.get('inventory.borderPadding', 4);

        let size = Config.get('inventory.itemSize', 64);
        let x = this.getPseudoRandomCoordinate(paneElem.offsetWidth, this.w, padding, itemId, 11345);
        let y = this.getPseudoRandomCoordinate(paneElem.offsetHeight, this.w, padding, itemId, 13532);

        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-inventory-item" data-id="' + this.itemId + '" />').get(0);

        this.setImage(imgDefaultItem);
        this.setSize(50, 50);
        this.setPosition(x, y);

        $(paneElem).append(this.elem);

        $(this.elem).click(ev =>
        {
            this.onMouseClick(ev);
        });

        $(this.elem).draggable({
            scroll: false,
            stack: '.n3q-item-icon',
            distance: 4,
            //opacity: 0.5,
            helper: function ()
            {
                let dragElem = $('<div class="n3q-base n3q-inventory-drag" />').get(0);
                let itemElem = $(this).clone().get(0);
                $(itemElem).css({ 'left': '0', 'top': '0', 'width': this.w, 'height': this.h });
                $(dragElem).append(itemElem);
                app.toFront(itemElem);
                $(app.getDisplay()).append(dragElem);
                return dragElem;
            },
            // zIndex: 2000000000,
            containment: '#n3q',
            start: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                this.app.toFront(this.elem);
                this.inDrag = true;
                $(this.elem).hide();
                return this.onDragStart(ev, ui);
            },
            drag: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                return this.onDrag(ev, ui);
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
                return true;
            }
        });
    }

    getX(): number { return this.x; }
    getY(): number { return this.y; }
    geSize(): number { return this.w; }

    match(pid: string, value: any)
    {
        if (this.item.getProperties()[pid]) {
            if (value) {
                return as.String(this.item.getProperties()[pid], null) == as.String(value, null);
            }
        }
        return false;
    }

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
        $(this.elem).css({ 'left': (x - this.w / 2) + 'px', 'top': (y - this.h / 2) + 'px' });
    }

    onMouseClick(ev: JQuery.Event): void
    {
        this.app.toFront(this.elem);

        let item = this.app.getItemRepository().getItem(this.itemId);
        if (item) {
            item.onClick(this.elem, new Point2D(ev.clientX, ev.clientY));
        }
    }

    private dragIsRezable: boolean = false;
    private onDragStart(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        let item = this.app.getItemRepository().getItem(this.itemId);
        if (item) {
            item.onDrag(this.elem, new Point2D(ev.clientX, ev.clientY));
        }

        this.dragIsRezable = as.Bool(this.item.getProperties().RezableAspect, true);
        return true;
    }

    private onDrag(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        if (!this.dragIsRezable) {
            if (!this.isPositionInInventory(ev, ui)) {
                return false;
            }
        }
        return true;
    }

    private onDragStop(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        if (this.isPositionInInventory(ev, ui)) {
            let pos = this.getPositionRelativeToPane(ev, ui);
            if (pos.x != this.x || pos.y != this.y) {
                this.setPosition(pos.x, pos.y);
                this.sendSetItemCoordinates(pos.x, pos.y);
            }
        } else if (this.isPositionInDropzone(ev, ui)) {
            let dropX = ev.pageX - $(this.app.getDisplay()).offset().left;
            this.sendRezItem(dropX);
            return false;
        }
        return true;
    }

    private isPositionInInventory(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        let scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
        let scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        let position = $(ui.helper).position();
        let itemElem = $(ui.helper).children().get(0);
        let width = $(itemElem).width();
        let height = $(itemElem).height();
        let x = position.left + width / 2;
        let y = position.top + height / 2;

        let paneElem = this.inv.getPane();
        let panePosition = $(paneElem).offset();
        panePosition.left -= scrollLeft;
        panePosition.top -= scrollTop;
        let paneWidth = $(paneElem).width();
        let paneHeight = $(paneElem).height();

        return x > panePosition.left && x < panePosition.left + paneWidth && y < panePosition.top + paneHeight && y > panePosition.top;
    }

    private getPositionRelativeToPane(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): { x: number, y: number }
    {
        let scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
        let scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        let position = $(ui.helper).position();
        let itemElem = $(ui.helper).children().get(0);
        let width = $(itemElem).width();
        let height = $(itemElem).height();
        let x = position.left + width / 2;
        let y = position.top + height / 2;

        let paneElem = this.inv.getPane();
        let panePosition = $(paneElem).offset();
        panePosition.left -= scrollLeft;
        panePosition.top -= scrollTop;

        return { 'x': x - panePosition.left, 'y': y - panePosition.top };
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
        return inDropzone;
    }

    sendSetItemCoordinates(x: number, y: number)
    {
        log.info('InventoryItem', 'move', x, y);

        let params = {
            'x': Math.round(x),
            'y': Math.round(y),
        };

        this.inv.sendCommand(this.itemId, 'SetItemCoordinates', params);
    }

    sendRezItem(x: number)
    {
        log.info('InventoryItem', 'rez', this.itemId, x);

        let to = this.app.getRoom().getJid();
        let destination = this.app.getRoom().getDestination();

        let params = {
            'to': to,
            'x': Math.round(x),
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
        let newProviderId: string = '';
        let newProperties: { [pid: string]: string } = {};

        let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
        if (vpPropsNode) {
            let attrs = vpPropsNode.attrs;
            if (attrs) {
                newProviderId = as.String(attrs.provider, null);
                for (let attrName in attrs) {
                    let attrValue = attrs[attrName];
                    if (attrName.endsWith('Url')) {
                        attrValue = this.app.itemProviderUrlFilter(newProviderId, attrName, attrValue);
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

        if (newProperties && newProviderId) {
            let item = this.app.getItemRepository().getItem(this.itemId);
            if (item) {
                this.app.getItemRepository().getItem(this.itemId).setProperties(newProperties);
            } else {
                this.app.getItemRepository().addItem(this.itemId, newProviderId, newProperties);
            }
        }

        this.isFirstPresence = false;
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();
    }
}
