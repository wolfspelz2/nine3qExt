import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { BackgroundApp } from './BackgroundApp';
import { ItemProperties } from '../lib/ItemProperties';
import { Projector } from './Projector';

export class ClientItem
{
    private roomJid: string;

    constructor(private repository: ClientRepository, private itemId: string, private properties: ItemProperties)
    {
    }

    getProperties(): ItemProperties { return this.properties; }

    setRezzed(roomJid: string)
    {
        this.roomJid = roomJid;
    }
}

export class ClientRepository
{
    private items: { [id: string]: ClientItem; } = {};

    constructor(private app: BackgroundApp, private projector: Projector)
    {
    }

    addItem(itemId: string, props: ItemProperties)
    {
        var item = this.items[itemId];
        if (item == null) {
            item = new ClientItem(this, itemId, props);
            this.items[itemId] = item;
        }
    }

    rezItem(itemId: string, roomJid: string): void
    {
        var item = this.items[itemId];
        if (item) {
            this.projector.projectItem(itemId, roomJid, item.getProperties());
            this.items[itemId].setRezzed(roomJid);
        }
    }

}
