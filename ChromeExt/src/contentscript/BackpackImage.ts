import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils, Point2D } from '../lib/Utils';
import { ItemProperties } from '../lib/ItemProperties';
import { ContentApp } from './ContentApp';
import { BackpackWindow } from './BackpackWindow';

import imgDefaultItem from '../assets/DefaultItem.png';

export class BackpackImage
{
    private isFirstPresence: boolean = true;
    private elem: HTMLDivElement;
    private iconElem: HTMLImageElement;
    private x: number = 100;
    private y: number = 100;
    private w: number = 64;
    private h: number = 64;
    private inDrag: boolean = false;

    constructor(protected app: ContentApp, private backpackWindow: BackpackWindow, private itemId: string, private properties: ItemProperties)
    {
        let paneElem = this.backpackWindow.getPane();
        let padding: number = Config.get('backpack.borderPadding', 4);

        let size = Config.get('inventory.itemSize', 64);
        let x = this.getPseudoRandomCoordinate(paneElem.offsetWidth, this.w, padding, itemId, 11345);
        let y = this.getPseudoRandomCoordinate(paneElem.offsetHeight, this.w, padding, itemId, 13532);

        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-backpack-item" data-id="' + this.itemId + '" />').get(0);

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
                let dragElem = $('<div class="n3q-base n3q-backpack-drag" />').get(0);
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
        if (this.properties[pid]) {
            if (value) {
                return as.String(this.properties[pid], null) == as.String(value, null);
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

        // let item = this.app.getItemRepository().getItem(this.itemId);
        // if (item) {
        //     item.onClick(this.elem, new Point2D(ev.clientX, ev.clientY));
        // }
    }

    private dragIsRezable: boolean = false;
    private onDragStart(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        this.dragIsRezable = as.Bool(this.properties['IsRezable'], true);
        return true;
    }

    private onDrag(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        if (!this.dragIsRezable) {
            if (!this.isPositionInBackpack(ev, ui)) {
                return false;
            }
        }
        return true;
    }

    private onDragStop(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        if (this.isPositionInBackpack(ev, ui)) {
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

    private isPositionInBackpack(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): boolean
    {
        let scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
        let scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        let position = $(ui.helper).position();
        let itemElem = $(ui.helper).children().get(0);
        let width = $(itemElem).width();
        let height = $(itemElem).height();
        let x = position.left + width / 2;
        let y = position.top + height / 2;

        let paneElem = this.backpackWindow.getPane();
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

        let paneElem = this.backpackWindow.getPane();
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
        this.properties.InventoryX = '' + Math.round(x);
        this.properties.InventoryY = '' + Math.round(y);
        this.backpackWindow.setItemProperties(this.itemId, this.properties);
    }

    sendRezItem(x: number)
    {
        log.info('BackpackItem', 'sendRezItem', this.itemId, x);
        this.backpackWindow.rezItem(this.itemId, this.app.getRoom().getJid(), Math.round(x), this.app.getRoom().getDestination());
    }

    getPseudoRandomCoordinate(space: number, size: number, padding: number, id: string, mod: number): number
    {
        let min = size / 2 + padding;
        let max = space - min;
        return Utils.pseudoRandomInt(min, max, id, '', mod);
    }

    // events

    create()
    {
        this.applyProperties(this.properties);
    }

    applyProperties(properties: ItemProperties)
    {
        let newProperties: ItemProperties = {};
        let providerId = as.String(properties['provider'], null);
        for (let key in properties) {
            let value = properties[key];
            if (key.endsWith('Url')) {
                value = this.app.itemProviderUrlFilter(providerId, key, value);
            }
            newProperties[key] = value;
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

        if (as.Bool(newProperties.IsRezzed, false)) {
            $(this.elem).addClass('n3q-backpack-item-rezzed');
        } else {
            $(this.elem).removeClass('n3q-backpack-item-rezzed');
        }

        if (newProperties.InventoryX && newProperties.InventoryY) {
            var x = as.Int(newProperties.InventoryX, -1);
            var y = as.Int(newProperties.InventoryY, -1);
            if (x >= 0 && y >= 0 && (x != this.x || y != this.y)) {
                this.setPosition(x, y);
            }
        }

        if (Config.get('backpack.itemPropertiesTooltip', false)) {
            let propsText = '';
            for (let key in newProperties) {
                propsText += key + ': ' + newProperties[key] + '\r\n';
            }
            $(this.elem).prop('title', propsText);;
        }

        this.properties = newProperties;
    }

    destroy()
    {
        $(this.elem).remove();
    }

}
