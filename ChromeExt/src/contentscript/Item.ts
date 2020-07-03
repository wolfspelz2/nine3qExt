import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { IframeWindow } from './IframeWindow';

import imgDefaultItem from '../assets/DefaultItem.png';

export class Item
{
    private providerId: string;
    private properties: { [pid: string]: string } = {};
    private iframeWindow: IframeWindow;

    constructor(private app: ContentApp)
    {
    }

    setProviderId(providerId: string) { this.providerId = providerId; }
    getProviderId(): string { return this.providerId; }

    setProperties(properties: { [pid: string]: string; }) { this.properties = properties; }
    getProperties() { return this.properties; }

    toggleIframe(aboveElem: HTMLElement = null)
    {
        if (this.iframeWindow) {
            this.iframeWindow.close();
        } else {
            this.iframeWindow = new IframeWindow(this.app);
            this.iframeWindow.show({
                above: aboveElem,
                resizable: as.Bool(this.properties.IframeResizable, true),
                titleText: as.String(this.properties.Label, 'Item'),
                url: as.String(this.properties.IframeUrl, 'https://example.com'),
                width: as.Int(this.properties.IframeWidth, 400),
                height: as.Int(this.properties.IframeHeight, 400),
                onClose: () => { this.iframeWindow = null; },
            });
        }
    }
}
