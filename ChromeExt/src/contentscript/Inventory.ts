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
    private itemServer: string;
    private userToken: string;
    private inventoryJid: string;
    private resource: string = Utils.randomString(15);
    private items: { [id: string]: InventoryItem; } = {};
    private window: InventoryWindow;
    private isSubscribed: boolean;
    private isAvailable: boolean;

    constructor(private app: ContentApp, private providerId: string) 
    {
        this.userToken = Config.get('itemProviders.' + providerId + '.config.userToken', '');

        let serviceUrl = Config.get('itemProviders.' + providerId + '.config.serviceUrl', {});
        let url = new URL(serviceUrl);
        let protocol = url.protocol;
        if (protocol == 'xmpp:' && this.userToken != '') {
            this.itemServer = url.pathname;
            this.inventoryJid = this.userToken + '@' + this.itemServer;
            this.isAvailable = true;
        }
    }

    getJid(): string { return this.inventoryJid; }
    getPane() { return this.window.getPane(); }
    getWindow() { return this.window; }
    getAvailable() { return this.isAvailable; }
    getProviderId() { return this.providerId; }

    async open(options: any)
    {
        if (!this.window) {
            this.window = new InventoryWindow(this.app, this);
            await this.window.show(options);
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

    sendDerezItem(itemId: string, x: number, y: number)
    {
        log.info('Inventory', 'derez', itemId);

        let params = {
            'x': Math.round(x),
            'y': Math.round(y)
        };

        this.sendCommand(itemId, 'Derez', params);
    }

    findItem(pid: string, value: any)
    {
        for (var itemId in this.items) {
            if (this.items[itemId].match(pid, value)) {
                return itemId;
            }
        }
        return null;
    }

    sendCommand(itemId: string, action: string, params: any)
    {
        let cmd = {};
        cmd['xmlns'] = 'vp:cmd';
        // cmd['user'] = this.userToken;
        cmd['method'] = 'itemAction';
        cmd['action'] = action;
        for (let paramName in params) {
            cmd[paramName] = params[paramName];
        }

        let to = this.inventoryJid + (itemId ? '/' + itemId : '');

        let message = xml('message', { 'type': 'chat', 'to': to })
            .append(xml('x', cmd))
            ;
        this.app.sendStanza(message);
    }
}
