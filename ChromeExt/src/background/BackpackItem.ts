import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { ItemProperties } from '../lib/ItemProperties';
import { BackpackShowItemData, BackpackRemoveItemData, BackpackSetItemData, ContentMessage } from '../lib/ContentMessage';
import { BackgroundApp } from './BackgroundApp';
import { BackpackRepository } from './BackpackRepository';

export class BackpackItem
{
    constructor(private app: BackgroundApp, private backpack: BackpackRepository, private itemId: string, private properties: ItemProperties)
    {
    }

    getProperties(): ItemProperties { return this.properties; }

    setProperties(props: ItemProperties)
    {
        this.properties = props;

        let data = new BackpackSetItemData(this.itemId, props);
        this.app.sendToAllTabs(ContentMessage.type_onBackpackSetItem, data);
    }

    isRezzed(): boolean
    {
        return as.Bool(this.properties.IsRezzed, false);
    }

    getDependentPresence(roomJid: string): xml
    {
        var presence = xml('presence', { 'from': roomJid + '/' + this.itemId });
        let protocolAttrs = { 'xmlns': 'vp:props', 'type': 'item', 'provider': 'nine3q' };
        let attrs = Object.assign(protocolAttrs, this.properties);
        presence.append(xml('x', attrs));
        return presence;
    }
}
