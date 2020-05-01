import { xml } from '@xmpp/client';
import log = require('loglevel');
import { Utils } from './Utils';

interface ConfigGetCallback { (value: any): void }
interface ConfigSetCallback { (): void }

export class Config
{
    private static onlineConfig: any = {};

    private static staticConfig: any = {
        'me': {
            'nickname': '',//'新しいアバター',//'new-avatar',
            'avatar': '',
        },
        'vp': {
            'locationMappingServiceUrl': 'http://lms.virtual-presence.org/api/',
            'deferPageEnterSec': 2,
        },
        'config': {
            'seviceUrl': 'https://config.weblin.sui.li/',
            'updateIntervalSec': Utils.randomInt(60000, 80000),
            'checkUpdateIntervalSec': 600,
        },
        'room': {
            'defaultAvatarSpeedPixelPerSec': 100,
            'randomEnterPosXMin': 400,
            'randomEnterPosXMax': 700,
            'showNicknameTooltip': true,
            'avatarDoubleClickDelaySec': 0.3,
            'maxChatAgeSec': 60,
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
    }

    static get(key: string, defaultValue: any): any
    {
        let result = Config.getOnline(key);
        if (result == undefined || result == null) {
            result = Config.getStatic(key);
        }
        if (result == undefined || result == null) {
            result = defaultValue;
        }
        return result;
    }

    static async getPreferLocal(key: string, defaultValue: any)
    {
        let result = await Config.getLocal(key, undefined);
        if (result == undefined) {
            result = Config.get(key, defaultValue);
        }
        return result;
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

    static async getLocal(key: string, defaultValue: any): Promise<any>
    {
        return new Promise(resolve =>
        {
            chrome.storage.sync.get([key], result =>
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
            chrome.storage.sync.set(dict, () => { resolve(); });
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
}