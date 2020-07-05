import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { ItemFrameWindow } from './ItemFrameWindow';

import imgDefaultItem from '../assets/DefaultItem.png';
import { timeStamp } from 'console';

export class Item
{
    private providerId: string;
    private properties: { [pid: string]: string } = {};
    private frameWindow: ItemFrameWindow;

    constructor(private app: ContentApp, private id: string)
    {
    }

    getId() { return this.id; }

    setProviderId(providerId: string) { this.providerId = providerId; }
    getProviderId(): string { return this.providerId; }

    setProperties(properties: { [pid: string]: string; }) { this.properties = properties; }
    getProperties(): any { return this.properties; }

    onClick(clickedElem: HTMLElement = null)
    {
        if (as.Bool(this.properties['IframeAspect'], false)) {
            this.openIframe(clickedElem);
        }
    }

    openIframe(aboveElem: HTMLElement = null)
    {
        if (!this.frameWindow) {
            this.frameWindow = new ItemFrameWindow(this.app);
            this.frameWindow.show({
                item: this,
                above: aboveElem,
                resizable: as.Bool(this.properties.IframeResizable, true),
                titleText: as.String(this.properties.Label, 'Item'),
                url: as.String(this.properties.IframeUrl, 'https://example.com'),
                width: as.Int(this.properties.IframeWidth, 400),
                height: as.Int(this.properties.IframeHeight, 400),
                onClose: () => { this.frameWindow = null; },
            });
        }
    }
}
