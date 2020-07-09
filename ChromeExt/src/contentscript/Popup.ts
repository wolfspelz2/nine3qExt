import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';

type PopupOptions = any;

export class Popup
{
    onClose: { (): void };

    protected windowElem: HTMLElement;

    constructor(protected app: ContentApp) { }

    show(options: PopupOptions)
    {
        this.onClose = options.onClose;

        if (!this.windowElem) {
            let windowId = Utils.randomString(15);

            let windowElem = <HTMLElement>$('<div id="' + windowId + '" class="n3q-base n3q-window n3q-chatwindow n3q-shadow-medium" data-translate="children" />').get(0);

            let closeElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-overlay n3q-shadow-small" title="Close" data-translate="attr:title:Common"><div class="n3q-base n3q-button-symbol n3q-button-close-small" />').get(0);
            $(closeElem).click(ev =>
            {
                this.close();
                ev.stopPropagation();
            });

            $(windowElem).append(closeElem);

            this.windowElem = windowElem;

            $(this.app.getDisplay()).append(windowElem);

            let maskId = Utils.randomString(15);

            this.isClosing = false;
            $(closeElem).click(ev =>
            {
                this.close();
            });

            $(windowElem).click(ev =>
            {
                this.app.toFront(windowElem);
            });
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }

    private isClosing: boolean;
    close(): void
    {
        if (!this.isClosing) {
            this.isClosing = true;

            if (this.onClose) { this.onClose(); }
            $(this.windowElem).remove();
            this.windowElem = null;
        }
    }
}
