import { xml } from '@xmpp/client';
import log = require('loglevel');
import { Utils } from './Utils';

interface ConfigGetCallback { (value: any): void }
interface ConfigSetCallback { (): void }

export class Config
{
    private static tempConfig: any = {};

    private static devConfig: any = {};

    private static onlineConfig: any = {};

    private static staticConfig: any = {
        'me': {
            'nickname': '',//'新しいアバター',//'new-avatar',
            'avatar': '',
        },
        'vp': {
            'locationMappingServiceUrl': 'http://lms.virtual-presence.org/api/',
            'deferPageEnterSec': 1.0,
        },
        'config': {
            'serviceUrl': 'https://config.weblin.sui.li/',
            'updateIntervalSec': Utils.randomInt(60000, 80000),
            'checkUpdateIntervalSec': 600,
        },
        'room': {
            'defaultAvatarSpeedPixelPerSec': 100,
            'randomEnterPosXMin': 300,
            'randomEnterPosXMax': 600,
            'showNicknameTooltip': true,
            'avatarDoubleClickDelaySec': 0.1,
            'maxChatAgeSec': 60,
            'chatWindowWidth': 400,
            'chatWindowHeight': 250,
            'chatWindowMaxHeight': 800,
        },
        'xmpp': {
            'service': 'wss://xmpp.weblin.sui.li/xmpp-websocket',
            'domain': 'xmpp.weblin.sui.li',
            'maxMucEnterRetries': 4,
        },
        'avatars': {
            'animationsUrlTemplate': 'http://avatar.zweitgeist.com/gif/{id}/config.xml',
            'animationsProxyUrlTemplate': 'https://avatar.weblin.sui.li/avatar/?url={url}',
            'list': ['002/sportive03_m', '002/business03_m', '002/child02_m', '002/sportive01_m', '002/business06_m', '002/casual04_f', '002/business01_f', '002/casual30_m', '002/sportive03_f', '002/casual16_m', '002/casual10_f', '002/business03_f', '002/casual03_m', '002/sportive07_m', '002/casual13_f', '002/casual09_m', '002/casual16_f', '002/child02_f', '002/sportive08_m', '002/casual15_m', '002/casual15_f', '002/casual01_f', '002/casual11_f', '002/sportive09_m', '002/casual20_f', '002/sportive02_f', '002/business05_m', '002/casual06_m', '002/casual10_m', '002/casual02_f',],
            'randomList': ['002/sportive03_m', '002/business03_m', '002/child02_m', '002/sportive01_m', '002/business06_m', '002/casual04_f', '002/business01_f', '002/casual30_m', '002/sportive03_f', '002/casual16_m', '002/casual10_f', '002/business03_f', '002/casual03_m', '002/sportive07_m', '002/casual13_f', '002/casual09_m', '002/casual16_f', '002/child02_f', '002/sportive08_m', '002/casual15_m', '002/casual15_f', '002/casual01_f', '002/casual11_f', '002/sportive09_m', '002/casual20_f', '002/sportive02_f', '002/business05_m', '002/casual06_m', '002/casual10_m', '002/casual02_f',],
        },
        'identity': {
            'identificatorUrlTemplate': 'https://avatar.weblin.sui.li/identity/?nickname={nickname}&avatarUrl={avatarUrl}',
        },
        'i18n': {
            'defaultLanguage': 'en-US',
            'languageMapping': {
                'de': 'de-DE',
            },
            'translations': {
                'en-US': {
                    'Common.Close': 'Close',

                    'Chatin.Enter chat here...': 'Enter chat here...',
                    'Chatin.SendChat': 'Send chat',

                    'Popup.title': 'Configure your avatar',
                    'Popup.description': 'Change name and avatar, press [save], and then reload the page.',
                    'Popup.Name': 'Name',
                    'Popup.Random': 'Random',
                    'Popup.Avatar': 'Avatar',
                    'Popup.Save': 'Save',

                    'Menu.Chat': 'Chat',
                    'Menu.Chat Window': 'History',
                    'Menu.Actions': 'Actions:',
                    'Menu.wave': 'Wave',
                    'Menu.dance': 'Dance',
                    'Menu.cheer': 'Cheer',
                    'Menu.kiss': 'Kiss',
                    'Menu.clap': 'Clap',
                    'Menu.laugh': 'Laugh',
                    'Menu.angry': 'Angry',
                    'Menu.deny': 'Deny',
                    'Menu.yawn': 'Yawn',

                    'Chatwindow.Chat History': 'Chat',
                },
                'de-DE': {
                    'Common.Close': 'Schließen',

                    'Chatin.Enter chat here...': 'Chat Text hier eingeben...',
                    'Chatin.SendChat': 'Chat abschicken',

                    'Popup.title': 'Avatar Einstellungen',
                    'Popup.description': 'Wähle Name und Avatar, dann drücke [Speichern] und lade die Seite neu.',
                    'Popup.Name': 'Name',
                    'Popup.Random': 'Zufallsname',
                    'Popup.Avatar': 'Avatar',
                    'Popup.Save': 'Speichern',

                    'Menu.Chat': 'Chat',
                    'Menu.Chat Window': 'Verlauf',
                    'Menu.Actions': 'Aktionen:',
                    'Menu.wave': 'Winken',
                    'Menu.dance': 'Tanzen',
                    'Menu.cheer': 'Jubeln',
                    'Menu.kiss': 'Küssen',
                    'Menu.clap': 'Klatschen',
                    'Menu.laugh': 'Lachen',
                    'Menu.angry': 'Ärgern',
                    'Menu.deny': 'Ablehnen',
                    'Menu.yawn': 'Gähnen',

                    'Chatwindow.Chat History': 'Chat',
                },
            },
            'serviceUrl': '',
        },
        'last': 0,
    }

    static get(key: string, defaultValue: any): any
    {
        let result = Config.getTemp(key);
        if (result == undefined || result == null) {
            result = Config.getDev(key);
        }
        if (result == undefined || result == null) {
            result = Config.getOnline(key);
        }
        if (result == undefined || result == null) {
            result = Config.getStatic(key);
        }
        if (result == undefined || result == null) {
            result = defaultValue;
        }
        return result;
    }

    static set(key: string, value: any): void
    {
        this.tempConfig[key] = value;
    }

    static async getPreferSync(key: string, defaultValue: any)
    {
        let result = await Config.getSync(key, undefined);
        if (result == undefined) {
            result = Config.get(key, defaultValue);
        }
        return result;
    }

    static getTemp(key: string): any
    {
        return this.tempConfig[key];
    }

    static getDev(key: string): any
    {
        return Config.getFromTree(this.devConfig, key);
    }

    static getOnline(key: string): any
    {
        return Config.getFromTree(this.onlineConfig, key);
    }

    static getStatic(key: string): any
    {
        return Config.getFromTree(this.staticConfig, key);
    }

    private static getFromTree(tree: any, key: string): any
    {
        let parts = key.split('.');
        let current = tree;
        parts.forEach(part =>
        {
            if (current != undefined && current != null && current[part] != undefined) {
                current = current[part];
            } else {
                current = null;
            }
        });
        return current;
    }

    static async getSync(key: string, defaultValue: any): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            if (chrome.storage != undefined) {
                chrome.storage.sync.get([key], result =>
                {
                    if (result[key] != undefined) {
                        resolve(result[key]);
                    } else {
                        resolve(defaultValue);
                    }
                });
            } else {
                reject('chrome.storage undefined');
            }
        });
    }

    static async setSync(key: string, value: any): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            let dict = {};
            dict[key] = value;
            if (chrome.storage != undefined) {
                chrome.storage.sync.set(dict, () => { resolve(); });
            } else {
                reject('chrome.storage undefined');
            }
        });
    }

    static async getLocal(key: string, defaultValue: any): Promise<any>
    {
        return new Promise(resolve =>
        {
            chrome.storage.local.get([key], result =>
            {
                if (result[key] != undefined) {
                    resolve(result[key]);
                } else {
                    resolve(defaultValue);
                }
            });
        });
    }

    static async setLocal(key: string, value: any): Promise<void>
    {
        return new Promise(resolve =>
        {
            let dict = {};
            dict[key] = value;
            chrome.storage.local.set(dict, () => { resolve(); });
        });
    }

    static getAllStatic(): any
    {
        return this.staticConfig;
    }

    static getAllOnline(): any
    {
        return this.onlineConfig;
    }

    static setAllStatic(values: any): void
    {
        log.debug('Config.setAllStatic');
        this.staticConfig = values;
    }

    static setAllOnline(values: any): void
    {
        log.debug('Config.setAllOnline');
        this.onlineConfig = values;
    }

    static setAllDev(values: any)
    {
        this.devConfig = values;
    }
}
