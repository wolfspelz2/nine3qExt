import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { BackgroundApp } from './BackgroundApp';
import { ItemProperties } from '../lib/ItemProperties';

export class ProjectedItem
{
    constructor(private app: BackgroundApp, private room: RoomProjector, private itemId: string, private properties: ItemProperties)
    {
    }

    /*
        $"<presence to='{to_XmlEncoded}' from='{from_XmlEncoded}'>"
            + $"<x xmlns='vp:props' type='item' provider='nine3q' Label='{name_XmlEncoded}' Width='{width_XmlEncoded}' Height='{height_XmlEncoded}' ImageUrl='{imageUrl_XmlEncoded}' RezzedAspect='true' RezzedX='{x_XmlEncoded}' />"
            + (forXmppMucWithFirebatSupport ? 
                    $"<x xmlns='firebat:user:identity' jid='{identityJid_XmlEncoded}' src='{identitySrc_XmlEncoded}' digest='{identityDigest_XmlEncoded}' />"
                + $"<x xmlns='firebat:avatar:state'>{position_Node}</x>"
                : "")
            + (withChatHistorySuppression ? $"<x xmlns='http://jabber.org/protocol/muc'><history seconds='0' maxchars='0' maxstanzas='0' /></x>" : "")
            + $"<x xmlns='vp:dependent'>{embeddedXml}</x>"
        + $"</presence>"

        var dependentFrom = roomJid + "/" + itemId;
        var dependentTo = "";
    */
    getPresence(): xml
    {
        var presence = xml('presence', { 'from': this.room.getJid() + '/' + this.itemId });
        let protocolAttrs = { 'xmlns': 'vp:props', 'type': 'item', 'provider': 'nine3q' };
        let attrs = Object.assign(protocolAttrs, this.properties);
        presence.append(xml('x', attrs));
        return presence;
    }
}

export class RoomProjector
{
    private items: { [id: string]: ProjectedItem; } = {};

    constructor(private app: BackgroundApp, private roomJid: string)
    {
    }

    getJid(): string { return this.roomJid; }

    projectItem(itemId: string, props: ItemProperties)
    {
        var item = this.items[itemId];
        if (item == null) {
            item = new ProjectedItem(this.app, this, itemId, props);
            this.items[itemId] = item;
        }
    }

    retractItem(itemId: string)
    {
        if (this.items[itemId]) {
            delete this.items[itemId];
        }
    }

    isItem(itemId: any): boolean
    {
        if (this.items[itemId]) {
            return true;
        }
        return false;
    }

    getDependentPresence(): xml
    {
        var result = xml('x', { 'xmlns': 'vp:dependent' });

        for (let id in this.items) {
            let itemPresence: xml = this.items[id].getPresence();
            result.append(itemPresence);
        }

        return result;
    }
}

export class Projector
{
    private rooms: { [jid: string]: RoomProjector; } = {};

    constructor(private app: BackgroundApp)
    {
    }

    projectItem(roomJid: string, itemId: string, props: ItemProperties)
    {
        var roomProjector = this.rooms[roomJid];
        if (roomProjector == null) {
            roomProjector = new RoomProjector(this.app, roomJid);
            this.rooms[roomJid] = roomProjector;
        }
        roomProjector.projectItem(itemId, props);
    }

    retractItem(roomJid: string, itemId: string)
    {
        var roomProjector = this.rooms[roomJid];
        if (roomProjector) {
            roomProjector.retractItem(itemId);
        }
    }

    isItem(roomJid: any, itemId: any): boolean
    {
        var roomProjector = this.rooms[roomJid];
        if (roomProjector) {
            return roomProjector.isItem(itemId);
        }
        return false;
    }

    stanzaOutFilter(stanza: xml): any
    {
        let toJid = new jid(stanza.attrs.to);
        let roomJid = toJid.bare().toString();
        let itemNick = toJid.getResource();

        if (stanza.name == 'presence') {
            if (as.String(stanza.attrs['type'], 'available') == 'available') {

                var roomProjector = this.rooms[roomJid];
                if (roomProjector) {
                    let dependentExtension = roomProjector.getDependentPresence();
                    if (dependentExtension) {
                        stanza.append(dependentExtension);
                    }
                }

            }
        }

        if (stanza.name == 'message') {
            if (as.String(stanza.attrs['type'], 'normal') == 'chat') {
                if (this.isItem(roomJid, itemNick)) {
                    stanza = null;
                }
            }
        }

        return stanza;
    }
}
