import log = require('loglevel');
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { InventoryWindow } from './InventoryWindow';
import { InventoryItem } from './InventoryItem';

export class Inventory
{
    private inventoryJid: string;
    private resource: string = Utils.randomString(15);
    private items: { [id: string]: InventoryItem; } = {};
    private window: InventoryWindow;
    private isSubscribed: boolean;

    constructor(private app: ContentApp, private providerId: string) 
    {
        let serviceUrl = Config.get('itemProviders.' + providerId + '.config.serviceUrl', {});
        let userToken = Config.get('itemProviders.' + providerId + '.config.userToken', '');
        let url = new URL(serviceUrl);
        let protocol = url.protocol;

        if (protocol == 'xmpp:' && userToken != '') {
            let chatServer = url.pathname;
            let roomName = userToken;
            this.inventoryJid = roomName + '@' + chatServer;
        }
    }

    getJid(): string { return this.inventoryJid; }
    getDisplay() { return this.window.getDisplay(); }

    open(options: any)
    {
        if (!this.window) {
            this.window = new InventoryWindow(this.app);
            this.window.show(options);
            this.subscribe();
        }
    }

    close()
    {
        if (this.window) {
            this.window.close();
            this.window = null;
        }
        this.unsubscribe();
    }

    onUnload()
    {
        // desparately try to...
        this.unsubscribe();
    }

    private subscribe(): void
    {
        if (!this.isSubscribed) {
            this.sendPresence();
            this.isSubscribed = true;
        }
    }

    private unsubscribe(): void
    {
        if (this.isSubscribed) {
            this.sendPresenceUnavailable();
            this.isSubscribed = false;
        }
    }

    private sendPresence(): void
    {
        let to = this.inventoryJid + '/' + this.resource;
        let presence = xml('presence', { 'to': to });
        this.app.sendStanza(presence);
        this.populateStarted();
    }

    private sendPresenceUnavailable(): void
    {
        let to = this.inventoryJid + '/' + this.resource;
        let presence = xml('presence', { 'type': 'unavailable', 'to': to });
        this.app.sendStanza(presence);
    }

    onPresence(stanza: any): void
    {
        let from = jid(stanza.attrs.from);
        let resource = from.getResource();

        let presenceType = as.String(stanza.attrs.type, '');
        if (presenceType == '') {
            presenceType = 'available';
        }

        let isSelf = (resource == this.resource);

        switch (presenceType) {
            case 'available':
                {
                    if (isSelf) {
                        this.populateComplete();
                    } else {
                        let isItem = false;

                        // presence x.vp:props type='item' 
                        let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
                        if (vpPropsNode) {
                            let attrs = vpPropsNode.attrs;
                            if (attrs) {
                                let type = as.String(attrs.type, '');
                                isItem = (type == 'item');
                            }
                        }

                        if (isItem) {
                            let item = this.items[resource];
                            if (!item) {
                                item = new InventoryItem(this.app, this, resource);
                                this.items[resource] = item;
                            }

                            item.onPresenceAvailable(stanza);
                        }
                    }
                }

                break;

            case 'unavailable':
                if (this.items[resource]) {
                    this.items[resource].onPresenceUnavailable(stanza);
                    delete this.items[resource];
                }

                break;
        }
    }

    populateStarted()
    {
    }

    populateComplete()
    {
    }
}
