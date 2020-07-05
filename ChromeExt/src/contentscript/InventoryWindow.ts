import * as $ from 'jquery';
import 'webpack-jquery-ui';
// import markdown = require('markdown');
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { Window } from './Window';
import { Inventory } from './Inventory';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Environment } from '../lib/Environment';

export class InventoryWindow extends Window
{
    private paneElem: HTMLElement;

    constructor(app: ContentApp, private inv: Inventory)
    {
        super(app);
    }

    getPane() { return this.paneElem; }

    async show(options: any)
    {
        options = await this.getSavedOptions(options, InventoryWindow.name);

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
                left = Math.max(aboveElem.offsetLeft - 120, left);
            }
            let top = this.app.getDisplay().offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    height -= minTop - top;
                    top = minTop;
                }
            }

            let paneElem = <HTMLElement>$('<div class="n3q-base n3q-inventory-pane" data-translate="children" />').get(0);
            let textElem = <HTMLElement>$('<div class="n3q-base n3q-text n3q-inventory-noitems" data-translate="text:Inventory">No items</div>').get(0);
            $(paneElem).append(textElem);
            $(contentElem).append(paneElem);

            // if (!this.inv.getAvailable()) {
            //     let url = this.app.getItemProviderConfigValue(this.inv.getProviderId(), 'unavailableUrl', '');
            //     if (url != '') {
            //         let uniqueId = await Config.getSync('me.id', '');
            //         url = url.replace('{id}', encodeURIComponent(uniqueId));
            //         let iframeElem = <HTMLElement>$('<iframe class="n3q-base n3q-inventory-iframe" src="' + url + ' " frameborder="0"></iframe>').get(0);
            //         $(contentElem).append(iframeElem);
            //     }
            // }

            this.app.translateElem(windowElem);

            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px', 'left': left + 'px', 'top': top + 'px' });

            this.onResizeStop = (ev: JQueryEventObject, ui: JQueryUI.ResizableUIParams) =>
            {
                let left = ui.position.left;
                let bottom = this.app.getDisplay().offsetHeight - (ui.position.top + ui.size.height);
                this.saveCoordinates(left, bottom, ui.size.width, ui.size.height);
            };

            this.onDragStop = (ev: JQueryEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                let size = { width: $(this.windowElem).width(), height: $(this.windowElem).height() }
                let left = ui.position.left;
                let bottom = this.app.getDisplay().offsetHeight - (ui.position.top + size.height);
                this.saveCoordinates(left, bottom, size.width, size.height);
            };

            $(paneElem).droppable({
                drop: (ev: JQueryEventObject, ui: JQueryUI.DroppableEventUIParam) =>
                {
                    let droppedAvatar = ui.draggable.get(0);
                    if (droppedAvatar) {
                        let droppedEntity = droppedAvatar.parentElement;
                        if (droppedEntity) {
                            let droppedId: string = $(droppedEntity).data('nick');
                            if (droppedId) {
                                let roomItem = this.app.getRoom().getItem(droppedId);
                                if (roomItem) {
                                    let x = Math.round(ui.offset.left - $(paneElem).offset().left + ui.draggable.width() / 2);
                                    let y = Math.round(ui.offset.top - $(paneElem).offset().top + ui.draggable.height() / 2)
                                    roomItem.beginDerez();
                                    this.inv.sendDerezItem(roomItem.getNick(), x, y);
                                }
                            }
                        }
                    }
                }
            });

            this.paneElem = paneElem;
        }
    }

    setCoordinates(left: number, bottom: number, width: number, height: number)
    {
        let coords = {};
        if (left > 0) { coords['left'] = left; }
        if (bottom > 0 && height > 0) { coords['top'] = this.app.getDisplay().offsetHeight - bottom - height; }
        if (width > 0) { coords['width'] = width; }
        if (height > 0) { coords['height'] = height; }

        $(this.windowElem).css(coords);
    }

    async saveCoordinates(left: number, bottom: number, width: number, height: number)
    {
        await this.saveOption(InventoryWindow.name, 'left', left);
        await this.saveOption(InventoryWindow.name, 'bottom', bottom);
        await this.saveOption(InventoryWindow.name, 'width', width);
        await this.saveOption(InventoryWindow.name, 'height', height);
    }

    setStatus(status: string, text?: string, detail?: { type: string; from: string; to: string; }): void
    {
        $('.n3q-inventory-pane .n3q-inventory-noitems').remove();

        if (Environment.isDevelopment()) {
            $(this.paneElem).find('.n3q-inventory-status').remove();

            if (status == 'error') {
                let elem = <HTMLDivElement>$(
                    `<div class="n3q-base n3q-inventory-status">
                        <div class="n3q-base n3q-text">` + as.Html('error: ' + detail.type, '') + `</div>
                        <div class="n3q-base n3q-text">` + as.Html(text, '') + `</div>
                        <div class="n3q-base n3q-text">` + as.Html('from: ' + detail.from, '') + `</div>
                    </div>`
                ).get(0);
                $(this.paneElem).append(elem);
            }
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }

}
