import imgDefaultItem from '../assets/DefaultItem.png';

import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D, Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { Payload } from '../lib/Payload';
import { ContentApp } from './ContentApp';
import { ItemFrameWindow } from './ItemFrameWindow';
import { ItemFramePopup } from './ItemFramePopup';
import { Pid } from '../lib/ItemProperties';
import { Memory } from '../lib/Memory';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { BackgroundApp } from '../background/BackgroundApp';

export class RepositoryItem
{
    private providerId: string;
    private properties: { [pid: string]: string } = {};
    private frameWindow: ItemFrameWindow;
    private framePopup: ItemFramePopup;

    constructor(protected app: ContentApp, private id: string)
    {
    }

    getId() { return this.id; }

    setProviderId(providerId: string) { this.providerId = providerId; }
    getProviderId(): string { return this.providerId; }

    setProperties(properties: { [pid: string]: string; }) { this.properties = properties; }
    getProperties(): any { return this.properties; }

    onClick(clickedElem: HTMLElement, clickPoint: Point2D)
    {
        if (as.Bool(this.properties[Pid.IframeAspect], false)) {
            let frame = as.String(this.properties[Pid.IframeFrame], 'Window');
            if (frame == 'Popup') {
                if (this.framePopup) {
                    this.framePopup.close();
                } else {
                    this.openIframe(clickedElem, clickPoint);
                }
            } else {
                if (!this.frameWindow) {
                    this.openIframe(clickedElem, clickPoint);
                }
            }
        }
    }

    onDrag(clickedElem: HTMLElement, clickPoint: Point2D)
    {
        if (as.Bool(this.properties[Pid.IframeAspect], false)) {
            let frame = as.String(this.properties[Pid.IframeFrame], 'Window');
            if (frame == 'Popup') {
                if (this.framePopup) {
                    this.framePopup.close();
                }
            }
        }
    }

    async openIframe(clickedElem: HTMLElement, clickPoint: Point2D)
    {
        let iframeUrl = as.String(this.properties[Pid.IframeUrl], null);
        let room = this.app.getRoom();
        let apiUrl = Config.get('itemProviders.' + this.providerId + '.config.' + 'apiUrl', '');
        let userId = await Memory.getSync(Utils.syncStorageKey_Id(), '');

        if (iframeUrl != '' && room && apiUrl != '' && userId != '') {
            // iframeUrl = 'https://jitsi.vulcan.weblin.com/{room}#userInfo.displayName="{name}"';
            let roomJid = room.getJid();
            let roomNick = room.getMyNick();
            let tokenOptions = {};
            if (await BackgroundMessage.isBackpackItem(this.id)) {
                tokenOptions['properties'] = await BackgroundMessage.getBackpackItemProperties(this.id);
            }
            let contextToken = await Payload.getContextToken(apiUrl, userId, this.id, 600, { 'room': roomJid }, tokenOptions);
            iframeUrl = iframeUrl
                .replace('{context}', encodeURIComponent(contextToken))
                .replace('{room}', encodeURIComponent(roomJid))
                .replace('{name}', encodeURIComponent(roomNick))
                ;

            let frame = as.String(this.properties[Pid.IframeFrame], 'Window');
            if (frame == 'Popup') {
                this.openIframePopup(iframeUrl, clickPoint);
            } else {
                this.openIframeWindow(iframeUrl, clickPoint);
            }
        }
    }

    openIframePopup(iframeUrl: string, clickPoint: Point2D)
    {
        if (!this.framePopup) {
            this.framePopup = new ItemFramePopup(this.app);
            this.framePopup.show({
                item: this,
                clickPos: clickPoint,
                url: iframeUrl,
                onClose: () => { this.framePopup = null; },
            });
        }
    }

    openIframeWindow(iframeUrl: string, clickPoint: Point2D)
    {
        if (!this.frameWindow) {
            this.frameWindow = new ItemFrameWindow(this.app);
            this.frameWindow.show({
                item: this,
                clickPos: clickPoint,
                url: iframeUrl,
                onClose: () => { this.frameWindow = null; },
            });
        }
    }
}
