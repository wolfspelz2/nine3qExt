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
    private itemServiceHost: string;
    private inventoryMucHost: string;
    private userToken: string;
    private inventoryJid: string;
    private resource: string = Utils.randomString(15);
    private items: { [id: string]: InventoryItem; } = {};
    private window: InventoryWindow;
    private isSubscribed: boolean;
    private isAvailable: boolean;

    constructor(protected app: ContentApp, private providerId: string) 
    {
        this.userToken = this.app.getItemProviderConfigValue(providerId, 'userToken', '');
        let serviceUrl = this.app.getItemProviderConfigValue(providerId, 'serviceUrl', '');

        if (serviceUrl != '') {
            let url = new URL(serviceUrl);
            this.itemServiceHost = url.pathname;
        }

        let room = app.getRoom().getJid();
        if (room != '') {
            let roomJid = new jid(room);
            this.inventoryMucHost = roomJid.getDomain();
        }

        if (this.userToken != '' && this.itemServiceHost != '' && this.inventoryMucHost != '') {
            this.inventoryJid = 'vis' + Utils.randomString(30).toLowerCase() + '@' + this.inventoryMucHost;
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
        this.joinStarted();
    }

    private sendPresenceUnavailable(): void
    {
        let to = this.inventoryJid + '/' + this.resource;
        let presence = xml('presence', { 'type': 'unavailable', 'to': to });
        this.app.sendStanza(presence);
    }

    onPresence(stanza: xml): void
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
                    this.window?.setStatus('ok');
                    if (isSelf) {
                        this.joinComplete();
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
                this.window?.setStatus('ok');
                if (this.items[resource]) {
                    this.items[resource].onPresenceUnavailable(stanza);
                    delete this.items[resource];
                }

                break;

            case 'error':
                let from = '';
                let to = '';
                let type = '';
                let text = '';

                if (stanza.attrs) { from = stanza.attrs?.from; }
                if (stanza.attrs) { to = stanza.attrs?.to; }
                if (stanza.children) {
                    let child = stanza.children[0];
                    if (child) {
                        if (child.children) {
                            child.children.forEach(node =>
                            {
                                if (node.name == 'text') {
                                    text = node.text();
                                } else {
                                    type = node.name;
                                }
                            });
                        }
                    }
                }

                log.info('error', from, to, type, text);
                this.window?.setStatus('error', text, { 'type': type, 'from': from, 'to': to });
                break;
        }
    }

    joinStarted()
    {
    }

    joinComplete()
    {
        this.sendPopulateInventoryCommand();
    }

    sendDerezItem(itemId: string, x: number, y: number)
    {
        log.info('Inventory', 'derez', itemId);

        let params = {
            'user': this.userToken,
            'x': Math.round(x),
            'y': Math.round(y),
        };

        this.sendItemActionCommand(itemId, 'Derez', params);
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

    sendItemActionCommand(itemId: string, action: string, params: any)
    {
        let cmd = {};
        cmd['xmlns'] = 'vp:cmd';
        cmd['method'] = 'itemAction';
        cmd['action'] = action;
        for (let paramName in params) {
            cmd[paramName] = params[paramName];
        }

        let to = this.userToken + '@' + this.itemServiceHost + (itemId ? '/' + itemId : '');

        let message = xml('message', { 'type': 'chat', 'to': to })
            .append(xml('x', cmd))
            ;
        this.app.sendStanza(message);
    }

    sendPopulateInventoryCommand()
    {
        let cmd = {};
        cmd['xmlns'] = 'vp:cmd';
        cmd['method'] = 'populateInventory';
        cmd['room'] = this.inventoryJid;

        let to = this.userToken + '@' + this.itemServiceHost;

        let message = xml('message', { 'type': 'chat', 'to': to })
            .append(xml('x', cmd))
            ;
        this.app.sendStanza(message);
    }
}
