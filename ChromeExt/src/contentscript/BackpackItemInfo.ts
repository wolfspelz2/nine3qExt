import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Pid } from '../lib/ItemProperties';
import { BackpackItem } from './BackpackItem';
import { ContentApp } from './ContentApp';

export class BackpackItemInfo
{
    private elem: HTMLElement = null;
    private hasStats = false;

    constructor(protected app: ContentApp, protected backpackItem: BackpackItem)
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

    hide(): void
    {
        $(this.elem).remove();
        this.elem = null;
    }

    setup(): void
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops n3q-backpackiteminfo n3q-shadow-small" data-translate="children" />').get(0);
        $(this.elem).css({ display: 'none' });

        let props = this.backpackItem.getProperties();

        let label = as.String(props[Pid.Label], null);
        if (label == null) {
            let label = as.String(props[Pid.Template], null);
        }
        if (label) {
            let labelElem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops-title" data-translate="text:ItemLabel">' + label + '</div>').get(0);
            $(this.elem).append(labelElem);
        }

        let statsPids = [Pid.IsRezzed, Pid.RezzedDestination];
        let listElem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops-list" data-translate="children" />').get(0);
        let hasStats = false;
        for (let i = 0; i < statsPids.length; i++) {
            let pid = statsPids[i];
            let value = props[pid];
            if (value) {
                hasStats = true;

                if (pid == Pid.RezzedDestination) {
                    if (value.startsWith('http://')) { value = value.substr('http://'.length); }
                    if (value.startsWith('https://')) { value = value.substr('https://'.length); }
                    if (value.startsWith('www.')) { value = value.substr('www.'.length); }
                }

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

        let destination = as.String(props[Pid.RezzedDestination], null);
        if (destination) {
            let goElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-backpack-go" data-translate="text:Backpack">Go to item</div>').get(0);
            $(goElem).on('click', () =>
            {
                window.location.assign(destination);
            });
            $(this.elem).append(goElem);
        }

        this.app.translateElem(this.elem);
        $(this.backpackItem.getElem()).append(this.elem);
    }
}
