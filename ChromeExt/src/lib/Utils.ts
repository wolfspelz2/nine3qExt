import { xml } from '@xmpp/client';

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

    private static randomStringChars = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
    public static randomString(length: number): string
    {
        var maxIndex: number = Utils.randomStringChars.length - 1;
        var result = '';
        for (var i = length; i > 0; --i) {
            result += Utils.randomStringChars[Math.round(Math.random() * maxIndex)];
        }
        return result;
    }

    public static randomInt(min: number, max: number): number
    {
        return Math.trunc(Math.random() * (max - min) + min);
    }
}
