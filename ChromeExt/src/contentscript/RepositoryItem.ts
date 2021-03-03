import imgDefaultItem from '../assets/DefaultItem.png';

import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D, Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { Payload } from '../lib/Payload';
import { ContentApp } from './ContentApp';
import { ItemFrameWindow, ItemFrameWindowOptions } from './ItemFrameWindow';
import { ItemFramePopup } from './ItemFramePopup';
import { Pid } from '../lib/ItemProperties';
import { Memory } from '../lib/Memory';
import { BackgroundMessage } from '../lib/BackgroundMessage';

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

    onDragStart(clickedElem: HTMLElement, clickPoint: Point2D)
    {
        if (this.framePopup) {
            this.framePopup.close();
        }
    }

    onClick(clickedElem: HTMLElement, clickPoint: Point2D, wasInFront: boolean)
    {
        if (as.Bool(this.properties[Pid.IframeAspect], false)) {
            let frame = as.String(JSON.parse(as.String(this.properties[Pid.IframeOptions], '{}')).frame, 'Window');
            if (frame == 'Popup') {
                if (this.framePopup) {
                    this.framePopup.close();
                } else {
                    if (wasInFront) {
                        this.openIframe(clickedElem);
                    }
                }
            } else {
                this.openIframe(clickedElem);
            }
        }
    }

    async openDocumentUrl(aboveElem: HTMLElement)
    {
        let url = as.String(this.properties[Pid.DocumentUrl], null);
        let room = this.app.getRoom();
        let apiUrl = Config.get('itemProviders.' + this.providerId + '.config.' + 'apiUrl', '');
        let userId = await Memory.getSync(Utils.syncStorageKey_Id(), '');

        if (url != '' && room && apiUrl != '' && userId != '') {
            let tokenOptions = {};
            if (await BackgroundMessage.isBackpackItem(this.id)) {
                tokenOptions['properties'] = await BackgroundMessage.getBackpackItemProperties(this.id);
            } else {
                tokenOptions['properties'] = this.properties;
            }
            let contextToken = await Payload.getContextToken(apiUrl, userId, this.id, 600, { 'room': room.getJid() }, tokenOptions);
            url = url.replace('{context}', encodeURIComponent(contextToken));

            let documentOptions = JSON.parse(as.String(this.properties[Pid.DocumentOptions], '{}'));
            this.openIframeWindow(aboveElem, url, documentOptions);
        }
    }
    async openIframe(clickedElem: HTMLElement)
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
            } else {
                tokenOptions['properties'] = this.properties;
            }
            let contextToken = await Payload.getContextToken(apiUrl, userId, this.id, 600, { 'room': roomJid }, tokenOptions);
            iframeUrl = iframeUrl
                .replace('{context}', encodeURIComponent(contextToken))
                .replace('{room}', encodeURIComponent(roomJid))
                .replace('{name}', encodeURIComponent(roomNick))
                ;

            let iframeOptions = JSON.parse(as.String(this.properties[Pid.IframeOptions], '{}'));
            if (as.String(iframeOptions.frame, 'Window') == 'Popup') {
                this.openIframePopup(clickedElem, iframeUrl, iframeOptions);
            } else {
                this.openIframeWindow(clickedElem, iframeUrl, iframeOptions);
            }
        }
    }

    openIframePopup(clickedElem: HTMLElement, iframeUrl: string, frameOptions: any)
    {
        if (this.framePopup == null) {
            this.framePopup = new ItemFramePopup(this.app);

            let options: ItemFrameWindowOptions = {
                item: this,
                elem: clickedElem,
                url: iframeUrl,
                onClose: () => { this.framePopup = null; },
                width: as.Int(frameOptions.width, 100),
                height: as.Int(frameOptions.height, 100),
                left: as.Int(frameOptions.left, -frameOptions.width / 2),
                bottom: as.Int(frameOptions.bottom, 50),
                resizable: as.Bool(frameOptions.rezizable, true),
                transparent: as.Bool(frameOptions.transparent, true)
            }

            this.framePopup.show(options);
        }
    }

    openIframeWindow(clickedElem: HTMLElement, iframeUrl: string, windowOptions: any)
    {
        if (this.frameWindow == null) {
            this.frameWindow = new ItemFrameWindow(this.app);

            let options: ItemFrameWindowOptions = {
                item: this,
                elem: clickedElem,
                url: iframeUrl,
                onClose: () => { this.frameWindow = null; },
                width: as.Int(windowOptions.width, 100),
                height: as.Int(windowOptions.height, 100),
                left: as.Int(windowOptions.left, -windowOptions.width / 2),
                bottom: as.Int(windowOptions.bottom, 50),
                resizable: as.Bool(windowOptions.rezizable, true),
                undockable: as.Bool(windowOptions.undockable, true),
                transparent: as.Bool(windowOptions.transparent, true),
                titleText: as.String(this.properties[Pid.Label], 'Item'),
            }

            this.frameWindow.show(options);
        }
    }

    positionFrame(width: number, height: number, left: number, bottom: number)
    {
        this.framePopup?.position(width, height, left, bottom);
    }

    closeFrame()
    {
        if (this.framePopup) {
            this.framePopup.close();
            this.framePopup = null;
        } else if (this.frameWindow) {
            this.frameWindow.close();
            this.frameWindow = null;
        }
    }

}
