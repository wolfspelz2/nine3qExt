import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Window } from './Window';
import { RepositoryItem } from './RepositoryItem';
import { Pid } from '../lib/ItemProperties';

type WindowOptions = any;

interface ItemFrameWindowOptions extends WindowOptions
{
    item: RepositoryItem;
    elem: HTMLElement;
    url: string;
    onClose: { (): void };
}

export class ItemFrameWindow extends Window
{
    protected refElem: HTMLElement;

    constructor(app: ContentApp)
    {
        super(app);
    }

    async show(options: ItemFrameWindowOptions)
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

            options.resizable = as.Bool(options.rezizable, true);
            options.titleText = as.String(options.item.getProperties().Label, 'Item');

            this.refElem = options.elem;

            log.debug('ItemFrameWindow', url);
            super.show(options);

            $(this.windowElem).addClass('n3q-itemframewindow');

            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-itemframewindow-content" src="' + url + ' " frameborder="0" allow="camera; microphone; fullscreen; display-capture"></iframe>').get(0);

            $(this.contentElem).append(iframeElem);
            this.app.translateElem(this.windowElem);

            this.position(options.width, options.height, options.left, options.bottom);

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

    position(width: number, height: number, left: number, bottom: number): void
    {
        let offset = this.refElem.getBoundingClientRect();
        let absLeft = offset.left + left;
        let absBottom = bottom;
        $(this.windowElem).css({ width: width + 'px', height: height + 'px', left: absLeft + 'px', bottom: absBottom + 'px' });
    }
}
