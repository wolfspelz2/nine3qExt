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

export class SettingsWindow extends Window
{
    constructor(app: ContentApp, display: HTMLElement)
    {
        super(app, display);
    }

    show(options: any)
    {
        options.titleText = this.app.translateText('Settingswindow.Settings', 'Settings');
        options.resizable = false;

        super.show(options);

        let aboveElem: HTMLElement = options.above;
        let bottom = as.Int(options.bottom, 150);
        let width = as.Int(options.width, 420);
        let height = as.Int(options.height, 410);

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-settingswindow');

            let left = 50;
            if (aboveElem) {
                left = aboveElem.offsetLeft - 180;
                if (left < 0) { left = 0; }
            }
            let top = this.display.offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    top = minTop;
                }
            }

            let uri = 'chrome-extension://' + chrome.runtime.id + '/popup.html';
            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-settingswindow-content" style="width: 420px; height: 380px;" src="' + uri + ' " frameborder="0"></iframe>').get(0);

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
