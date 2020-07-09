import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Popup } from './Popup';
import { Item } from './Item';

type PopupOptions = any;

interface ItemFramePopupOptions extends PopupOptions
{
    item: Item;
    above: HTMLElement;
    url: string;
    onClose: { (): void };
}

export class ItemFramePopup extends Popup
{
    constructor(app: ContentApp)
    {
        super(app);
    }

    async show(options: ItemFramePopupOptions)
    {
        try {
            let url: string = options.url;
            if (!url) { throw 'No url' }

            options.width = as.Int(options.item.getProperties().IframeWidth, 400);
            options.height = as.Int(options.item.getProperties().IframeHeight, 400);

            log.debug('ItemFramePopup', url);
            super.show(options);

            $(this.windowElem).addClass('n3q-itemframepopup');

            let left = 50;
            let top = 50;
            let minLeft = 10;
            let minTop = 10;
            let vertOffset = 50;

            let itemPos = $(options.above).offset();
            let scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
            let scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            let itemAbsLeft = itemPos.left - scrollLeft;
            let itemAbsTop = itemPos.top - scrollTop;
            let itemWidth = as.Int(options.item.getProperties().Width, 64);
            let itemHeight = as.Int(options.item.getProperties().Height, 64);
            let itemCenterLeft = itemAbsLeft + itemWidth / 2;
            let itemCenterTop = itemAbsTop + itemHeight / 2;

            left = itemCenterLeft - options.width / 2;
            top = itemCenterTop - options.height / 2 - vertOffset;

            if (left < minLeft) { left = minLeft; }
            if (top < minTop) { top = minTop; }

            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-itemframepopup-content" src="' + url + ' " frameborder="0"></iframe>').get(0);

            $(this.windowElem).append(iframeElem);
            this.app.translateElem(this.windowElem);
            $(this.windowElem).css({ 'width': options.width + 'px', 'height': options.height + 'px', 'left': left + 'px', 'top': top + 'px' });
            this.app.toFront(this.windowElem, ContentApp.DisplayLayer_Popup)

        } catch (error) {
            log.info('ItemFramePopup', error);
            if (options.onClose) { options.onClose(); }
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }
}
