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
    private rooms: { [roomJid: string]: Room; } = {};
    private inventories: { [invJid: string]: Inventory; } = {};
    private propertyStorage: PropertyStorage = new PropertyStorage();
    private babelfish: Translator;
    private stayOnTabChange: boolean = false;
    private xmppWindow: XmppWindow;
    private settingsWindow: SettingsWindow;
    private stanzasResponses: { [stanzaId: string]: StanzaResponseHandler } = {}

    // Getter

    getPropertyStorage(): PropertyStorage { return this.propertyStorage; }
    getDisplay(): HTMLElement { return this.display; }
    getRoom(): Room
    {
        let room = null;
        for (let roomJid in this.rooms) {
            room = this.rooms[roomJid];
            break;
        }
        return room;
    }
    getInventory(): Inventory
    {
        let inv = null;
        for (let invJid in this.inventories) {
            inv = this.inventories[invJid];
            break;
        }
        return inv;
    }

    constructor(private appendToMe: HTMLElement, private messageHandler: ContentAppNotificationCallback)
    {
    }

    async start()
    {
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
            let config = await BackgroundMessage.getConfigTree(Config.devConfigName);
            Config.setDevTree(config);
        } catch (error) {
            log.debug(error.message);
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

        //this.createPageControl();

        chrome.runtime?.onMessage.addListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });

        this.enterPage();
        this.pingBackgroundToKeepConnectionAlive();
    }

    stop()
    {
        this.stop_pingBackgroundToKeepConnectionAlive();
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

        for (let roomJid in this.rooms) {
            if (this.rooms[roomJid]) {
                this.rooms[roomJid].onUnload();
                delete this.rooms[roomJid];
            }
        }

        try {
            chrome.runtime?.onMessage.removeListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });
        } catch (error) {
            //            
        }

        // Remove all jquery dialogs (they are appended to <body> and appendTo:#n3q wont work)
        $('.n3q-ui-dialog').remove();

        // Remove our own top element
        $('#n3q').remove();

        this.display = null;
    }

    toggleStayOnTabChange(): void
    {
        this.stayOnTabChange = !this.stayOnTabChange;
        if (this.stayOnTabChange) {
            this.messageHandler({ 'type': ContentAppNotification.type_onTabChangeStay });
        } else {
            this.messageHandler({ 'type': ContentAppNotification.type_onTabChangeLeave });
        }
    }

    getStayOnTabChange(): boolean { return this.stayOnTabChange; }

    test(): void
    {
        this.showInventoryWindow()
    }

    showInventoryWindow(): void
    {
        let inv = new Inventory(this, 'nine3q');
        let jid = inv.getJid();

        if (!this.inventories[jid]) {
            this.inventories[jid] = inv;

            inv.open({
                'above': this.display,
                onClose: () =>
                {
                    if (this.inventories[jid]) {
                        this.inventories[jid].close();
                        delete this.inventories[jid];
                    }
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

    createPageControl()
    {
        let controlElem: HTMLElement = $('<div class="n3q-base n3q-ctrl" id="n3q-hello"></div>').get(0);
        this.display.append(controlElem);

        $('#n3q-hello').text(HelloWorld.getText());

        let enterButton: HTMLElement = $('<button class="n3q-base">enter</button>').get(0);
        controlElem.append(enterButton);
        $(enterButton).click(() =>
        {
            // this.enterRoomByJid('d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
            // this.enterRoomByPageUrl('https://www.galactic-developments.de/');
            this.enterPage();
        });
    }

    showSettings(aboveElem: HTMLElement)
    {
        if (!this.settingsWindow) {
            this.settingsWindow = new SettingsWindow(this);
            this.settingsWindow.show({ 'above': aboveElem });
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
        return false;
    }

    handle_recvStanza(jsStanza: any): any
    {
        let stanza: xml = Utils.jsObject2xmlObject(jsStanza);
        log.debug('ContentApp.recvStanza', stanza);

        if (this.xmppWindow) {
            let stanzaText = stanza.toString();
            this.xmppWindow.showLine('_IN_', stanzaText);
        }

        switch (stanza.name) {
            case 'presence': this.onPresence(stanza); break;
            case 'message': this.onMessage(stanza); break;
            case 'iq': this.onIq(stanza); break;
        }
    }

    handle_userSettingsChanged(): any
    {
        this.messageHandler({ 'type': ContentAppNotification.type_restart });
    }

    enterPage()
    {
        this.enterRoomByPageUrl(Browser.getCurrentPageUrl());
    }

    leavePage()
    {
        // Leave all, there should be only one
        for (let roomJid in this.rooms) {
            this.leaveRoomByJid(roomJid);
        }

        for (let inventoryJid in this.inventories) {
            var inv = this.inventories[inventoryJid];
            inv.close();
        }
    }

    static getRoomJidFromLocationUrl(locationUrl: string): string
    {
        let jid = '';
        let url = new URL(locationUrl);
        return url.pathname;
    }

    async enterRoomByPageUrl(pageUrl: string): Promise<void>
    {
        if (Config.get('vp.useLocationMappingService', false)) {

            let url = new URL(Config.get('vp.locationMappingServiceUrl', 'https://lms.virtual-presence.org/api/'));
            url.searchParams.set('Method', 'VPI.Info');
            url.searchParams.set('sDocumentURL', pageUrl);
            url.searchParams.set('Format', 'json');

            let response = await BackgroundMessage.fetchUrl(url.toString(), '');
            if (response.ok) {
                try {
                    let mappingResponse: ILocationMapperResponse = JSON.parse(response.data);
                    let locationUrl = mappingResponse.sLocationURL;
                    log.debug('Mapped', pageUrl, ' => ', locationUrl);
                    let roomJid = ContentApp.getRoomJidFromLocationUrl(locationUrl);
                    this.enterRoomByJid(roomJid, pageUrl);
                } catch (error) {
                    log.info(error);
                }
            }

        } else {

            try {
                let vpi = new VpiResolver(BackgroundMessage, Config);
                vpi.language = Translator.getShortLanguageCode(this.babelfish.getLanguage());

                let locationUrl = await vpi.map(pageUrl);
                log.debug('Mapped', pageUrl, ' => ', locationUrl);
                let roomJid = ContentApp.getRoomJidFromLocationUrl(locationUrl);
                this.enterRoomByJid(roomJid, pageUrl);
            } catch (error) {
                log.info(error);
            }

        }
    }

    async enterRoomByJid(roomJid: string, roomDestination: string): Promise<void>
    {
        if (this.rooms[roomJid] === undefined) {
            this.rooms[roomJid] = new Room(this, roomJid, roomDestination, await this.getSavedPosition());
        }
        log.debug('ContentApp.enterRoomByJid', roomJid);
        this.rooms[roomJid].enter();
    }

    leaveRoomByJid(roomJid: string): void
    {
        log.debug('ContentApp.leaveRoomByJid', roomJid);
        if (this.rooms[roomJid] != undefined) {
            this.rooms[roomJid].leave();
            delete this.rooms[roomJid];
        }
    }

    onPresence(stanza: xml): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (this.rooms[roomOrUser]) {
            this.rooms[roomOrUser].onPresence(stanza);
        } else if (this.inventories[roomOrUser]) {
            this.inventories[roomOrUser].onPresence(stanza);
        }
    }

    onMessage(stanza: xml): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (this.rooms[roomOrUser]) {
            this.rooms[roomOrUser].onMessage(stanza);
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
        log.debug('ContentApp.sendStanza', stanza);
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

    static itemProviderUrlFilter(providerId: string, attrName: string, attrValue: string): any
    {
        if (providerId && providerId != '') {
            let propertyUrlFilter = Config.get('itemProviders.' + providerId + '.config.itemPropertyUrlFilter', {});
            if (propertyUrlFilter) {
                for (let key in propertyUrlFilter) {
                    let value = propertyUrlFilter[key];
                    if (key && value) {
                        attrValue = attrValue.replace(key, value);
                    }
                }
            }
        }
        return attrValue;
    }

    // my active

    async assertActive()
    {
        try {
            let active = await Config.getSync('me.active', '');
            if (active == '') {
                await Config.setSync('me.active', 'true');
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getActive(): Promise<boolean>
    {
        try {
            let active = await Config.getSync('me.active', 'true');
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
            let nickname = await Config.getSync('me.nickname', '');
            if (nickname == '') {
                await Config.setSync('me.nickname', 'Your name');
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getUserNickname(): Promise<string>
    {
        try {
            return await Config.getSync('me.nickname', 'no name');
        } catch (error) {
            log.info(error);
            return 'no name';
        }
    }

    // my avatar

    async assertUserAvatar()
    {
        try {
            let avatar = await Config.getSync('me.avatar', '');
            if (avatar == '') {
                avatar = AvatarGallery.getRandomAvatar();
                await Config.setSync('me.avatar', avatar);
            }
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    async getUserAvatar(): Promise<string>
    {
        try {
            return await Config.getSync('me.avatar', '004/pinguin');
        } catch (error) {
            log.info(error);
            return '004/pinguin';
        }
    }

    // my x

    async assertSavedPosition()
    {
        try {
            let x = as.Int(await BackgroundMessage.getSessionConfig('me.x', -1), -1);
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
            await BackgroundMessage.setSessionConfig('me.x', x);
        } catch (error) {
            log.info(error);
        }
    }

    async getSavedPosition(): Promise<number>
    {
        let x = 0;

        try {
            x = as.Int(await BackgroundMessage.getSessionConfig('me.x', -1), -1);
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
