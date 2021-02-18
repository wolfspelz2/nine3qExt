import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Pid } from '../lib/ItemProperties';
import { ContentApp } from './ContentApp';
import { RoomItem } from './RoomItem';

export class RoomItemStats
{
    private elem: HTMLElement = null;

    constructor(protected app: ContentApp, protected roomItem: RoomItem, protected onClose: () => void)
    {
    }

    show(): void
    {
        if (this.elem == null) {
            this.setup();
        }

        this.app.toFront(this.elem);
        $(this.elem).stop().fadeIn('fast');
    }

    close(): void
    {
        // $(this.elem).stop().fadeOut('fast');
        $(this.elem).remove();
        if (this.onClose) { this.onClose(); }
    }

    setup(): void
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops n3q-roomitemstats n3q-shadow-small" data-translate="children" />').get(0);
        $(this.elem).css({ display: 'none' });

        let props = this.roomItem.getProperties();

        let label = as.String(props[Pid.Label], null);
        if (label == null) {
            let label = as.String(props[Pid.Template], null);
        }
        if (label) {
            let labelElem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops-title" data-translate="text:ItemLabel">' + label + '</div>').get(0);
            $(this.elem).append(labelElem);
        }

        let stats = as.String(props[Pid.Stats], null);
        let statsPids = stats.split(' ');

        let listElem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops-list" data-translate="children" />').get(0);
        let hasStats = false;
        for (let i = 0; i < statsPids.length; i++) {
            let pid = statsPids[i];
            let value = props[pid];
            if (value) {
                hasStats = true;
                let lineElem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops-line" data-translate="children">'
                    + '<span class="n3q-base n3q-itemprops-key" data-translate="text:ItemPid">'
                    + pid + '</span><span class="n3q-base n3q-itemprops-value" data-translate="text:ItemValue" title="'
                    + as.Html(value) + '">'
                    + as.Html(value) + '</span>'
                    + '</div>').get(0);
                $(listElem).append(lineElem);
            }
        }
        if (hasStats) {
            $(this.elem).append(listElem);
        }

        this.app.translateElem(this.elem);
        $(this.roomItem.getElem()).append(this.elem);
    }
}
