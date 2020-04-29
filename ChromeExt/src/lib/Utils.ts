import { xml } from '@xmpp/client';

export class Utils
{
    static jsObject2xmlObject(stanza: any): xml
    {
        let children = [];
        if (stanza.children != undefined) {
            stanza.children.forEach((child: any) => { children.push(this.jsObject2xmlObject(child)); });
        }
        return xml(stanza.name, stanza.attrs, children);
    }
}
