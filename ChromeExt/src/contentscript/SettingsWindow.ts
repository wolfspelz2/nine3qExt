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
        let bottom = as.Int(options.bottom, 200);
        let width = as.Int(options.width, 430);
        let height = as.Int(options.height, 410);

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-settingswindow');
            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px' });

            let uri = 'chrome-extension://' + chrome.runtime.id + '/popup.html';
            let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-settingswindow-content" style="width: 430px; height: 380px;" src="' + uri + ' " frameborder="0"></iframe>').get(0);

            $(contentElem).append(iframeElem);

            this.app.translateElem(windowElem);

            if (aboveElem) {
                let left = aboveElem.offsetLeft - 180;
                if (left < 0) { left = 0; }
                let screenHeight = this.display.offsetHeight;
                let top = screenHeight - height - bottom;
                $(windowElem).css({ left: left + 'px', top: top + 'px' });
            }
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }
}
