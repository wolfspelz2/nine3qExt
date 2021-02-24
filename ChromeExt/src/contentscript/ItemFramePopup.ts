import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D, Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Popup } from './Popup';
import { RepositoryItem } from './RepositoryItem';
import { Pid } from '../lib/ItemProperties';
import { Config } from '../lib/Config';

type PopupOptions = any;

interface ItemFramePopupOptions extends PopupOptions
{
    item: RepositoryItem;
    elem: HTMLElement;
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

            let json = as.String(options.item.getProperties()[Pid.IframeOptions], '{}');
            let iframeOptions = JSON.parse(json);

            options.width = as.Int(iframeOptions.width, 100);
            options.height = as.Int(iframeOptions.height, 100);
            options.left = as.Int(iframeOptions.left, -options.width / 2);
            options.bottom = as.Int(iframeOptions.bottom, 50);

            log.debug('ItemFramePopup', url);
            super.show(options);

            $(this.windowElem).addClass('n3q-itemframepopup');

            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-itemframepopup-content" src="' + url + ' " frameborder="0"></iframe>').get(0);

            $(this.windowElem).append(iframeElem);
            this.app.translateElem(this.windowElem);

            this.position(options.width, options.height, options.left, options.bottom);

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

    position(width: number, height: number, left: number, bottom: number): void
    {
        $(this.windowElem).css({ width: width + 'px', height: height + 'px', left: left + 'px', bottom: bottom + 'px' });
    }
}
