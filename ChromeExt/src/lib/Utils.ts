import { xml } from '@xmpp/client';
import { uniqueNamesGenerator, Config, adjectives, colors, animals } from 'unique-names-generator';

export class Point2D
{
    constructor(public x: number, public y: number) { }
}

export class Utils
{
    static localStorageKey_X(): string { return 'me.x'; }
    static localStorageKey_Active(): string { return 'me.active'; }
    static localStorageKey_StayOnTabChange(roomJid: string): string { return 'room.' + roomJid + '.stayOnTabChange'; }
    static localStorageKey_BackpackIsOpen(roomJid: string): string { return 'room.' + roomJid + '.backpackIsOpen'; }
    static localStorageKey_VidconfIsOpen(roomJid: string): string { return 'room.' + roomJid + '.vidconfIsOpen'; }
    static localStorageKey_ChatIsOpen(roomJid: string): string { return 'room.' + roomJid + '.chatIsOpen'; }
    static localStorageKey_CustomConfig(): string { return 'dev.config'; }
    static syncStorageKey_Id(): string { return 'me.id'; }
    static syncStorageKey_Nickname(): string { return 'me.nickname'; }
    static syncStorageKey_Avatar(): string { return 'me.avatar'; }

    static jsObject2xmlObject(stanza: any): xml
    {
        let children = [];
        if (stanza.children != undefined) {
            stanza.children.forEach((child: any) =>
            {
                if (typeof child === typeof '') {
                    children.push(child);
                } else {
                    children.push(this.jsObject2xmlObject(child));
                }
            });
        }
        return xml(stanza.name, stanza.attrs, children);
    }

    static async sleep(ms: number): Promise<void>
    {
        ms = ms < 0 ? 0 : ms;
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    private static randomStringChars = '0123456789abcdefghijklmnopqrstuvwxyz';
    static randomString(length: number): string
    {
        var maxIndex: number = Utils.randomStringChars.length - 1;
        var result = '';
        for (var i = length; i > 0; --i) {
            result += Utils.randomStringChars[Math.round(Math.random() * maxIndex)];
        }
        return result;
    }

    static randomInt(min: number, max: number): number
    {
        let f = Math.random() * (max - min) + min;
        f = Math.min(max - 0.001, f);
        f = Math.max(min, f);
        let i = Math.trunc(f);
        return i;
    }

    static pseudoRandomInt(min: number, max: number, key: string, suffix: string, mod: number): number
    {
        let hash = Utils.hash(key + suffix) % mod;
        let f = min + (max - min) / mod * hash;
        let i = Math.trunc(f);
        return i;
    }

    static randomNickname(): string
    {
        const customConfig: Config = {
            dictionaries: [colors, animals],
            separator: ' ',
            length: 2,
            style: 'capital',
        };

        const randomName: string = uniqueNamesGenerator(customConfig);
        return randomName;
    }

    static hash(s: string): number
    {
        var hash = 0;
        if (s.length == 0) return 0;

        s += 'abcd';

        for (let i = 0; i < s.length; i++) {
            let char = s.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash;
        }

        return Math.abs(hash);
    }

    static base64Encode(s: string): string
    {
        return window.btoa(s);
    }

    static base64Decode(s: string): string
    {
        return window.atob(s);
    }
}
