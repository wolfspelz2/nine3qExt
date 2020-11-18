import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Window } from './Window';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';

export class VidconfWindow extends Window
{
    private url: string;

    constructor(app: ContentApp)
    {
        super(app);
    }

    show(options: any)
    {
        options.titleText = this.app.translateText('Vidconfwindow.Video Conference', 'Video Conference');
        options.resizable = true;
        options.popoutable = true;

        super.show(options);

        let aboveElem: HTMLElement = options.above;
        let bottom = as.Int(options.bottom, Config.get('room.vidconfBottom', 200));
        let width = as.Int(options.width, Config.get('room.vidconfWidth', 600));
        let height = as.Int(options.height, Config.get('room.vidconfHeight', 400));

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-vidconfwindow');

            let left = 50;
            if (aboveElem) {
                left = Math.max(aboveElem.offsetLeft - 250, left);
            }
            let top = this.app.getDisplay().offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    height -= minTop - top;
                    top = minTop;
                }
            }

            this.url = options.url;
            this.url = encodeURI(this.url);
            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-vidconfwindow-content" src="' + this.url + ' " frameborder="0" allow="camera; microphone; display-capture"></iframe>').get(0);

            $(contentElem).append(iframeElem);

            this.app.translateElem(windowElem);

            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px', 'left': left + 'px', 'top': top + 'px' });
        }
    }

    popout(): void
    {
        let params = `scrollbars=no,resizable=yes,status=no,location=no,toolbar=no,menubar=no,width=600,height=300,left=100,top=100`;
        let popout = window.open(this.url, Utils.randomString(10), params);
        popout.focus();
        this.close();
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }
}
