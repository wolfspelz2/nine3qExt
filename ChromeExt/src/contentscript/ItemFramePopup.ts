import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Popup } from './Popup';
import { RepositoryItem } from './RepositoryItem';
import { Pid } from '../lib/ItemProperties';

type PopupOptions = any;

interface ItemFramePopupOptions extends PopupOptions
{
    item: RepositoryItem;
    clickPos: Point2D;
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

            options.minLeft = as.Int(options.minLeft, 10);
            options.minTop = as.Int(options.minTop, 10);
            options.offsetLeft = as.Int(options.bottom, 0);
            options.offsetTop = as.Int(options.bottom, -60);
            options.width = as.Int(options.item.getProperties()[Pid.IframeWidth], 400);
            options.height = as.Int(options.item.getProperties()[Pid.IframeHeight], 400);

            log.debug('ItemFramePopup', url);
            super.show(options);

            $(this.windowElem).addClass('n3q-itemframepopup');

            let left = Math.max(options.clickPos.x - options.width / 2 + options.offsetLeft, options.minLeft);
            let top = Math.max(options.clickPos.y - options.height / 2 + options.offsetTop, options.minTop);

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
