import * as $ from 'jquery';
import 'webpack-jquery-ui';
// import markdown = require('markdown');
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { ContentApp } from './ContentApp';
import { Room } from './Room';
import { Window } from './Window';
import { Payload } from '../lib/Payload';
import { Item } from './Item';

type WindowOptions = any;

interface ItemFrameWindowOptions extends WindowOptions
{
    item: Item;
    above: HTMLElement;
    resizable: boolean;
    titleText: string;
    url: string;
    width: number;
    height: number;
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

            let room = this.app.getRoom();
            if (!room) { throw 'No room' }

            let roomJid = room.getJid();

            let item = options.item;
            let providerId = item.getProviderId();
            let apiUrl = this.app.getItemProviderConfigValue(providerId, 'apiUrl', '');
            let userId = this.app.getItemProviderConfigValue(providerId, 'userToken', '');
            let itemId = item.getId();

            if (apiUrl == '') { throw 'No apiUrl' }
            if (userId == '') { throw 'No userId' }
            if (itemId == '') { throw 'No itemId' }

            let token = await Payload.getToken(apiUrl, userId, itemId, 3600, { 'room': roomJid });
            url = url.replace('{token}', encodeURIComponent(token));
            log.debug('ItemFrameWindow', url);

            super.show(options);

            let aboveElem: HTMLElement = options.above;
            let bottom = as.Int(options.bottom, 150);
            let width = as.Int(options.width, 400);
            let height = as.Int(options.height, 400);

            $(this.windowElem).addClass('n3q-itemframewindow');

            let left = 50;
            if (aboveElem) {
                left = Math.max(aboveElem.offsetLeft - 180, left);
            }
            let top = this.app.getDisplay().offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    top = minTop;
                }
            }

            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-itemframewindow-content" src="' + url + ' " frameborder="0"></iframe>').get(0);

            $(this.contentElem).append(iframeElem);

            this.app.translateElem(this.windowElem);

            $(this.windowElem).css({ 'width': width + 'px', 'height': height + 'px', 'left': left + 'px', 'top': top + 'px' });

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
