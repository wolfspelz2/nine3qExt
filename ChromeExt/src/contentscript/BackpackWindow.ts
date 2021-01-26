import * as $ from 'jquery';
import 'webpack-jquery-ui';
// import markdown = require('markdown');
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { ItemProperties } from '../lib/ItemProperties';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { ItemChangeOptions } from '../lib/ItemChangeOptions';
import { ContentApp } from './ContentApp';
import { Window } from './Window';
import { BackpackItem as BackpackItem } from './BackpackItem';
import { Environment } from '../lib/Environment';
import { ItemException } from '../lib/ItemExcption';
import { ItemExceptionToast, SimpleErrorToast } from './Toast';

import shredderImage from '../assets/Shredder.png';

export class BackpackWindow extends Window
{
    private paneElem: HTMLElement;
    private items: { [id: string]: BackpackItem; } = {};

    constructor(app: ContentApp)
    {
        super(app);
    }

    getPane() { return this.paneElem; }

    async show(options: any)
    {
        options = await this.getSavedOptions(BackpackWindow.name, options);

        options.titleText = this.app.translateText('BackpackWindow.Inventory', 'Local Stuff');
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

            let left = as.Int(options.left, 50);
            if (options.left == null) {
                if (aboveElem) {
                    left = Math.max(aboveElem.offsetLeft - 120, left);
                }
            }
            let top = this.app.getDisplay().offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    height -= minTop - top;
                    top = minTop;
                }
            }

            let paneElem = <HTMLElement>$('<div class="n3q-base n3q-backpack-pane" data-translate="children" />').get(0);
            $(contentElem).append(paneElem);

            if (Environment.isDevelopment()) {
                let inElem = <HTMLElement>$('<textarea class="n3q-base n3q-backpack-in n3q-input n3q-text" />').get(0);
                $(inElem).hide();
                $(contentElem).append(inElem);

                let toggleElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-backpack-button n3q-backpack-toggle">Input</div>').get(0);
                $(contentElem).append(toggleElem);
                $(toggleElem).on('click', () =>
                {
                    if ($(inElem).is(':hidden')) {
                        $(inElem).show();
                    } else {
                        $(inElem).hide();
                    }
                });

                let addElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-backpack-button n3q-backpack-add">Add</div>').get(0);
                $(contentElem).append(addElem);
                $(addElem).on('click', () =>
                {
                    let text = as.String($(inElem).val(), '');
                    text = text.replace(/'/g, '"',);
                    let json = JSON.parse(text);
                    let itemId = Utils.randomString(20);
                    json.Id = itemId;
                    this.createItem(itemId, json, {});
                });

                let dumpElem = <HTMLElement>$('<div class="n3q-base n3q-backpack-dump" />').get(0);
                $(dumpElem).css({ backgroundImage: 'url(' + shredderImage + ')' });
                $(contentElem).append(dumpElem);
                $(dumpElem).droppable({
                    hoverClass: 'n3q-backpack-dump-drophilite',
                    tolerance: 'pointer',
                    drop: async (ev: JQueryEventObject, ui: JQueryUI.DroppableEventUIParam) =>
                    {
                        let droppedItem = ui.draggable.get(0);
                        if (droppedItem) {
                            let droppedId: string = $(droppedItem).data('id');
                            if (droppedId) {
                                this.deleteItem(droppedId);
                                ev.stopPropagation();
                            }
                        }
                    }
                });
            }

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
                                    // roomItem.beginDerez();
                                    this.derezItem(roomItem.getNick(), roomItem.getRoom().getJid(), x, y);
                                }
                            }
                        }
                    }
                }
            });

            this.paneElem = paneElem;

            try {
                let response = await BackgroundMessage.getBackpackState();
                if (response && response.ok) {
                    this.populate(response.items);
                }
            } catch (ex) {

            }
        }
    }

    populate(items: { [id: string]: ItemProperties; })
    {
        for (let id in items) {
            this.onShowItem(id, items[id]);
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
        await this.saveOptions(BackpackWindow.name, { 'left': left, 'bottom': bottom, 'width': width, 'height': height });
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }

    onShowItem(id: string, properties: ItemProperties)
    {
        let item = this.items[id];
        if (!item) {
            item = new BackpackItem(this.app, this, id, properties);
            this.items[id] = item;
        }
        item.create();
    }

    onSetItem(id: string, properties: ItemProperties)
    {
        if (this.items[id]) {
            this.items[id].applyProperties(properties);
        }
    }

    onHideItem(id: string)
    {
        if (this.items[id]) {
            this.items[id].destroy();
            delete this.items[id];
        }
    }

    createItem(id: string, properties: ItemProperties, options: ItemChangeOptions)
    {
        BackgroundMessage.addBackpackItem(id, properties, options);
    }

    setItemProperties(id: string, properties: ItemProperties, options: ItemChangeOptions)
    {
        BackgroundMessage.setBackpackItemProperties(id, properties, options);
    }

    rezItem(id: string, room: string, x: number, destination: string) { this.rezItemAsync(id, room, x, destination); }
    async rezItemAsync(id: string, room: string, x: number, destination: string)
    {
        log.debug('BackpackWindow', 'rezItem', id, 'to', room);

        try {
            await BackgroundMessage.rezBackpackItem(id, room, x, destination, {});
        } catch (ex) {
            new ItemExceptionToast(this.app, Config.get('room.errorToastDurationSec', 8), ex).show();
        }
    }

    async derezItem(id: string, room: string, x: number, y: number)
    {
        log.debug('BackpackWindow', 'derezItem', id, 'from', room);

        try {
            await BackgroundMessage.derezBackpackItem(id, room, -1, -1, {});
        } catch (ex) {
            new ItemExceptionToast(this.app, Config.get('room.errorToastDurationSec', 8), ex).show();
        }
    }

    async deleteItem(id: string)
    {
        log.debug(BackpackWindow.name, this.deleteItem.name, id);

        try {
            await BackgroundMessage.deleteBackpackItem(id, {});
        } catch (ex) {
            new ItemExceptionToast(this.app, Config.get('room.errorToastDurationSec', 8), ex).show();
        }
    }
}
