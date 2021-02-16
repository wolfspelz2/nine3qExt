import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Config } from '../lib/Config';
import { Pid } from '../lib/ItemProperties';
import { ContentApp } from './ContentApp';

export class ItemStatsTooltip
{
    private elem: HTMLElement = null;
    private offset = Config.get('room.itemStatsTooltipOffset', { x: 0, y: 0 });

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
    }

    async show(x: number, y: number): Promise<void>
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-tooltip n3q-statstooltip" data-translate="children" />').get(0);
        $(this.elem).css({ display: 'none' });

        let props = await BackgroundMessage.getBackpackItemProperties(this.itemId);

        let label = as.String(props[Pid.Label], null);
        if (label == null) {
            let label = as.String(props[Pid.Template], null);
        }
        if (label) {
            let labelElem = <HTMLDivElement>$('<div class="n3q-base n3q-statstooltip-label" data-translate="text:ItemLabel">' + label + '</div>').get(0);
            $(this.elem).append(labelElem);
        }

        let stats = as.String(props[Pid.Stats], null);
        let statsPids = stats.split(' ');

        for (let i = 0; i < statsPids.length; i++) {
            let pid = statsPids[i];
            let value = props[pid];
            if (value) {
                let lineElem = <HTMLDivElement>$('<div class="n3q-base n3q-tooltip-line" data-translate="children">'
                    + '<span class="n3q-base n3q-statstooltip-key" data-translate="text:ItemPid">'
                    + pid + '</span><span class="n3q-base n3q-statstooltip-value">'
                    + as.Html(value) + '</span>'
                    + '</div>').get(0);
                $(this.elem).append(lineElem);
            }
        }

        this.app.translateElem(this.elem);
        this.moveTo(x, y);
        $(this.app.getDisplay()).append(this.elem);
        this.app.toFront(this.elem);
        $(this.elem).delay(600).fadeIn(200);
    }

    moveTo(x: number, y: number)
    {
        let height = $(this.elem).height()

        let left = x - this.offset.x;
        let top = y - height - this.offset.y;
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
