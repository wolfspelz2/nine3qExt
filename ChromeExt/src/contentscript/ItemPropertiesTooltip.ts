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

    constructor(protected app: ContentApp, protected itemId: string, protected target: HTMLElement)
    {
        $(this.target).on({
            mousemove: (ev) =>
            {
                this.moveTo(ev.clientX, ev.clientY);
            },
            mouseleave: () => 
            {
                this.close();
            }
        });

        // $(this.elem).on({
        //     click: (ev) =>
        //     {
        //         this.detach();
        //     }
        // });
    }

    // detach()
    // {
    //     if (this.target) {
    //         $(this.target).off('mousemove', 'mouseleave');
    //         this.target = null;
    //     }
    // }

    async show(x: number, y: number): Promise<void>
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
        $(this.app.getDisplay()).append(this.elem);
        this.app.toFront(this.elem);
        $(this.elem).delay(400).fadeIn(200);
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
            this.elem = null;
        }
    }
}
