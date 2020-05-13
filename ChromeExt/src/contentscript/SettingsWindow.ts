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

        if (this.windowElem) {
            let window = this.windowElem;
            let content = this.contentElem;
            $(window).addClass('n3q-settingswindow');
            $(window).css({ 'width': '430px', 'height': '410px' });

            let uri = 'chrome-extension://' + chrome.runtime.id + '/popup.html';
            let iframe = <HTMLElement>$('<iframe class="n3q-base n3q-settingswindow-content" style="width: 430px; height: 380px;" src="' + uri + ' " frameborder="0"></iframe>').get(0);

            $(content).append(iframe);

            this.app.translateElem(window);

            if (aboveElem) {
                let left = aboveElem.offsetLeft - 180;
                if (left < 0) { left = 0; }
                let screenHeight = this.display.offsetHeight;
                let top = this.display.offsetHeight - 600;
                $(window).css({ left: left + 'px', top: top + 'px' });
            }
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }
}
