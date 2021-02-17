import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Config } from '../lib/Config';
import { Pid } from '../lib/ItemProperties';
import { ContentApp } from './ContentApp';

export class ItemPropertiesTooltip
{
    private elem: HTMLElement = null;
    private offset = Config.get('backpack.itemPropertiesTooltipOffset', { x: 0, y: 0 });

    constructor(protected app: ContentApp, protected parentElem: HTMLElement, protected itemId: string, protected onClose: { (): void })
    {
    }

    async show(x: number, y: number): Promise<ItemPropertiesTooltip>
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-tooltip n3q-propstooltip" data-translate="children" />').get(0);
        $(this.elem).css({ display: 'none' });

        let props = await BackgroundMessage.getBackpackItemProperties(this.itemId);

        let keys = [];
        for (let pid in props) { keys.push(pid); }
        keys = keys.sort();

        for (let i in keys) {
            let pid = keys[i]
            let value = props[pid];
            let lineElem = <HTMLDivElement>$('<div class="n3q-base n3q-tooltip-line" data-translate="children">'
                + '<span class="n3q-base n3q-propstooltip-key" data-translate="text:ItemPid">'
                + pid + '</span><span class="n3q-base n3q-propstooltip-value">'
                + as.Html(value) + '</span>'
                + '</div>').get(0);
            $(this.elem).append(lineElem);
        }

        this.app.translateElem(this.elem);
        this.moveTo(x, y);
        $(this.parentElem).append(this.elem);
        this.app.toFront(this.elem);
        $(this.elem).fadeIn(100);

        return this;
    }

    moveTo(x: number, y: number)
    {
        let left = x - this.offset.x;
        let top = y - this.offset.y;
        $(this.elem).css({ top: top, left: left });
    }

    close(): void
    {
        if (this.elem != null) {
            $(this.elem).stop();
            $(this.elem).remove();
            if (this.onClose) { this.onClose(); }
            this.elem = null;
        }
    }
}
