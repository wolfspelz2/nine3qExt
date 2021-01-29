import log = require('loglevel');
import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Panic } from '../lib/Panic';
import { Config } from '../lib/Config';
import { Memory } from '../lib/Memory';
import { AvatarGallery } from '../lib/AvatarGallery';
import { Translator } from '../lib/Translator';
import { Browser } from '../lib/Browser';
import { ItemException } from '../lib/ItemExcption';
import { BackpackShowItemData, BackpackSetItemData, BackpackRemoveItemData, ContentMessage } from '../lib/ContentMessage';
import { HelloWorld } from './HelloWorld';
import { PropertyStorage } from './PropertyStorage';
import { Room } from './Room';
import { VpiResolver } from './VpiResolver';
import { SettingsWindow } from './SettingsWindow';
import { XmppWindow } from './XmppWindow';
import { ChangesWindow } from './ChangesWindow';
import { ItemRepository } from './ItemRepository';
import { TestWindow } from './TestWindow';
import { BackpackWindow } from './BackpackWindow';
import { SimpleErrorToast, SimpleToast } from './Toast';
import { IframeApi } from './IframeApi';
import { Environment } from '../lib/Environment';

interface ILocationMapperResponse
{
    //    sMessage: string;
    sLocationURL: string;
}

export class ContentAppNotification
{
    static type_onTabChangeStay: string = 'onTabChangeStay';
    static type_onTabChangeLeave: string = 'onTabChangeLeave';
    static type_stopped: string = 'stopped';
    static type_restart: string = 'restart';
}

interface ContentAppNotificationCallback { (msg: any): void }
interface StanzaResponseHandler { (stanza: xml): void }

export class ContentApp
{
    private display: HTMLElement;
    private pageUrl: string;
    private presetPageUrl: string;
    private roomJid: string;
    private room: Room;
    private itemRepository: ItemRepository;
    private propertyStorage: PropertyStorage = new PropertyStorage();
    private babelfish: Translator;
    private xmppWindow: XmppWindow;
    private backpackWindow: BackpackWindow;
    private settingsWindow: SettingsWindow;
    private stanzasResponses: { [stanzaId: string]: StanzaResponseHandler } = {};
    private onRuntimeMessageClosure: (message: any, sender: any, sendResponse: any) => any;
    private iframeApi: IframeApi;

    private stayHereIsChecked: boolean = false;
    private backpackIsOpen: boolean = false;
    private inventoryIsOpen: boolean = false;
    private vidconfIsOpen: boolean = false;
    private chatIsOpen: boolean = false;

    // Getter

    getPropertyStorage(): PropertyStorage { return this.propertyStorage; }
    getDisplay(): HTMLElement { return this.display; }
    getItemRepository() { return this.itemRepository; }
    getRoom(): Room { return this.room; }
    getBackpackWindow(): BackpackWindow { return this.backpackWindow; }

    constructor(protected appendToMe: HTMLElement, private messageHandler: ContentAppNotificationCallback)
    {
        this.itemRepository = new ItemRepository(this);
    }

    async start(params: any)
    {
        if (params && params.nickname) { await Memory.setSync(Utils.syncStorageKey_Nickname(), params.nickname); }
        if (params && params.avatar) { await Memory.setSync(Utils.syncStorageKey_Avatar(), params.avatar); }
        if (params && params.pageUrl) { this.presetPageUrl = params.pageUrl; }
        if (params && params.x) { await Memory.setLocal(Utils.localStorageKey_X(), params.x); }

        try {
            await BackgroundMessage.waitReady();
        } catch (error) {
            log.debug(error.message);
            Panic.now();
        }
        if (Panic.isOn) { return; }

        if (!await this.getActive()) {
            log.info('Avatar disabled');
            this.messageHandler({ 'type': ContentAppNotification.type_stopped });
            return;
        }

        try {
            let config = await BackgroundMessage.getConfigTree(Config.onlineConfigName);
            Config.setOnlineTree(config);
        } catch (error) {
            log.debug(error.message);
            Panic.now();
        }
        if (Panic.isOn) { return; }

        try {
            let config = await BackgroundMessage.getConfigTree(Config.devConfigName);
            Config.setDevTree(config);
        } catch (error) {
            log.debug(error.message);
        }

        {
            let pageUrl = Browser.getCurrentPageUrl();
            let parsedUrl = new URL(pageUrl);
            if (parsedUrl.hash.search('#n3qdisable') >= 0) {
                return;
            }
            let ignoredDomains: Array<string> = Config.get('vp.ignoredDomainSuffixes', []);
            for (let i = 0; i < ignoredDomains.length; i++) {
                if (parsedUrl.host.endsWith(ignoredDomains[i])) {
                    return;
                }
            }
        }

        await Utils.sleep(as.Float(Config.get('vp.deferPageEnterSec', 1)) * 1000);

        let language: string = Translator.mapLanguage(navigator.language, lang => { return Config.get('i18n.languageMapping', {})[lang]; }, Config.get('i18n.defaultLanguage', 'en-US'));
        this.babelfish = new Translator(Config.get('i18n.translations', {})[language], language, Config.get('i18n.serviceUrl', ''));

        await this.assertActive();
        if (Panic.isOn) { return; }
        await this.assertUserNickname();
        if (Panic.isOn) { return; }
        await this.assertUserAvatar();
        if (Panic.isOn) { return; }
        await this.assertSavedPosition();
        if (Panic.isOn) { return; }

        let page = $('<div id="n3q" class="n3q-base n3q-hidden-print" />').get(0);
        this.display = $('<div class="n3q-base n3q-display" />').get(0);
        $(page).append(this.display);
        this.appendToMe.append(page);

        if (Environment.isExtension()) {
            this.onRuntimeMessageClosure = (message: any, sender: chrome.runtime.MessageSender, sendResponse: (response?: any) => void) => this.onRuntimeMessage(message, sender, sendResponse);
            chrome.runtime.onMessage.addListener(this.onRuntimeMessageClosure);
        }

        // this.enterPage();
        await this.checkPageUrlChanged();

        this.stayHereIsChecked = await Memory.getLocal(Utils.localStorageKey_StayOnTabChange(this.roomJid), false);

        this.backpackIsOpen = await Memory.getLocal(Utils.localStorageKey_BackpackIsOpen(this.roomJid), false);
        if (this.backpackIsOpen && this.roomJid != '') {
            this.showBackpackWindow(null);
        }

        this.startCheckPageUrl();
        this.pingBackgroundToKeepConnectionAlive();
        this.iframeApi = new IframeApi(this).start();
    }

    stop()
    {
        this.iframeApi?.stop();
        this.stop_pingBackgroundToKeepConnectionAlive();
        this.stopCheckPageUrl();
        this.leavePage();
        this.onUnload();
    }

    onUnload()
    {
        if (this.room) {
            this.room.onUnload();
            this.room = null;
        }

        try {
            chrome.runtime?.onMessage.removeListener(this.onRuntimeMessageClosure);
        } catch (error) {
            //            
        }

        // Remove our own top element
        $('#n3q').remove();

        this.display = null;
    }

    test(): void
    {
        // new SimpleToast(this, 'test', 4, 'warning', 'Heiner (dev)', 'greets').show();

        // this.showBackpackWindow(null);

        //new TestWindow(this).show({});
    }

    showBackpackWindow(aboveElem: HTMLElement): void
    {
        if (this.backpackWindow == null) {
            this.setBackpackIsOpen(true);
            this.backpackWindow = new BackpackWindow(this);
            this.backpackWindow.show({ 'above': aboveElem, onClose: () => { this.backpackWindow = null; this.setBackpackIsOpen(false); } });
        }
    }

    showXmppWindow()
    {
        this.xmppWindow = new XmppWindow(this);
        this.xmppWindow.show({ onClose: () => { this.xmppWindow = null; } });
    }

    showChangesWindow()
    {
        new ChangesWindow(this).show({});
    }

    showSettings(aboveElem: HTMLElement)
    {
        if (!this.settingsWindow) {
            this.settingsWindow = new SettingsWindow(this);
            /* await */ this.settingsWindow.show({ 'above': aboveElem, onClose: () => { this.settingsWindow = null; } });
        }
    }

    // Stay on tab change

    setBackpackIsOpen(value: boolean): void
    {
        this.backpackIsOpen = value; this.evaluateStayOnTabChange();
        if (value) {
            /* await */ Memory.setLocal(Utils.localStorageKey_BackpackIsOpen(this.roomJid), value);
        } else {
            /* await */ Memory.deleteLocal(Utils.localStorageKey_BackpackIsOpen(this.roomJid));
        }
    }

    setInventoryIsOpen(value: boolean): void
    {
        this.inventoryIsOpen = value; this.evaluateStayOnTabChange();
    }

    setVidconfIsOpen(value: boolean): void
    {
        this.vidconfIsOpen = value; this.evaluateStayOnTabChange();
    }

    setChatIsOpen(value: boolean): void
    {
        this.chatIsOpen = value; this.evaluateStayOnTabChange();
    }

    getStayHereIsChecked(): boolean
    {
        return this.stayHereIsChecked;
    }

    toggleStayHereIsChecked(): void
    {
        this.stayHereIsChecked = !this.stayHereIsChecked;

        if (this.stayHereIsChecked) {
            /* await */ Memory.setLocal(Utils.localStorageKey_StayOnTabChange(this.roomJid), this.stayHereIsChecked);
        } else {
            /* await */ Memory.deleteLocal(Utils.localStorageKey_StayOnTabChange(this.roomJid));
        }

        this.evaluateStayOnTabChange();
    }

    evaluateStayOnTabChange(): void
    {
        let stay = this.backpackIsOpen || this.inventoryIsOpen || this.vidconfIsOpen || this.chatIsOpen || this.stayHereIsChecked;
        if (stay) {
            this.messageHandler({ 'type': ContentAppNotification.type_onTabChangeStay });
        } else {
            this.messageHandler({ 'type': ContentAppNotification.type_onTabChangeLeave });
        }
    }

    // Backgound pages dont allow timers 
    // and alerts were unreliable on first test.
    // So, let the content script call the background
    private pingBackgroundToKeepConnectionAliveSec: number = Config.get('xmpp.pingBackgroundToKeepConnectionAliveSec', 180);
    private pingBackgroundToKeepConnectionAliveTimer: number = undefined;
    private pingBackgroundToKeepConnectionAlive()
    {
        if (this.pingBackgroundToKeepConnectionAliveTimer == undefined) {
            this.pingBackgroundToKeepConnectionAliveTimer = <number><unknown>setTimeout(async () =>
            {
                try {
                    await BackgroundMessage.pingBackground();
                } catch (error) {
                    //
                }

                this.pingBackgroundToKeepConnectionAliveTimer = undefined;
                this.pingBackgroundToKeepConnectionAlive();
            }, this.pingBackgroundToKeepConnectionAliveSec * 1000);
        }
    }

    private stop_pingBackgroundToKeepConnectionAlive()
    {
        if (this.pingBackgroundToKeepConnectionAliveTimer != undefined) {
            clearTimeout(this.pingBackgroundToKeepConnectionAliveTimer);
            this.pingBackgroundToKeepConnectionAliveTimer = undefined;
        }
    }

    // IPC

    onDirectRuntimeMessage(message: any)
    {
        this.onSimpleRuntimeMessage(message);
    }

    private onRuntimeMessage(message, sender: chrome.runtime.MessageSender, sendResponse: (response?: any) => void): any
    {
        this.onSimpleRuntimeMessage(message);
    }

    private onSimpleRuntimeMessage(message): any
    {
        switch (message.type) {
            case ContentMessage.type_recvStanza: {
                this.handle_recvStanza(message.stanza);
            } break;

            case ContentMessage.type_userSettingsChanged: {
                this.handle_userSettingsChanged();
            } break;

            case ContentMessage.type_extensionActiveChanged: {
                this.handle_extensionActiveChanged(message.data.state);
            } break;

            case ContentMessage.type_sendPresence: {
                this.handle_sendPresence();
                return false;
            } break;

            case ContentMessage.type_onBackpackShowItem: {
                this.backpackWindow?.onShowItem(message.data.id, message.data.properties);
                return false;
            } break;
            case ContentMessage.type_onBackpackSetItem: {
                this.backpackWindow?.onSetItem(message.data.id, message.data.properties);
                return false;
            } break;
            case ContentMessage.type_onBackpackHideItem: {
                this.backpackWindow?.onHideItem(message.data.id);
                return false;
            } break;
        }
        return true;
    }

    handle_recvStanza(jsStanza: any): any
    {
        let stanza: xml = Utils.jsObject2xmlObject(jsStanza);
        log.debug('ContentApp.recvStanza', stanza, as.String(stanza.attrs.type, stanza.name == 'presence' ? 'available' : 'normal'), 'to=', stanza.attrs.to, 'from=', stanza.attrs.from);

        if (this.xmppWindow) {
            let stanzaText = stanza.toString();
            this.xmppWindow.showLine('_IN_', stanzaText);
        }

        switch (stanza.name) {
            case 'presence': this.onPresence(stanza); break;
            case 'message': this.onMessage(stanza); break;
            case 'iq': this.onIq(stanza); break;
        }

        // return true;
    }

    handle_userSettingsChanged(): any
    {
        this.messageHandler({ 'type': ContentAppNotification.type_restart });
    }

    handle_extensionActiveChanged(state: boolean): any
    {
        if (state) {
            // should not happen
        } else {
            this.messageHandler({ 'type': ContentAppNotification.type_stopped });
        }
    }

    handle_sendPresence(): void
    {
        if (this.room) {
            this.room.sendPresence();
        }
    }

    // handle_ItemException(ex: ItemException)
    // {
    //     new SimpleErrorToast(this,
    //         'Warning-' + ex.fact.toString() + '-' + ex.reason.toString(),
    //         Config.get('room.errorToastDurationSec', 10),
    //         'warning',
    //         ex.fact.toString(),
    //         ex.reason.toString(),
    //         ex.detail ?? ''
    //     )
    //         .show();
    // }

    // enterPage()
    // {
    //     this.pageUrl = Browser.getCurrentPageUrl();
    //     this.enterRoomByPageUrl(this.pageUrl);
    // }

    leavePage()
    {
        this.leaveRoom();
    }

    async checkPageUrlChanged()
    {
        try {
            let pageUrl = this.presetPageUrl ?? Browser.getCurrentPageUrl();

            let strippedUrlPrefixes = Config.get('vp.strippedUrlPrefixes', []);
            for (let i in strippedUrlPrefixes) {
                if (pageUrl.startsWith(strippedUrlPrefixes[i])) {
                    pageUrl = pageUrl.substring(strippedUrlPrefixes[i].length);
                }
            }
            
            let newSignificatParts = pageUrl ? this.getSignificantUrlParts(pageUrl) : '';
            let oldSignificatParts = this.pageUrl ? this.getSignificantUrlParts(this.pageUrl) : '';
            if (newSignificatParts == oldSignificatParts) { return }

            log.debug('Page changed', this.pageUrl, ' => ', pageUrl);
            this.pageUrl = pageUrl;

            let vpi = new VpiResolver(BackgroundMessage, Config);
            vpi.language = Translator.getShortLanguageCode(this.babelfish.getLanguage());
            let newLocation = await vpi.map(pageUrl);
            let newRoomJid = ContentApp.getRoomJidFromLocationUrl(newLocation);

            if (newRoomJid == this.roomJid) {
                log.debug('Same room', pageUrl, ' => ', this.roomJid);
                return;
            }

            this.leavePage();

            this.roomJid = newRoomJid;
            log.debug('Mapped', pageUrl, ' => ', this.roomJid);

            if (this.roomJid != '') {
                this.enterRoom(this.roomJid, pageUrl);
            }

        } catch (error) {
            log.info(error);
        }
    }

    getSignificantUrlParts(url: string)
    {
        let parsedUrl = new URL(url)
        return parsedUrl.host + parsedUrl.pathname + parsedUrl.search;
    }

    private checkPageUrlSec: number = Config.get('room.checkPageUrlSec', 5);
    private checkPageUrlTimer: number;
    private startCheckPageUrl()
    {
        this.stopCheckPageUrl();
        this.checkPageUrlTimer = <number><unknown>setTimeout(async () =>
        {
            await this.checkPageUrlChanged();
            this.checkPageUrlTimer = undefined;
            this.startCheckPageUrl();
        }, this.checkPageUrlSec * 1000);
    }

    private stopCheckPageUrl()
    {
        if (this.checkPageUrlTimer) {
            clearTimeout(this.checkPageUrlTimer);
            this.checkPageUrlTimer = undefined;
        }
    }

    static getRoomJidFromLocationUrl(locationUrl: string): string
    {
        let jid = '';
        let url = new URL(locationUrl);
        return url.pathname;
    }

    // async enterRoomByPageUrl(pageUrl: string): Promise<void>
    // {
    //     try {
    //         let vpi = new VpiResolver(BackgroundMessage, Config);
    //         vpi.language = Translator.getShortLanguageCode(this.babelfish.getLanguage());

    //         this.locationUrl = await vpi.map(pageUrl);
    //         log.debug('Mapped', pageUrl, ' => ', this.locationUrl);

    //         let roomJid = ContentApp.getRoomJidFromLocationUrl(this.locationUrl);
    //         this.enterRoom(roomJid, pageUrl);

    //     } catch (error) {
    //         log.info(error);
    //     }
    // }

    async enterRoom(roomJid: string, roomDestination: string): Promise<void>
    {
        this.leaveRoom();

        this.room = new Room(this, roomJid, roomDestination, await this.getSavedPosition());
        log.debug('ContentApp.enterRoom', roomJid);
        this.room.enter();
    }

    leaveRoom(): void
    {
        if (this.room) {
            log.debug('ContentApp.leaveRoom', this.room.getJid());
            this.room.leave();
            this.room = null;
        }
    }

    onPresence(stanza: xml): void
    {
        let isHandled = false;

        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (!isHandled) {
            if (this.room) {
                if (roomOrUser == this.room.getJid()) {
                    this.room.onPresence(stanza);
                    isHandled = true;
                }
            }
        }
    }

    onMessage(stanza: xml): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (this.room) {
            if (roomOrUser == this.room.getJid()) {
                this.room.onMessage(stanza);
            }
        }
    }

    onIq(stanza: xml): void
    {
        if (stanza.attrs) {
            let id = stanza.attrs.id;
            if (id) {
                if (this.stanzasResponses[id]) {
                    this.stanzasResponses[id](stanza);
                    delete this.stanzasResponses[id];
                }
            }
        }
    }

    async sendStanza(stanza: xml, stanzaId: string = null, responseHandler: StanzaResponseHandler = null): Promise<void>
    {
        log.debug('ContentApp.sendStanza', stanza, as.String(stanza.attrs.type, stanza.name == 'presence' ? 'available' : 'normal'), 'to=', stanza.attrs.to);
        try {
            if (this.xmppWindow) {
                let stanzaText = stanza.toString();
                this.xmppWindow.showLine('OUT', stanzaText);
            }

            if (stanzaId && responseHandler) {
                this.stanzasResponses[stanzaId] = responseHandler;
            }

            await BackgroundMessage.sendStanza(stanza);
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    // Window management

    public static DisplayLayer_Default = 0;
    public static DisplayLayer_Popup = 1;

    private toFrontCurrentIndex: number = 1;
    toFront(elem: HTMLElement, layer = ContentApp.DisplayLayer_Default)
    {
        this.toFrontCurrentIndex++;
        elem.style.zIndex = '' + (this.toFrontCurrentIndex + layer * 1000000000);
    }

    enableScreen(on: boolean): void
    {
        // if (on) {
        //     this.originalScreenHeight = this.screenElem.style.height;
        //     this.screenElem.style.height = '100%';
        // } else {
        //     this.screenElem.style.height = this.originalScreenHeight;
        // }
    }

    private dropzoneELem: HTMLElement = null;
    showDropzone()
    {
        this.hideDropzone();

        this.dropzoneELem = <HTMLElement>$('<div class="n3q-base n3q-dropzone" />').get(0);
        $(this.display).append(this.dropzoneELem);
        this.toFront(this.dropzoneELem);
    }

    hideDropzone()
    {
        if (this.dropzoneELem) {
            $(this.dropzoneELem).remove();
            this.dropzoneELem = null;
        }
    }

    hiliteDropzone(state: boolean)
    {
        if (this.dropzoneELem) {
            if (state) {
                $(this.dropzoneELem).addClass('n3q-dropzone-hilite');
            } else {
                $(this.dropzoneELem).removeClass('n3q-dropzone-hilite');
            }
        }
    }

    // i18n

    translateText(key: string, defaultText: string = null): string
    {
        return this.babelfish.translateText(key, defaultText);
    }

    translateElem(elem: HTMLElement): void
    {
        this.babelfish.translateElem(elem);
    }

    // Dont show this message again management

    localStorage_DontShowNotice_KeyPrefix: string = 'DontShowNotice';

    async isDontShowNoticeType(type: string): Promise<boolean>
    {
        return await Memory.getLocal(this.localStorage_DontShowNotice_KeyPrefix + type, false);
    }

    async setDontShowNoticeType(type: string, value: boolean): Promise<void>
    {
        await Memory.setLocal(this.localStorage_DontShowNotice_KeyPrefix + type, value);
    }

    // my active

    async assertActive()
    {
        try {
            let active = await Memory.getLocal(Utils.localStorageKey_Active(), '');
            if (active == '') {
                await Memory.setLocal(Utils.localStorageKey_Active(), 'true');
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getActive(): Promise<boolean>
    {
        try {
            let active = await Memory.getLocal(Utils.localStorageKey_Active(), 'true');
            return as.Bool(active, false);
        } catch (error) {
            log.info(error);
            return false;
        }
    }

    // my nickname

    async assertUserNickname()
    {
        try {
            let nickname = await Memory.getSync(Utils.syncStorageKey_Nickname(), '');
            if (nickname == '') {
                await Memory.setSync(Utils.syncStorageKey_Nickname(), 'Your name');
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getUserNickname(): Promise<string>
    {
        try {
            return await Memory.getSync(Utils.syncStorageKey_Nickname(), 'no name');
        } catch (error) {
            log.info(error);
            return 'no name';
        }
    }

    // my avatar

    async assertUserAvatar()
    {
        try {
            let avatar = await Memory.getSync(Utils.syncStorageKey_Avatar(), '');
            if (avatar == '') {
                avatar = AvatarGallery.getRandomAvatar();
                await Memory.setSync(Utils.syncStorageKey_Avatar(), avatar);
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getUserAvatar(): Promise<string>
    {
        try {
            return await Memory.getSync(Utils.syncStorageKey_Avatar(), '004/pinguin');
        } catch (error) {
            log.info(error);
            return '004/pinguin';
        }
    }

    // my x

    async assertSavedPosition()
    {
        try {
            let x = as.Int(await Memory.getLocal(Utils.localStorageKey_X(), -1), -1);
            if (x < 0) {
                x = Utils.randomInt(as.Int(Config.get('room.randomEnterPosXMin', 400)), as.Int(Config.get('room.randomEnterPosXMax', 700)))
                await this.savePosition(x);
            }
        } catch (error) {
            log.info(error);
        }
    }

    async savePosition(x: number): Promise<void>
    {
        try {
            await Memory.setLocal(Utils.localStorageKey_X(), x);
        } catch (error) {
            log.info(error);
        }
    }

    async getSavedPosition(): Promise<number>
    {
        let x = 0;

        try {
            x = as.Int(await Memory.getLocal(Utils.localStorageKey_X(), -1), -1);
        } catch (error) {
            log.info(error);
        }

        if (x <= 0) {
            x = this.getDefaultPosition(await this.getUserNickname());
        }

        return x;
    }

    getDefaultPosition(key: string = null): number
    {
        let pos: number = 300;
        let width = this.display.offsetWidth;
        if (!width) { width = 500; }
        if (key) {
            pos = Utils.pseudoRandomInt(250, width - 80, key, '', 7237);
        } else {
            pos = Utils.randomInt(250, width - 80);
        }
        return pos;
    }
}
