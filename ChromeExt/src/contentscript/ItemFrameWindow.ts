import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Window } from './Window';
import { Item } from './Item';
import { RoomItem } from './RoomItem';
import { InventoryItem } from './InventoryItem';

type WindowOptions = any;

interface ItemFrameWindowOptions extends WindowOptions
{
    item: Item;
    above: HTMLElement;
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

            options.bottom = 150;
            options.width = as.Int(options.item.getProperties().IframeWidth, 400);
            options.height = as.Int(options.item.getProperties().IframeHeight, 400);
            options.resizable = as.Bool(options.item.getProperties().IframeResizable, true);
            options.titleText = as.String(options.item.getProperties().Label, 'Item');

            log.debug('ItemFrameWindow', url);
            super.show(options);

            $(this.windowElem).addClass('n3q-itemframewindow');

            let left = 50;
            let top = 50;
            let minLeft = 10;
            let minTop = 10;

            if (options.item instanceof RoomItem) {
                left = Math.max(options.above.offsetLeft - 180, left);
                top = this.app.getDisplay().offsetHeight - options.height - options.bottom;
            } else if (options.item instanceof InventoryItem) {
                let itemPos = $(options.above).offset();
                let scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
                let scrollTop = window.pageYOffset || document.documentElement.scrollTop;
                let itemAbsLeft = itemPos.left - scrollLeft;
                let itemAbsTop = itemPos.top - scrollTop;
                let itemWidth = as.Int(options.item.getProperties().Width, 64);
                let itemHeight = as.Int(options.item.getProperties().Height, 64);
                let itemCenterLeft = itemAbsLeft + itemWidth / 2;
                let itemCenterTop = itemAbsTop + itemHeight / 2;
                let left = itemCenterLeft - options.width / 2;
                let top = itemCenterTop - options.height / 2;
            }

            if (left < minLeft) { left = minLeft; }
            if (top < minTop) { top = minTop; }

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
