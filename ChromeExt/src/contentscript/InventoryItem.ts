import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { Inventory } from './Inventory';

import imgDefaultItem from '../assets/DefaultIcon.png';

export class InventoryItem
{
    private isFirstPresence: boolean = true;
    private properties: { [pid: string]: string } = {};

    constructor(app: ContentApp, inv: Inventory, private id: string)
    {
    }

    getDefaultIcon(): string { return imgDefaultItem; }

    remove(): void
    {
    }

    // presence

    onPresenceAvailable(stanza: any): void
    {

    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();
    }
}
