import log = require('loglevel');
import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Panic } from '../lib/Panic';
import { Config } from '../lib/Config';
import { AvatarGallery } from '../lib/AvatarGallery';
import { Translator } from '../lib/Translator';
import { Browser } from '../lib/Browser';
import { HelloWorld } from './HelloWorld';
import { PropertyStorage } from './PropertyStorage';
import { Room } from './Room';
import { VpiResolver } from './VpiResolver';
import { SettingsWindow } from './SettingsWindow';
import { XmppWindow } from './XmppWindow';
import { ChangesWindow } from './ChangesWindow';
import { Inventory } from './Inventory';
import { ItemProvider } from './ItemProvider';
import { ItemRepository } from './ItemRepository';
import { TestWindow } from './TestWindow';

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
    private locationUrl: string;
    private room: Room;
    private inventories: { [invJid: string]: Inventory; } = {};
    private itemProviders: { [providerId: string]: ItemProvider; } = {};
    private itemRepository: ItemRepository;
    private propertyStorage: PropertyStorage = new PropertyStorage();
    private babelfish: Translator;
    private xmppWindow: XmppWindow;
    private settingsWindow: SettingsWindow;
    private stanzasResponses: { [stanzaId: string]: StanzaResponseHandler } = {}
    private runtimeOnMessageClosure: (message: any, sender: any, sendResponse: any) => any;

    private stayHereIsChecked: boolean = false;
    private inventoryIsOpen: boolean = false;
    private vidconfIsOpen: boolean = false;
    private chatIsOpen: boolean = false;

    // Getter

    getPropertyStorage(): PropertyStorage { return this.propertyStorage; }
    getDisplay(): HTMLElement { return this.display; }
    getItemRepository() { return this.itemRepository; }
    getRoom(): Room { return this.room; }
    getInventoryByProviderId(providerId: string): Inventory
    {
        for (let invJid in this.inventories) {
            let inv = this.inventories[invJid];
            if (inv.getProviderId() == providerId) {
                return inv;
            }
        }
    }

    constructor(protected appendToMe: HTMLElement, private messageHandler: ContentAppNotificationCallback)
    {
        this.itemRepository = new ItemRepository(this);
    }

    async start()
    {
        try {
            await BackgroundMessage.waitReady();
        } catch (error) {
            log.debug(error.message);
            Panic.now();
        }
        if (Panic.isOn) { return; }

        if (!await this.getActive()) {
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
            let config = await BackgroundMessage.getConfigTree(Config.sessionConfigName);
            Config.setSessionTree(config);
        } catch (error) {
            log.debug(error.message);
        }

        try {
            let config = await BackgroundMessage.getConfigTree(Config.devConfigName);
            Config.setDevTree(config);
        } catch (error) {
            log.debug(error.message);
        }

        {
            let pageUrl = Browser.getCurrentPageUrl();
            let parsedUrl = new URL(pageUrl);
            let ignoredDomains: Array<string> = Config.get('vp.ignoredDomainSuffixes', []);
            for (let i = 0; i < ignoredDomains.length; i++) {
                if (parsedUrl.host.endsWith(ignoredDomains[i])) {
                    return;
                }
            }
        }

        await this.initItemProviders();

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

        // chrome.runtime?.onMessage.addListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });
        this.runtimeOnMessageClosure = this.getRuntimeOnMessageClosure();
        chrome.runtime?.onMessage.addListener(this.runtimeOnMessageClosure);

        // this.enterPage();
        this.checkPageUrlChanged();

        this.startCheckPageUrl();
        this.pingBackgroundToKeepConnectionAlive();
    }

    getRuntimeOnMessageClosure()
    {
        var self = this;
        function runtimeOnMessageClosure(message, sender, sendResponse)
        {
            return self.runtimeOnMessage(message, sender, sendResponse);
        }
        return runtimeOnMessageClosure;
    }

    //   var myFunc = makeFunc();
    //   myFunc();

    stop()
    {
        this.stop_pingBackgroundToKeepConnectionAlive();
        this.stopCheckPageUrl();
        this.leavePage();
        this.onUnload();
    }

    onUnload()
    {
        for (let inventoryJid in this.inventories) {
            if (this.inventories[inventoryJid]) {
                this.inventories[inventoryJid].onUnload();
                delete this.inventories[inventoryJid];
            }
        }

        if (this.room) {
            this.room.onUnload();
            this.room = null;
        }

        try {
            chrome.runtime?.onMessage.removeListener(this.runtimeOnMessageClosure);
        } catch (error) {
            //            
        }

        // Remove all jquery dialogs (they are appended to <body> and appendTo:#n3q wont work)
        $('.n3q-ui-dialog').remove();

        // Remove our own top element
        $('#n3q').remove();

        this.display = null;
    }

    test(): void
    {
        // BackgroundMessage.test();
        new TestWindow(this).show({});
    }

    async showInventoryWindow(aboveElem: HTMLElement, providerId: string): Promise<void>
    {
        let inv = new Inventory(this, providerId);
        let jid = inv.getJid();

        if (!this.inventories[jid]) {
            this.inventories[jid] = inv;

            this.setInventoryIsOpen(true);

            await inv.open({
                'above': aboveElem,
                onClose: () =>
                {
                    if (this.inventories[jid]) {
                        this.inventories[jid].close();
                        delete this.inventories[jid];
                    }
                    this.setInventoryIsOpen(false);
                },
            });
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
            this.settingsWindow.show({ 'above': aboveElem });
        }
    }

    // Stay on tab change

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

    getStayHereIsChecked(): boolean { return this.stayHereIsChecked; }
    toggleStayHereIsChecked(): void
    {
        this.stayHereIsChecked = !this.stayHereIsChecked;
        this.evaluateStayOnTabChange();
    }

    evaluateStayOnTabChange(): void
    {
        let stay = this.inventoryIsOpen || this.vidconfIsOpen || this.chatIsOpen || this.stayHereIsChecked;
        if (stay) {
            this.messageHandler({ 'type': ContentAppNotification.type_onTabChangeStay });
        } else {
            this.messageHandler({ 'type': ContentAppNotification.type_onTabChangeLeave });
        }
    }

    // Backgound pages dont allow timers 
    // and alerts were unreliable on first test.
    // So ,let the content script call the background
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

    runtimeOnMessage(message, sender: chrome.runtime.MessageSender, sendResponse): any
    {
        switch (message.type) {
            case 'recvStanza': {
                this.handle_recvStanza(message.stanza);
                sendResponse();
            } break;

            case 'userSettingsChanged': {
                this.handle_userSettingsChanged();
                sendResponse();
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

    // enterPage()
    // {
    //     this.pageUrl = Browser.getCurrentPageUrl();
    //     this.enterRoomByPageUrl(this.pageUrl);
    // }

    leavePage()
    {
        this.leaveRoom();

        for (let inventoryJid in this.inventories) {
            var inv = this.inventories[inventoryJid];
            inv.close();
        }
    }

    async checkPageUrlChanged()
    {
        try {
            let pageUrl = Browser.getCurrentPageUrl();

            let newSignificatParts = pageUrl ? this.getSignificantUrlParts(pageUrl) : '';
            let oldSignificatParts = this.pageUrl ? this.getSignificantUrlParts(this.pageUrl) : '';
            if (newSignificatParts == oldSignificatParts) { return }

            log.debug('Page changed', this.pageUrl, ' => ', pageUrl);
            this.pageUrl = pageUrl;

            let vpi = new VpiResolver(BackgroundMessage, Config);
            vpi.language = Translator.getShortLanguageCode(this.babelfish.getLanguage());
            let newLocation = await vpi.map(pageUrl);
            if (newLocation == this.locationUrl) {
                log.debug('Same room', pageUrl, ' => ', this.locationUrl);
                return;
            }

            this.leavePage();

            this.locationUrl = newLocation;
            log.debug('Mapped', pageUrl, ' => ', this.locationUrl);

            if (this.locationUrl != '') {
                let roomJid = ContentApp.getRoomJidFromLocationUrl(this.locationUrl);
                this.enterRoom(roomJid, pageUrl);
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

        if (!isHandled) {
            if (this.inventories[roomOrUser]) {
                this.inventories[roomOrUser].onPresence(stanza);
                isHandled = true;
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

    translateText(key: string, text: string): string
    {
        return this.babelfish.translateText(key, text);
    }

    translateElem(elem: HTMLElement): void
    {
        this.babelfish.translateElem(elem);
    }

    // Item provider

    async initItemProviders(): Promise<void>
    {
        let itemProviders = Config.get('itemProviders', {});
        let gotAnyProvider = false;

        for (let providerId in itemProviders) {
            let providerConfig = await Config.getSync(Utils.syncStorageKey_ItemProviderConfig(providerId), null);
            if (providerConfig) {
                this.itemProviders[providerId] = new ItemProvider(providerConfig);
                gotAnyProvider = gotAnyProvider || as.Bool(providerConfig['inventoryActive'], false);
            }
        }

        Config.set('inventory.enabled', gotAnyProvider)
    }

    getItemProviderConfigValue(providerId: string, configKey: string, defaultValue: any): any
    {
        if (providerId) {
            var itemProvider = this.itemProviders[providerId];
            if (itemProvider) {
                return itemProvider.getConfig(configKey, defaultValue);
            }
        }
        return defaultValue;
    }

    itemProviderUrlFilter(providerId: string, propName: string, propValue: string): string
    {
        if (providerId) {
            if (this.itemProviders[providerId]) {
                return this.itemProviders[providerId].propertyUrlFilter(propValue);
            }
        }
        return propValue;
    }

    // my active

    async assertActive()
    {
        try {
            let active = await Config.getSync(Utils.syncStorageKey_Active(), '');
            if (active == '') {
                await Config.setSync(Utils.syncStorageKey_Active(), 'true');
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getActive(): Promise<boolean>
    {
        try {
            let active = await Config.getSync(Utils.syncStorageKey_Active(), 'true');
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
            let nickname = await Config.getSync(Utils.syncStorageKey_Nickname(), '');
            if (nickname == '') {
                await Config.setSync(Utils.syncStorageKey_Nickname(), 'Your name');
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getUserNickname(): Promise<string>
    {
        try {
            return await Config.getSync(Utils.syncStorageKey_Nickname(), 'no name');
        } catch (error) {
            log.info(error);
            return 'no name';
        }
    }

    // my avatar

    async assertUserAvatar()
    {
        try {
            let avatar = await Config.getSync(Utils.syncStorageKey_Avatar(), '');
            if (avatar == '') {
                avatar = AvatarGallery.getRandomAvatar();
                await Config.setSync(Utils.syncStorageKey_Avatar(), avatar);
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getUserAvatar(): Promise<string>
    {
        try {
            return await Config.getSync(Utils.syncStorageKey_Avatar(), '004/pinguin');
        } catch (error) {
            log.info(error);
            return '004/pinguin';
        }
    }

    // my x

    async assertSavedPosition()
    {
        try {
            let x = as.Int(await BackgroundMessage.getSessionConfig(Utils.syncStorageKey_X(), -1), -1);
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
            await BackgroundMessage.setSessionConfig(Utils.syncStorageKey_X(), x);
        } catch (error) {
            log.info(error);
        }
    }

    async getSavedPosition(): Promise<number>
    {
        let x = 0;

        try {
            x = as.Int(await BackgroundMessage.getSessionConfig(Utils.syncStorageKey_X(), -1), -1);
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
