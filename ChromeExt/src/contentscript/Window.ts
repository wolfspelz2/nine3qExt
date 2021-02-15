import * as $ from 'jquery';
import 'webpack-jquery-ui';
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { Memory } from '../lib/Memory';

type WindowOptions = any;

export class Window
{
    onResizeStart: { (ev: JQueryEventObject, ui: JQueryUI.ResizableUIParams): void };
    onResizeStop: { (ev: JQueryEventObject, ui: JQueryUI.ResizableUIParams): void };
    onResize: { (ev: JQueryEventObject, ui: JQueryUI.ResizableUIParams): void };
    onDragStart: { (ev: JQueryEventObject, ui: JQueryUI.DraggableEventUIParams): void };
    onDrag: { (ev: JQueryEventObject, ui: JQueryUI.DraggableEventUIParams): void };
    onDragStop: { (ev: JQueryEventObject, ui: JQueryUI.DraggableEventUIParams): void };
    onClose: { (): void };

    protected windowElem: HTMLElement;
    protected contentElem: HTMLElement;

    constructor(protected app: ContentApp) { }

    show(options: WindowOptions)
    {
        this.onClose = options.onClose;

        if (!this.windowElem) {
            let windowId = Utils.randomString(15);
            let resizable = as.Bool(options.resizable, false);
            let undockable = as.Bool(options.undockable, false);

            let windowElem = <HTMLElement>$('<div id="' + windowId + '" class="n3q-base n3q-window n3q-shadow-medium" data-translate="children" />').get(0);
            let titleBarElem = <HTMLElement>$('<div class="n3q-base n3q-window-title-bar" data-translate="children" />').get(0);
            let titleElem = <HTMLElement>$('<div class="n3q-base n3q-window-title" data-translate="children" />').get(0);
            let titleTextElem = <HTMLElement>$('<div class="n3q-base n3q-window-title-text">' + (options.titleText ? options.titleText : '') + '</div>').get(0);

            let undockElem = undockable ? <HTMLElement>$(
                `<div class="n3q-base n3q-window-button n3q-window-button-2" title="Undock" data-translate="attr:title:Common">
                    <div class="n3q-base n3q-button-symbol n3q-button-undock" />
                </div>`
            ).get(0) : null;

            let closeElem = <HTMLElement>$(
                `<div class="n3q-base n3q-window-button" title="Close" data-translate="attr:title:Common">
                    <div class="n3q-base n3q-button-symbol n3q-button-close" />
                </div>`
            ).get(0);

            let contentElem = <HTMLElement>$('<div class="n3q-base n3q-window-content" data-translate="children" />').get(0);
            let resizeElem = resizable ? <HTMLElement>$('<div class="n3q-base n3q-window-resize n3q-window-resize-se"/>').get(0) : null;

            $(titleElem).append(titleTextElem);
            $(titleBarElem).append(titleElem);
            if (undockable) { $(titleBarElem).append(undockElem); }
            $(titleBarElem).append(closeElem);
            $(windowElem).append(titleBarElem);

            $(windowElem).append(contentElem);
            $(windowElem).append(resizeElem);

            this.contentElem = contentElem;
            this.windowElem = windowElem;

            $(this.app.getDisplay()).append(windowElem);
            this.app.toFront(windowElem);

            let maskId = Utils.randomString(15);

            if (resizable) {
                $(windowElem).resizable({
                    minWidth: 180,
                    minHeight: 30,
                    handles: {
                        'se': '#n3q #' + windowId + ' .n3q-window-resize-se',
                    },
                    start: (ev: JQueryEventObject, ui: JQueryUI.ResizableUIParams) =>
                    {
                        $(windowElem).append('<div id="' + maskId + '" style="background-color: #ffffff; opacity: 0.001; position: absolute; left: 0; top: 0; right: 0; bottom: 0;"></div>');
                        if (this.onResize) { this.onResize(ev, ui); }
                    },
                    resize: (ev: JQueryEventObject, ui: JQueryUI.ResizableUIParams) =>
                    {
                        if (this.onResizeStart) { this.onResizeStart(ev, ui); }
                    },
                    stop: (ev: JQueryEventObject, ui: JQueryUI.ResizableUIParams) =>
                    {
                        $('#' + maskId).remove();
                        if (this.onResizeStop) { this.onResizeStop(ev, ui); }
                    },
                });
            }

            $(undockElem).click(ev =>
            {
                this.undock();
            });

            this.isClosing = false;
            $(closeElem).click(ev =>
            {
                this.close();
            });

            $(windowElem).click(ev =>
            {
                this.app.toFront(windowElem);
            });

            $(windowElem).draggable({
                handle: '.n3q-window-title',
                scroll: false,
                iframeFix: true,
                stack: '.n3q-entity',
                // opacity: 0.5,
                distance: 4,
                containment: 'document',
                start: (ev: JQueryEventObject, ui: JQueryUI.DraggableEventUIParams) =>
                {
                    this.app.toFront(windowElem);
                    if (this.onDragStart) { this.onDragStart(ev, ui); }
                },
                drag: (ev: JQueryEventObject, ui: JQueryUI.DraggableEventUIParams) =>
                {
                    if (this.onDrag) { this.onDrag(ev, ui); }
                },
                stop: (ev: JQueryEventObject, ui: JQueryUI.DraggableEventUIParams) =>
                {
                    if (this.onDragStop) { this.onDragStop(ev, ui); }
                },
            });
        }
    }

    async getSavedOptions(name: string, presetOptions : any): Promise<any>
    {
        let savedOptions = await Memory.getLocal('window.' + name, null);
        let options = presetOptions;
        for (let key in savedOptions) {
            options[key] = savedOptions[key];
        }
        return options;
    }

    async saveOptions(name: string, value: any): Promise<void>
    {
        await Memory.setLocal('window.' + name, value);
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }

    undock(): void
    {
        let params = `scrollbars=no,resizable=yes,status=no,location=no,toolbar=no,menubar=no,width=600,height=300,left=100,top=100`;
        let undocked = window.open('about:blank', Utils.randomString(10), params);
        undocked.focus();
        undocked.onload = function ()
        {
            let html = `<div style="font-size:30px">Undocked</div>`;
            undocked.document.body.insertAdjacentHTML('afterbegin', html);
        };
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
