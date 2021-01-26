import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D } from '../lib/Utils';
import { Config } from '../lib/Config';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { ItemProperties, Pid } from '../lib/ItemProperties';
import { ItemChangeOptions } from '../lib/ItemChangeOptions';
import { RpcProtocol } from '../lib/RpcProtocol';
import { ItemExceptionToast, SimpleErrorToast } from './Toast';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';

import imgDefaultItem from '../assets/DefaultItem.png';
import { ItemException } from '../lib/ItemExcption';

export class RoomItem extends Entity
{
    private isFirstPresence: boolean = true;

    constructor(app: ContentApp, room: Room, private roomNick: string, isSelf: boolean)
    {
        super(app, room, isSelf);

        $(this.getElem()).addClass('n3q-item');
        $(this.getElem()).attr('data-nick', roomNick);
    }

    getDefaultAvatar(): string { return imgDefaultItem; }
    getRoomNick(): string { return this.roomNick; }
    getProviderId(): string { return this.app.getItemRepository().getItem(this.roomNick).getProviderId(); }

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

        let itemId: string = this.roomNick;

        let vpAnimationsUrl = '';
        let vpImageUrl = '';
        let vpRezzedX = -1;

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

        if (await BackgroundMessage.isBackpackItem(itemId)) {
            newProperties = await BackgroundMessage.getBackpackItemProperties(itemId);
            newProviderId = as.String(newProperties[Pid.Provider], '');
        } else {
            let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
            if (vpPropsNode) {
                let attrs = vpPropsNode.attrs;
                if (attrs) {
                    newProviderId = as.String(attrs.provider, '');

                    for (let attrName in attrs) {
                        let attrValue = attrs[attrName];
                        if (attrName.endsWith('Url')) {
                            attrValue = ContentApp.itemProviderUrlFilter(newProviderId, attrName, attrValue);
                        }
                        newProperties[attrName] = attrValue;
                    }
                }
            }
        }

        // vpNickname = as.String(attrs.Nickname, '');
        vpAnimationsUrl = as.String(newProperties[Pid.AnimationsUrl], '');
        vpImageUrl = as.String(newProperties[Pid.ImageUrl], '');
        vpRezzedX = as.Int(newProperties[Pid.RezzedX], -1);

        // Do someting with the data

        // vpAnimationsUrl = 'https://weblin-avatar.dev.sui.li/items/baum/avatar.xml';
        // vpAnimationsUrl = '';
        // vpImageUrl = 'https://weblin-avatar.dev.sui.li/items/baum/idle.png';
        // vpImageUrl = '';

        if (this.isFirstPresence) {
            this.avatarDisplay = new Avatar(this.app, this, false);
            if (newProperties[Pid.ApplierAspect]) {
                this.avatarDisplay.makeDroppable();
            }
        }

        if (this.avatarDisplay) {
            if (vpAnimationsUrl != '') {
                let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://webex.vulcan.weblin.com/Avatar/DataUrl?url={url}')).replace('{url}', encodeURIComponent(vpAnimationsUrl));
                this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
            } else {
                if (vpImageUrl != '') {
                    this.avatarDisplay?.updateObservableProperty('ImageUrl', vpImageUrl);
                }
            }
        }

        if (newProperties[Pid.Width] && newProperties[Pid.Height]) {
            var w = as.Int(newProperties[Pid.Width], -1);
            var h = as.Int(newProperties[Pid.Height], -1);
            if (w > 0 && h > 0) {
                this.avatarDisplay?.setSize(w, h);
            }
        }

        if (presenceHasCondition) {
            this.avatarDisplay?.setCondition(newCondition);
        }

        if (vpRezzedX >= 0) {
            newX = vpRezzedX;
        }

        if (this.isFirstPresence) {
            if (!presenceHasPosition && vpRezzedX < 0) {
                newX = this.isSelf ? await this.app.getSavedPosition() : this.app.getDefaultPosition(this.roomNick);
            }
            if (newX < 0) { newX = 100; }
            this.setPosition(newX);
        } else {
            if (presenceHasPosition || vpRezzedX >= 0) {
                if (this.getPosition() != newX) {
                    this.move(newX);
                }
            }
        }

        if (this.isFirstPresence) {
            this.show(true, Config.get('room.fadeInSec', 0.3));
        }

        if (newProperties && newProviderId != '') {
            let item = this.app.getItemRepository().getItem(this.roomNick);
            if (item) {
                this.app.getItemRepository().getItem(this.roomNick).setProperties(newProperties);
            } else {
                this.app.getItemRepository().addItem(this.roomNick, newProviderId, newProperties);
            }
        }

        if (this.isFirstPresence) {
            let props = this.app.getItemRepository().getItem(this.roomNick).getProperties();
            let label = as.String(props[Pid.Label], this.roomNick);
            if (this.room?.iAmAlreadyHere()) {
                this.room?.showChatMessage(label, 'appeared');
            } else {
                this.room?.showChatMessage(label, 'is present');
            }
        }

        this.isFirstPresence = false;
    }

    onPresenceUnavailable(stanza: any): void
    {
        let props = this.app.getItemRepository().getItem(this.roomNick).getProperties();
        let label = as.String(props[Pid.Label], this.roomNick);

        this.remove();

        this.room?.showChatMessage(label, 'disappeared');
    }

    onMouseClickAvatar(ev: JQuery.Event): void
    {
        super.onMouseClickAvatar(ev);

        let item = this.app.getItemRepository().getItem(this.roomNick);
        if (item) {
            item.onClick(this.getElem(), new Point2D(ev.clientX, ev.clientY));
        }
    }

    onDragAvatarStart(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
        super.onDragAvatarStart(ev, ui);

        let item = this.app.getItemRepository().getItem(this.roomNick);
        if (item) {
            item.onDrag(this.getElem(), new Point2D(ev.clientX, ev.clientY));
        }
    }

    onQuickSlideReached(newX: number): void
    {
        super.onQuickSlideReached(newX);

        if (!this.isDerezzing) {
            this.sendMoveMessage(newX);
        }
    }

    async sendMoveMessage(newX: number): Promise<void>
    {
        let itemId = this.roomNick;
        if (await BackgroundMessage.isBackpackItem(itemId)) {
            BackgroundMessage.modifyBackpackItemProperties(itemId, { [Pid.RezzedX]: '' + newX }, [], {});
        } else {
            this.sendItemActionCommand('Rezzed.MoveTo', { 'x': newX });
        }
    }

    async applyItem(passiveItem: RoomItem)
    {
        let itemId = this.roomNick;
        let passiveItemId = passiveItem.getRoomNick();

        if (!await BackgroundMessage.isBackpackItem(passiveItemId)) {
            let fact = ItemException.Fact[ItemException.Fact.NotApplied];
            let reason = ItemException.Reason[ItemException.Reason.NotYourItem];
            let detail = passiveItemId;
            new SimpleErrorToast(this.app, 'Warning-' + fact + '-' + reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', fact, reason, detail).show();
            return;
        }

        if (await BackgroundMessage.isBackpackItem(itemId)) {

            try {
                await BackgroundMessage.executeBackpackItemAction(itemId, 'Applier.Apply', { 'passive': passiveItemId }, [itemId, passiveItemId]);
            } catch (ex) {
                // new SimpleErrorToast(this.app, 'Warning-' + error.fact + '-' + error.reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', error.fact, error.reason, error.detail).show();
                let fact = typeof ex.fact === 'number' ? ItemException.Fact[ex.fact] : ex.fact;
                let reason = typeof ex.reason === 'number' ? ItemException.Reason[ex.reason] : ex.reason;
                let detail = ex.detail;
                new SimpleErrorToast(this.app, 'Warning-' + fact + '-' + reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', fact, reason, detail).show();
            }

        } else {
            this.sendItemActionCommand('Applier.Apply', { 'passive': passiveItemId });
        }
    }

    sendItemActionCommand(action: string, params: any)
    {
        let itemId = this.roomNick;
        let item = this.app.getItemRepository().getItem(itemId);
        if (item) {
            let userToken = ContentApp.getItemProviderConfigValue(item.getProviderId(), 'userToken', '');
            if (userToken != '') {

                let cmd = {};
                cmd['xmlns'] = 'vp:cmd';
                cmd['method'] = 'itemAction';
                cmd['action'] = action;
                cmd['user'] = userToken;
                cmd['item'] = itemId;
                for (let paramName in params) {
                    cmd[paramName] = params[paramName];
                }

                let to = this.room.getJid() + '/' + itemId;
                let message = xml('message', { 'type': 'chat', 'to': to }).append(xml('x', cmd));
                this.app.sendStanza(message);
            }
        }
    }

    private isDerezzing: boolean = false;
    beginDerez(): void
    {
        this.isDerezzing = true;
        $(this.getElem()).hide().delay(1000).show(0);
    }
}
