import { xml } from '@xmpp/client';

interface ConfigGetCallback { (value: any): void }
interface ConfigSetCallback { (): void }

export class Config
{
    private static onlineConfig: any = {};
    private static staticConfig: any = {
        'locationMappingServiceUrl': 'http://lms.virtual-presence.org/api/',
        'speedInPixelPerMsec': 0.1,
        'doubleClickDelay': 20,
        'maxMucEnterRetries': 4,
        'roomEnterPosXMin': 400,
        'roomEnterPosXMax': 700,
        'maxChatDelaySec': 60,
        'xmpp': {
            'service': 'wss://xmpp.weblin.sui.li/xmpp-websocket',
            'domain': 'xmpp.weblin.sui.li',
            'user': 'at4peaaa1o74iuv2us4h4v8u4k',
            'pass': 'e8149223956afdd79a4345fcdf884e9c502c1bea',
        },
        'avatars': {
            'baseUrl': 'https://avatar.weblin.sui.li/avatar/?url=http://avatar.zweitgeist.com/gif/',
            'list': [
                '002/sportive03_m',
                '002/business03_m',
                '002/child02_m',
                '002/sportive01_m',
                '002/business06_m',
                '002/casual04_f',
                '002/business01_f',
                '002/casual30_m',
                '002/sportive03_f',
                '002/casual16_m',
                '002/casual10_f',
                '002/business03_f',
                '002/casual03_m',
                '002/sportive07_m',
                '002/casual13_f',
                '002/casual09_m',
                '002/casual16_f',
                '002/child02_f',
                '002/sportive08_m',
                '002/casual15_m',
                '002/casual15_f',
                '002/casual01_f',
                '002/casual11_f',
                '002/sportive09_m',
                '002/casual20_f',
                '002/sportive02_f',
                '002/business05_m',
                '002/casual06_m',
                '002/casual10_m',
                '002/casual02_f',
            ]
        }
    }

    public static get(key: string, defaultValue: any): any
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

    public static getPreferLocal(key: string, defaultValue: any, callback: ConfigGetCallback): void
    {
        Config.getLocal(key, value =>
        {
            if (value == undefined || value == null) {
                value = this.get(key, defaultValue);
            }
            callback(value);
        });
    }

    public static getOnline(key: string): any
    {
        return Config.getFromTree(this.onlineConfig, key);
    }

    public static getStatic(key: string): any
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

    private static getLocal(key: string, getComplete: ConfigGetCallback): void
    {
        chrome.storage.sync.get([key], function (result)
        {
            getComplete(result.key);
        });
    }

    public static setLocal(key: string, value: any, setComplete: ConfigSetCallback): void
    {
        chrome.storage.sync.set({ key: value }, setComplete);
    }

    public static getAllStatic(): any
    {
        return this.staticConfig;
    }

    public static getAllOnline(): any
    {
        return this.onlineConfig;
    }

    public static setAllStatic(values: any): void
    {
        this.staticConfig = values;
    }

    public static setAllOnline(values: any): void
    {
        this.onlineConfig = values;
    }
}
