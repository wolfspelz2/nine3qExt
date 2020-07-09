import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Window } from './Window';
import { Item } from './Item';
import { RoomItem } from './RoomItem';
import { InventoryItem } from './InventoryItem';

type WindowOptions = any;

interface ItemFrameWindowOptions extends WindowOptions
{
    item: Item;
    clickPos: Point2D;
    url: string;
    onClose: { (): void };
}

export class ItemFrameWindow extends Window
{
    constructor(app: ContentApp)
    {
        super(app);
    }

    async show(options: ItemFrameWindowOptions)
    {
        try {
            let url: string = options.url;
            if (!url) { throw 'No url' }

            options.minLeft = as.Int(options.minLeft, 10);
            options.minTop = as.Int(options.minTop, 10);
            options.offsetLeft = as.Int(options.bottom, 20);
            options.offsetTop = as.Int(options.bottom, -350);
            options.width = as.Int(options.item.getProperties().IframeWidth, 400);
            options.height = as.Int(options.item.getProperties().IframeHeight, 400);
            options.resizable = as.Bool(options.item.getProperties().IframeResizable, true);
            options.titleText = as.String(options.item.getProperties().Label, 'Item');

            log.debug('ItemFrameWindow', url);
            super.show(options);

            $(this.windowElem).addClass('n3q-itemframewindow');

            let left = Math.max(options.clickPos.x - options.width / 2 + options.offsetLeft, options.minLeft);
            let top = Math.max(options.clickPos.y - options.height / 2 + options.offsetTop, options.minTop);

            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-itemframewindow-content" src="' + url + ' " frameborder="0"></iframe>').get(0);

            $(this.contentElem).append(iframeElem);
            this.app.translateElem(this.windowElem);
            $(this.windowElem).css({ 'width': options.width + 'px', 'height': options.height + 'px', 'left': left + 'px', 'top': top + 'px' });
            this.app.toFront(this.windowElem)

        } catch (error) {
            log.info('ItemFrameWindow', error);
            if (options.onClose) { options.onClose(); }
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }
}
