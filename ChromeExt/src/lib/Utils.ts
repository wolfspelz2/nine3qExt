import { xml } from '@xmpp/client';
import { uniqueNamesGenerator, Config, adjectives, colors, animals } from 'unique-names-generator';

export class Utils
{
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

    static async sleep(ms): Promise<void>
    {
        ms = ms < 0 ? 0 : ms;
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    private static randomStringChars = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
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
        let f = Math.random() * (max - min) + min);
        f = Math.min(max - 0.001, f);
        f = Math.max(min, f);
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
}
