import log = require('loglevel');
import { as } from '../lib/as';
import { xml, jid } from '@xmpp/client';
import { Config } from '../lib/Config';
import { BackgroundApp } from './BackgroundApp';

export class ProjectedItem
{
    constructor(private app: BackgroundApp, private room: RoomProjector, private itemId: string, private properties: { [pid: string]: string })
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
        let basicAttrs = { 'xmlns': 'vp:props', 'type': 'item', 'provider': 'nine3q' };
        let attrs = Object.assign(basicAttrs, this.properties);
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

    addItem(itemId: string, props: { [pid: string]: string; })
    {
        var projectedItem = this.items[itemId];
        if (projectedItem == null) {
            projectedItem = new ProjectedItem(this.app, this, itemId, props);
            this.items[itemId] = projectedItem;
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

    addItem(roomJid: string, itemId: string, props: { [pid: string]: string })
    {
        var roomProjector = this.rooms[roomJid];
        if (roomProjector == null) {
            roomProjector = new RoomProjector(this.app, roomJid);
            this.rooms[roomJid] = roomProjector;
        }
        roomProjector.addItem(itemId, props);
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
