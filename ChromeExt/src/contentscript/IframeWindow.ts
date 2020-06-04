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

export class IframeWindow extends Window
{
    constructor(app: ContentApp)
    {
        super(app);
    }

    show(options: any)
    {
        super.show(options);

        let aboveElem: HTMLElement = options.above;
        let bottom = as.Int(options.bottom, 150);
        let width = as.Int(options.width, 400);
        let height = as.Int(options.height, 400);

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-iframewindow');

            let left = 50;
            if (aboveElem) {
                left = aboveElem.offsetLeft - 180;
                if (left < 0) { left = 0; }
            }
            let top = this.app.getDisplay().offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    top = minTop;
                }
            }

            let url: string = options.url;
 
            let room = this.app.getRoom();
            if (room) {
                let jid = room.getJid();
                url = url.replace('{room}', jid);
            }

            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-iframewindow-content" src="' + url + ' " frameborder="0"></iframe>').get(0);

            $(contentElem).append(iframeElem);

            this.app.translateElem(windowElem);

            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px', 'left': left + 'px', 'top': top + 'px' });
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }
}
