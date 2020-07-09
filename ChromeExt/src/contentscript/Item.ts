import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Payload } from '../lib/Payload';
import { ContentApp } from './ContentApp';
import { ItemFrameWindow } from './ItemFrameWindow';
import { ItemFramePopup } from './ItemFramePopup';

import imgDefaultItem from '../assets/DefaultItem.png';

export class Item
{
    private providerId: string;
    private properties: { [pid: string]: string } = {};
    private frameWindow: ItemFrameWindow;
    private framePopup: ItemFramePopup;

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

    async openIframe(clickedElem: HTMLElement)
    {
        let iframeUrl = as.String(this.properties.IframeUrl, null);
        let room = this.app.getRoom();
        let apiUrl = this.app.getItemProviderConfigValue(this.providerId, 'apiUrl', '');
        let userId = this.app.getItemProviderConfigValue(this.providerId, 'userToken', '');

        if (iframeUrl != '' && room && apiUrl != '' && userId != '') {
            let roomJid = room.getJid();
            let contextToken = await Payload.getContextToken(apiUrl, userId, this.id, 3600, { 'room': roomJid });
            iframeUrl = iframeUrl.replace('{context}', encodeURIComponent(contextToken));

            let frame = as.String(this.properties.IframeFrame, 'Window');
            if (frame == 'Popup') {
                this.openIframePopup(iframeUrl, clickedElem);
            } else {
                this.openIframeWindow(iframeUrl, clickedElem);
            }
        }
    }

    openIframePopup(iframeUrl: string, aboveElem: HTMLElement = null)
    {
        if (!this.framePopup) {
            this.framePopup = new ItemFramePopup(this.app);
            this.framePopup.show({
                item: this,
                above: aboveElem,
                url: iframeUrl,
                onClose: () => { this.framePopup = null; },
            });
        }
    }

    openIframeWindow(iframeUrl: string, aboveElem: HTMLElement = null)
    {
        if (!this.frameWindow) {
            this.frameWindow = new ItemFrameWindow(this.app);
            this.frameWindow.show({
                item: this,
                above: aboveElem,
                url: iframeUrl,
                onClose: () => { this.frameWindow = null; },
            });
        }
    }
}
