import log = require('loglevel');
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { InventoryWindow } from './InventoryWindow';
import { InventoryItem } from './InventoryItem';
import { Environment } from '../lib/Environment';

export class Inventory
{
    private itemServiceHost: string;
    private inventoryMucHost: string;
    private userToken: string;
    private jid: string;
    private resource: string = '_obs' + Utils.randomString(10);
    private items: { [id: string]: InventoryItem; } = {};
    private window: InventoryWindow;
    private isSubscribed: boolean;

    constructor(protected app: ContentApp, private providerId: string) 
    {
        this.userToken = this.app.getItemProviderConfigValue(this.providerId, 'userToken', '');
        let serviceUrl = this.app.getItemProviderConfigValue(this.providerId, 'serviceUrl', '');
        let backpackName = this.app.getItemProviderConfigValue(this.providerId, 'backpackRoom', '');
        this.resource = this.app.getItemProviderConfigValue(this.providerId, 'backpackParticipant', '');

        if (serviceUrl != '') {
            let url = new URL(serviceUrl);
            this.itemServiceHost = url.pathname;
        }

        let room = this.app.getRoom().getJid();
        if (room != '') {
            let roomJid = new jid(room);
            this.inventoryMucHost = roomJid.getDomain();
        }

        this.jid = backpackName + '@' + this.inventoryMucHost;
        if (Environment.isDevelopment()) {
            // this.jid = 'visualinventory@' + this.inventoryMucHost;
        }
    }

    getJid(): string { return this.jid; }
    getPane() { return this.window.getPane(); }
    getWindow() { return this.window; }
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
        let to = this.jid + '/' + this.resource;
        let presence = xml('presence', { 'to': to });
        this.app.sendStanza(presence);
        this.joinStarted();
    }

    private sendPresenceUnavailable(): void
    {
        let to = this.jid + '/' + this.resource;
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
                            this.window?.setStatus('');

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
                this.window?.setStatus('unavailable', 'Leaving');
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
        this.window?.setStatus('joining', 'Updates requested');
    }

    joinComplete()
    {
        this.window?.setStatus('joined', 'Updates acknowledged');
        this.sendPopulateInventoryCommand();
        this.window?.setStatus('populating', 'Items requested');
    }

    sendDerezItem(itemId: string, roomJid: string, x: number, y: number)
    {
        log.info('Inventory', 'sendDerezItem', itemId);
        this.sendDerezItemCommand(itemId, roomJid, Math.round(x), Math.round(y));
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

    sendRezItemCommand(itemId: string, room: string, x: number, destination: string)
    {
        let cmd = {};
        cmd['xmlns'] = 'vp:cmd';
        cmd['method'] = 'itemAction';
        cmd['action'] = 'Inventory.Rez';
        cmd['room'] = room;
        cmd['item'] = itemId;
        cmd['x'] = x;
        cmd['destination'] = destination;

        let to = this.userToken + '@' + this.itemServiceHost + '/' + itemId;
        let message = xml('message', { 'to': to }).append(xml('x', cmd));
        this.app.sendStanza(message);
    }

    sendDerezItemCommand(itemId: string, room: string, x: number, y: number)
    {
        let cmd = {};
        cmd['xmlns'] = 'vp:cmd';
        cmd['method'] = 'itemAction';
        cmd['action'] = 'Rezzed.Derez';
        cmd['user'] = this.userToken;
        cmd['item'] = itemId;
        cmd['x'] = x;
        cmd['y'] = y;

        let to = room + '/' + itemId;
        let message = xml('message', { 'type': 'chat', 'to': to }).append(xml('x', cmd));
        this.app.sendStanza(message);
    }

    sendItemActionCommand(itemId: string, action: string, params: any)
    {
        let cmd = {};
        cmd['xmlns'] = 'vp:cmd';
        cmd['method'] = 'itemAction';
        cmd['action'] = action;
        cmd['user'] = this.userToken;
        cmd['item'] = itemId;
        for (let paramName in params) {
            cmd[paramName] = params[paramName];
        }

        let to = this.jid + '/' + itemId;
        // let to = this.userToken + '@' + this.itemServiceHost + (itemId ? '/' + itemId : '');
        let message = xml('message', { 'type': 'chat', 'to': to }).append(xml('x', cmd));
        this.app.sendStanza(message);
    }

    sendPopulateInventoryCommand()
    {
        let cmd = {};
        cmd['xmlns'] = 'vp:cmd';
        cmd['method'] = 'populateInventory';
        cmd['user'] = this.userToken;
        cmd['room'] = this.jid;

        let to = this.userToken + '@' + this.itemServiceHost;
        let message = xml('message', { 'to': to }).append(xml('x', cmd));
        this.app.sendStanza(message);
    }
}
