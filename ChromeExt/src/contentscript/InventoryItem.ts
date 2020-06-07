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
    private size: number = 48;
    private inDrag: boolean = false;

    constructor(private app: ContentApp, private inv: Inventory, private itemId: string)
    {
        let paneElem = this.inv.getPane();
        let padding: number = Config.get('inventory.borderPadding', 4);

        let size = Config.get('inventory.iconSize', 32);
        let x = this.getPseudoRandomCoordinate(paneElem.offsetWidth, this.size, padding, itemId, 11345);
        let y = this.getPseudoRandomCoordinate(paneElem.offsetHeight, this.size, padding, itemId, 13532);

        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-inventory-item" data-id="' + this.itemId + '" />').get(0);
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
                this.onDrag(ev);
            },
            stop: (ev: JQueryMouseEventObject, ui) =>
            {
                var itemUnchanged = this.onDragStop(ev);
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
    private dragIsRezable: boolean = false;
    private onDragStart(ev: JQueryMouseEventObject): void
    {
        let offsetX: number = ev.originalEvent['offsetX'];
        let offsetY: number = ev.originalEvent['offsetY'];
        this.dragClickOffset = { 'dx': offsetX - this.size / 2, 'dy': offsetY - this.size / 2 };
        this.dragIsRezable = as.Bool(this.properties.RezableAspect, false);
    }

    private onDrag(ev: JQueryMouseEventObject): void
    {
        if (this.dragIsRezable) {
            this.isPositionInDropzone(ev);
        }
    }

    private onDragStop(ev: JQueryMouseEventObject): boolean
    {
        if (this.isPositionInInventory(ev)) {
            let newX = ev.offsetX - this.dragClickOffset.dx;
            let newY = ev.offsetY - this.dragClickOffset.dy;
            if (newX != this.x || newY != this.y) {
                this.setPosition(newX, newY);
                this.moveItem(newX, newY);
            }
        } else if (this.isPositionInDropzone(ev)) {
            let dropX = ev.pageX - $(this.app.getDisplay()).offset().left;
            this.rezItem(dropX);
            return false;
        }
        return true;
    }

    private isPositionInInventory(ev: JQueryMouseEventObject): boolean
    {
        let x = ev.offsetX;
        let y = ev.offsetY;
        let paneElem = this.inv.getPane();

        return ev.originalEvent['toElement'] == paneElem && x > 0 && x < paneElem.offsetWidth && y > 0 && y < paneElem.offsetHeight;
    }

    private isPositionInDropzone(ev: JQueryMouseEventObject): boolean
    {
        let x = ev.pageX;
        let y = ev.pageY;
        let displayElem = this.app.getDisplay();
        let dropZoneHeight: number = Config.get('inventory.dropZoneHeight', 100);
        x = x - $(displayElem).offset().left;
        y = $(displayElem).height() - y;
        return x > 0 && y > 0 && y < dropZoneHeight;
    }

    moveItem(x: number, y: number)
    {
        log.info('InventoryItem', 'move', x, y);
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

        if (newProperties.ContainerX && newProperties.ContainerY) {
            var x = as.Int(newProperties.ContainerX, -1);
            var y = as.Int(newProperties.ContainerY, -1);
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
