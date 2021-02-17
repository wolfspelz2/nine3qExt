import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Config } from '../lib/Config';
import { Pid } from '../lib/ItemProperties';
import { ContentApp } from './ContentApp';
import { RoomItem } from './RoomItem';

export class ItemStats
{
    private elem: HTMLElement = null;
    private hasStats = false;

    constructor(protected app: ContentApp, protected roomItem: RoomItem)
    {
    }

    show(): void
    {
        if (this.elem == null) {
            this.setup();
        }

        if (this.hasStats) {
            this.app.toFront(this.elem);
            $(this.elem).stop().fadeIn('fast');
        }
    }

    hide(): void
    {
        // $(this.elem).stop().fadeOut('fast');
        $(this.elem).remove();
        this.elem = null;
    }

    setup(): void
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-itemstats n3q-shadow-small" data-translate="children" />').get(0);
        $(this.elem).css({ display: 'none' });

        let props = this.roomItem.getProperties();

        let label = as.String(props[Pid.Label], null);
        if (label == null) {
            let label = as.String(props[Pid.Template], null);
        }
        if (label) {
            let labelElem = <HTMLDivElement>$('<div class="n3q-base n3q-itemstats-label" data-translate="text:ItemLabel">' + label + '</div>').get(0);
            $(this.elem).append(labelElem);
        }

        let stats = as.String(props[Pid.Stats], null);
        let statsPids = stats.split(' ');

        for (let i = 0; i < statsPids.length; i++) {
            let pid = statsPids[i];
            let value = props[pid];
            if (value) {
                this.hasStats = true;
                let lineElem = <HTMLDivElement>$('<div class="n3q-base n3q-itemstats-line" data-translate="children">'
                    + '<span class="n3q-base n3q-itemstats-key" data-translate="text:ItemPid">'
                    + pid + '</span><span class="n3q-base n3q-itemstats-value">'
                    + as.Html(value) + '</span>'
                    + '</div>').get(0);
                $(this.elem).append(lineElem);
            }
        }

        this.app.translateElem(this.elem);
        $(this.roomItem.getElem()).append(this.elem);
        this.app.toFront(this.elem);
    }
}
