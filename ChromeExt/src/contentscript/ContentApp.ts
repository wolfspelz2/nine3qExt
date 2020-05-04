import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Platform } from '../lib/Platform';
import { Panic } from '../lib/Panic';
import { Config } from '../lib/Config';
import { AvatarGallery } from '../lib/AvatarGallery';
import { Translator } from '../lib/Translator';
import { Room } from './Room';
import { PropertyStorage } from './PropertyStorage';
import { HelloWorld } from './HelloWorld';
require('webpack-jquery-ui');
require('webpack-jquery-ui/css')
require('webpack-jquery-ui/dialog');

interface ILocationMapperResponse
{
    //    sMessage: string;
    sLocationURL: string;
}

export class ContentApp
{
    private display: HTMLElement;
    private xmpp: any;
    private rooms: { [roomJid: string]: Room; } = {};
    private storage: PropertyStorage = new PropertyStorage();
    private keepAliveSec: number = 180;
    private babelfish: Translator;

    // Getter

    getStorage(): PropertyStorage { return this.storage; }
    getAssetUrl(filePath: string) { return Platform.getAssetUrl(filePath); }

    constructor(private appendToMe: HTMLElement)
    {
    }

    async start()
    {
        try {
            let config = await Platform.getConfig();
            Config.setAllOnline(config);
        } catch (error) {
            log.info(error);
            Panic.now();
        }

        await Utils.sleep(as.Float(Config.get('vp.deferPageEnterSec', 1)) * 1000);

        let language: string = Translator.mapLanguage(navigator.language, lang => { return Config.get('i18n.languageMapping', {})[lang]; }, Config.get('i18n.defaultLanguage', 'en-US'));
        this.babelfish = new Translator(Config.get('i18n.translations', {})[language], language, Config.get('i18n.serviceUrl', ''));

        await this.assertUserNickname();
        await this.assertUserAvatar();
        await this.assertSavedPosition();

        let page = $('<div id="n3q-id-page" class="n3q-base n3q-hidden-print" />').get(0);
        this.display = $('<div class="n3q-base n3q-display" />').get(0);
        $(page).append(this.display);
        this.appendToMe.append(page);

        //this.createPageControl();

        chrome.runtime?.onMessage.addListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });

        this.enterPage();
    }

    stop()
    {
        this.leavePage();

        chrome.runtime?.onMessage.removeListener((message, sender, sendResponse) => { return this.runtimeOnMessage(message, sender, sendResponse); });

        $(this.display).remove();
        this.display = null;
    }

    test()
    {
        let dialog = $(`
            <div id="dialog" title="Basic dialog">
                <p>This is the default dialog which is useful for displaying information. The dialog window can be moved, resized and closed with the 'x' icon.</p>
            </div>
              `).get(0);
        $(this.display).append(dialog);
        $('#dialog').dialog();
        $($(dialog).parentsUntil(this.display).get(0)).addClass('n3q-ui-dialog');
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

    runtimeOnMessage(message, sender: chrome.runtime.MessageSender, sendResponse): any
    {
        switch (message.type) {
            case 'recvStanza': return this.handle_recvStanza(message.stanza); break;
        }
        return true;
    }

    handle_recvStanza(jsStanza: any): any
    {
        let stanza: xml = Utils.jsObject2xmlObject(jsStanza);
        log.debug('ContentApp.handle_recvStanza', stanza);

        switch (stanza.name) {
            case 'presence': this.onPresence(stanza);
            case 'message': this.onMessage(stanza);
        }
    }

    enterPage()
    {
        this.enterRoomByPageUrl(Platform.getCurrentPageUrl());
    }

    leavePage()
    {
        // Leave all, there should be only one
        for (let roomJid in this.rooms) {
            this.leaveRoomByJid(roomJid);
        }
    }

    static getRoomJidFromLocationUrl(locationUrl: string): string
    {
        let jid = '';
        let url = new URL(locationUrl);
        return url.pathname;
    }

    enterRoomByPageUrl(pageUrl: string): void
    {
        let url = new URL(Config.get('vp.locationMappingServiceUrl', 'http://lms.virtual-presence.org/api/'));
        url.searchParams.set('Method', 'VPI.Info');
        url.searchParams.set('sDocumentURL', pageUrl);
        url.searchParams.set('Format', 'json');

        Platform.fetchUrl(url.toString(), 'unversioned', (ok, status, statusText, data: string) =>
        {
            if (ok) {
                try {
                    let mappingResponse: ILocationMapperResponse = JSON.parse(data);
                    let locationUrl = mappingResponse.sLocationURL;
                    log.info('Mapped', pageUrl, ' => ', locationUrl);
                    let roomJid = ContentApp.getRoomJidFromLocationUrl(locationUrl);
                    this.enterRoomByJid(roomJid);
                } catch (error) {
                    log.info(error);
                }
            }
        });
    }

    async enterRoomByJid(roomJid: string): Promise<void>
    {
        if (this.rooms[roomJid] === undefined) {
            this.rooms[roomJid] = new Room(this, this.display, roomJid, await this.getSavedPosition());
        }
        log.info('ContentApp.enterRoomByJid', roomJid);
        this.rooms[roomJid].enter();
    }

    leaveRoomByJid(roomJid: string): void
    {
        log.info('ContentApp.leaveRoomByJid', roomJid);
        if (this.rooms[roomJid] != undefined) {
            this.rooms[roomJid].leave();
            delete this.rooms[roomJid];
        }
    }

    onPresence(stanza: xml): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (this.rooms[roomOrUser] != undefined) {
            this.rooms[roomOrUser].onPresence(stanza);
        }
    }

    onMessage(stanza: xml): void
    {
        let from = jid(stanza.attrs.from);
        let roomOrUser = from.bare();

        if (this.rooms[roomOrUser] != undefined) {
            this.rooms[roomOrUser].onMessage(stanza);
        }
    }

    sendStanza(stanza: xml): void
    {
        log.debug('ContentApp.sendStanza', stanza);
        try {
            chrome.runtime.sendMessage({ 'type': 'sendStanza', 'stanza': stanza });
        } catch (error) {
            Panic.now();
            // log.info(ex);
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

    translateElem(elem: HTMLElement): void
    {
        this.babelfish.translateElem(elem);
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
            let x = as.Int(await Platform.getLocalStorage('me.x', -1), -1);
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
            await Platform.setLocalStorage('me.x', x);
        } catch (error) {
            log.info(error);
        }
    }

    async getSavedPosition(): Promise<number>
    {
        let x = 0;

        try {
            x = as.Int(await Platform.getLocalStorage('me.x', -1), -1);
        } catch (error) {
            log.info(error);
        }

        if (x <= 0) {
            x = this.getDefaultPosition();
        }

        return x;
    }

    getDefaultPosition(): number
    {
        return Utils.randomInt(100, 500);
    }
}
