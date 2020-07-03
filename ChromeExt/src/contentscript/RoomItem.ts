import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { Item } from './Item';

import imgDefaultItem from '../assets/DefaultItem.png';

export class RoomItem extends Entity
{
    private isFirstPresence: boolean = true;

    constructor(app: ContentApp, room: Room, private nick: string, isSelf: boolean)
    {
        super(app, room, isSelf);

        $(this.getElem()).addClass('n3q-item');
        $(this.getElem()).attr('data-nick', nick);
    }

    getDefaultAvatar(): string { return imgDefaultItem; }
    getNick(): string { return this.nick; }

    remove(): void
    {
        this.avatarDisplay?.stop();
        super.remove();
    }

    // presence

    async onPresenceAvailable(stanza: any): Promise<void>
    {
        let presenceHasPosition: boolean = false;
        let newX: number = 123;

        let presenceHasCondition: boolean = false;
        let newCondition: string = '';

        let vpAnimationsUrl = '';
        let vpImageUrl = '';

        let newProviderId: string = '';
        let newProperties: { [pid: string]: string } = {};

        // Collect info

        {
            let stateNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'firebat:avatar:state');
            if (stateNode) {
                let positionNode = stateNode.getChild('position');
                if (positionNode) {
                    newX = as.Int(positionNode.attrs.x, -1);
                    if (newX != -1) {
                        presenceHasPosition = true;
                    }
                }
                presenceHasCondition = true;
                let conditionNode = stateNode.getChild('condition');
                if (conditionNode) {
                    newCondition = as.String(conditionNode.attrs.status, '');
                }
            }
        }

        {
            let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
            if (vpPropsNode) {
                let attrs = vpPropsNode.attrs;
                if (attrs) {
                    newProviderId = as.String(attrs.provider, null);

                    for (let attrName in attrs) {
                        let attrValue = attrs[attrName];
                        if (attrName.endsWith('Url')) {
                            attrValue = this.app.itemProviderUrlFilter(newProviderId, attrName, attrValue);
                        }
                        newProperties[attrName] = attrValue;
                    }

                    // vpNickname = as.String(attrs.Nickname, '');
                    vpAnimationsUrl = as.String(newProperties.AnimationsUrl, '');
                    vpImageUrl = as.String(newProperties.ImageUrl, '');
                }
            }
        }

        // Do someting with the data

        // vpAnimationsUrl = 'https://weblin-avatar.dev.sui.li/items/baum/avatar.xml';
        // vpAnimationsUrl = '';
        // vpImageUrl = 'https://weblin-avatar.dev.sui.li/items/baum/idle.png';
        // vpImageUrl = '';

        if (this.isFirstPresence) {
            this.avatarDisplay = new Avatar(this.app, this, this.isSelf);
        }

        if (this.avatarDisplay) {
            if (vpAnimationsUrl != '') {
                let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://avatar.weblin.sui.li/avatar/?url={url}')).replace('{url}', encodeURIComponent(vpAnimationsUrl));
                this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
            } else {
                if (vpImageUrl != '') {
                    this.avatarDisplay?.updateObservableProperty('ImageUrl', vpImageUrl);
                }
            }
        }

        if (newProperties.Width && newProperties.Height) {
            var w = as.Int(newProperties.Width, -1);
            var h = as.Int(newProperties.Height, -1);
            if (w > 0 && h > 0) {
                this.avatarDisplay?.setSize(w, h);
            }
        }

        if (presenceHasCondition) {
            this.avatarDisplay?.setCondition(newCondition);
        }

        if (this.isFirstPresence) {
            if (!presenceHasPosition) {
                newX = this.isSelf ? await this.app.getSavedPosition() : this.app.getDefaultPosition(this.nick);
            }
            if (newX < 0) { newX = 100; }
            this.setPosition(newX);
        } else {
            if (presenceHasPosition) {
                if (this.getPosition() != newX) {
                    this.move(newX);
                }
            }
        }

        if (this.isFirstPresence) {
            this.show(true, Config.get('room.fadeInSec', 0.3));
        }

        if (this.isFirstPresence) {
            if (this.room?.iAmAlreadyHere()) {
                this.room?.showChatMessage(this.nick, 'appeared');
            } else {
                this.room?.showChatMessage(this.nick, 'is present');
            }
        }

        if (newProperties && newProviderId) {
            let item = this.app.getItemRepository().getItem(this.nick);
            if (item) {
                this.app.getItemRepository().getItem(this.nick).setProperties(newProperties);
            } else {
                this.app.getItemRepository().addItem(this.nick, newProviderId, newProperties);
            }
        }

        this.isFirstPresence = false;
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();

        this.room?.showChatMessage(this.nick, 'disappeared');
    }

    onMouseClickAvatar(ev: JQuery.Event): void
    {
        super.onMouseClickAvatar(ev);

        let item = this.app.getItemRepository().getItem(this.nick);
        if (item) {
            item.onClick(this.getElem());
        }
    }

    onQuickSlideReached(newX: number): void
    {
        super.onQuickSlideReached(newX);

        if (!this.isDerezzing) {
            this.sendMoveMessage(newX);
        }
    }

    sendMoveMessage(newX: number): void
    {
        this.sendCommand(this.nick, 'MoveTo', { 'x': newX });
    }

    sendCommand(itemId: string, action: string, params: any)
    {
        let item = this.app.getItemRepository().getItem(itemId);
        if (item) {
            let userToken = this.app.getItemProviderConfigValue(item.getProviderId(), 'userToken', '');
            if (userToken != '') {

                let cmd = {};
                cmd['xmlns'] = 'vp:cmd';
                cmd['user'] = userToken;
                cmd['method'] = 'itemAction';
                cmd['action'] = action;
                for (let paramName in params) {
                    cmd[paramName] = params[paramName];
                }

                let to = this.room.getJid() + '/' + itemId;

                let message = xml('message', { 'type': 'chat', 'to': to })
                    .append(xml('x', cmd))
                    ;
                this.app.sendStanza(message);
            }
        }
    }

    private isDerezzing: boolean = false;
    beginDerez(): void
    {
        this.isDerezzing = true;
        $(this.getElem()).hide();
    }
}
