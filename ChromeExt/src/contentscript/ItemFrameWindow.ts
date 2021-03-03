import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D, Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Window } from './Window';
import { RepositoryItem } from './RepositoryItem';
import { Pid } from '../lib/ItemProperties';
import { Config } from '../lib/Config';

type WindowOptions = any;

export interface ItemFrameWindowOptions extends WindowOptions
{
    item: RepositoryItem;
    elem: HTMLElement;
    url: string;
    onClose: { (): void };
}

export class ItemFrameWindow extends Window
{
    protected refElem: HTMLElement;
    private url: string;
    private title: string;
    private width = 400;
    private height = 400;

    constructor(app: ContentApp)
    {
        super(app);
    }

    async show(options: ItemFrameWindowOptions)
    {
        try {
            let url: string = options.url;
            if (!url) { throw 'No url' }

            this.refElem = options.elem;

            log.debug('ItemFrameWindow', url);
            super.show(options);

            $(this.windowElem).addClass('n3q-itemframewindow');

            this.title = options.titleText; // member for undock
            this.url = options.url; // member for undock
            this.width = options.width; // member for undock
            this.height = options.height; // member for undock

            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-itemframewindow-content" src="' + this.url + ' " frameborder="0" allow="camera; microphone; fullscreen; display-capture"></iframe>').get(0);

            $(this.contentElem).append(iframeElem);
            this.app.translateElem(this.windowElem);

            this.position(options.width, options.height, options.left, options.bottom);

            this.app.toFront(this.windowElem, ContentApp.LayerWindow)

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

    undock(): void
    {
        let left = Config.get('roomItem.frameUndockedLeft', 100);
        let top = Config.get('roomItem.frameUndockedTop', 100);
        let width = this.width;
        let height = this.height;
        let params = 'scrollbars=no,resizable=yes,status=no,location=no,toolbar=no,menubar=no,width=' + width + ',height=' + height + ',left=' + left + ',top=' + top + '';

        let url = this.url;
        let title = this.title;

        this.close();

        let undocked = window.open(url, Utils.randomString(10), params);
        undocked.focus();
    }
}
