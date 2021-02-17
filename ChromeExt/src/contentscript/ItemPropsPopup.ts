import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Config } from '../lib/Config';
import { Pid } from '../lib/ItemProperties';
import { ContentApp } from './ContentApp';
import { RoomItem } from './RoomItem';

export class ItemPropsPopup
{
    private elem: HTMLElement = null;

    constructor(protected app: ContentApp, protected parentElem: HTMLElement, protected itemId: string)
    {
    }

    async show(): Promise<void>
    {
        if (this.elem == null) {
            await this.setup();
        }

        this.app.toFront(this.elem);
        $(this.elem).show();
    }

    hide(): void
    {
        // $(this.elem).stop().fadeOut('fast');
        $(this.elem).remove();
        this.elem = null;
    }

    async setup(): Promise<void>
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops" data-translate="children" />').get(0);
        $(this.elem).css({ display: 'none' });

        let props = await BackgroundMessage.getBackpackItemProperties(this.itemId);

        let keys = [];
        for (let pid in props) { keys.push(pid); }
        keys = keys.sort();

        for (let i in keys) {
            let pid = keys[i]
            let value = props[pid];
            let lineElem = <HTMLDivElement>$('<div class="n3q-base n3q-itemprops-line" data-translate="children">'
                + '<span class="n3q-base n3q-itemprops-key">'
                + pid + '</span><span class="n3q-base n3q-itemprops-value">'
                + as.Html(value) + '</span>'
                + '</div>').get(0);
            $(this.elem).append(lineElem);
        }

        $(this.parentElem).append(this.elem);
    }
}
