import * as $ from 'jquery';
import 'webpack-jquery-ui';
// import markdown = require('markdown');
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { Window } from './Window';

export class InventoryWindow extends Window
{
    private paneElem: HTMLElement;
    
    constructor(app: ContentApp)
    {
        super(app);
    }

    getDisplay() { return this.paneElem; }

    show(options: any)
    {
        options.titleText = this.app.translateText('InventoryWindow.Inventory', 'Your Stuff');
        options.resizable = true;

        super.show(options);

        let aboveElem: HTMLElement = options.above;
        let bottom = as.Int(options.bottom, 250);
        let width = as.Int(options.width, 400);
        let height = as.Int(options.height, 300);

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-inventorywindow');

            let left = 50;
            if (aboveElem) {
                left = aboveElem.offsetLeft - 120;
                if (left < 0) { left = 0; }
            }
            let top = this.app.getDisplay().offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    height -= minTop - top;
                    top = minTop;
                }
            }

            let paneElem = <HTMLElement>$('<div class="n3q-base n3q-inventorywindow-pane" data-translate="children" />').get(0);

            $(contentElem).append(paneElem);

            this.app.translateElem(windowElem);

            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px', 'left': left + 'px', 'top': top + 'px' });

            this.onResize = (ev: JQueryEventObject) =>
            {
            };

            this.paneElem = paneElem;
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }

}