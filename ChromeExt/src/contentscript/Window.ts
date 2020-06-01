import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';

export class Window
{
    onResize: { (ev: JQueryEventObject): void };
    onDragStop: { (ev: JQueryEventObject): void };
    onClose: { (): void };

    protected windowElem: HTMLElement;
    protected contentElem: HTMLElement;

    constructor(protected app: ContentApp, protected display: HTMLElement) { }

    show(options: any)
    {
        this.onClose = options.onClose;

        if (!this.windowElem) {
            let windowId = Utils.randomString(15);
            let resizable = as.Bool(options.resizable, false);

            let windowElem = <HTMLElement>$('<div id="' + windowId + '" class="n3q-base n3q-window n3q-chatwindow n3q-shadow-medium" data-translate="children" />').get(0);
            let titleBarElem = <HTMLElement>$('<div class="n3q-base n3q-window-title-bar" data-translate="children" />').get(0);
            let titleElem = <HTMLElement>$('<div class="n3q-base n3q-window-title" data-translate="children" />').get(0);
            let titleTextElem = <HTMLElement>$('<div class="n3q-base n3q-window-title-text">' + (options.titleText ? options.titleText : '') + '</div>').get(0);
            let closeElem = <HTMLElement>$(
                `<div class="n3q-base n3q-window-button" title="Close" data-translate="attr:title:Common">
                    <div class="n3q-base n3q-button-symbol n3q-button-close" />
                </div>`
            ).get(0);
            let contentElem = <HTMLElement>$('<div class="n3q-base n3q-window-content" data-translate="children" />').get(0);
            let resizeElem = resizable ? <HTMLElement>$('<div class="n3q-base n3q-window-resize n3q-window-resize-se"/>').get(0) : null;

            $(titleElem).append(titleTextElem);
            $(titleBarElem).append(titleElem);
            $(titleBarElem).append(closeElem);
            $(windowElem).append(titleBarElem);

            $(windowElem).append(contentElem);
            $(windowElem).append(resizeElem);

            this.contentElem = contentElem;
            this.windowElem = windowElem;

            $(this.display).append(windowElem);

            let maskId = Utils.randomString(15);

            if (resizable) {
                $(windowElem).resizable({
                    minWidth: 180,
                    minHeight: 30,
                    handles: {
                        'se': '#n3q #' + windowId + ' .n3q-window-resize-se',
                    },
                    resize: (ev: JQueryEventObject) =>
                    {
                        if (this.onResize) { this.onResize(ev); }
                    },
                    start: (ev: JQueryEventObject) =>
                    {
                        $(windowElem).append('<div id="' + maskId + '" style="background-color: #ffffff; opacity: 0.001; position: absolute; left: 0; top: 0; right: 0; bottom: 0;"></div>');
                    },
                    stop: (ev: JQueryEventObject) =>
                    {
                        $('#' + maskId).remove();
                    },
                });
            }

            $(closeElem).click(ev =>
            {
                this.close();
            });

            $(windowElem).draggable({
                handle: '.n3q-window-title',
                scroll: false,
                iframeFix: true,
                stack: '.n3q-entity',
                // opacity: 0.5,
                distance: 4,
                containment: 'document',
                stop: (ev: JQueryEventObject) =>
                {
                    if (this.onDragStop) { this.onDragStop(ev); }
                },
            });
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }


    close(): void
    {
        if (this.onClose) { this.onClose(); }

        $(this.windowElem).remove();
        this.windowElem = null;
    }
}
