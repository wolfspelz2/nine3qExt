import imgDefaultItem from '../assets/DefaultItem.png';

import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Point2D, Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { ItemProperties, Pid } from '../lib/ItemProperties';
import { Memory } from '../lib/Memory';
import { ItemException } from '../lib/ItemExcption';
import { ItemExceptionToast, SimpleErrorToast, SimpleToast } from './Toast';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { RoomItemStats } from './RoomItemStats';
import { ItemFrameUnderlay } from './ItemFrameUnderlay';

export class RoomItem extends Entity
{
    private isFirstPresence: boolean = true;
    protected statsDisplay: RoomItemStats;
    protected screenUnderlay: ItemFrameUnderlay;

    constructor(app: ContentApp, room: Room, private roomNick: string, isSelf: boolean)
    {
        super(app, room, isSelf);

        $(this.getElem()).addClass('n3q-item');
        $(this.getElem()).attr('data-nick', roomNick);

        if (Config.get('backpack.enabled', false)) {
            $(this.getElem()).hover(() =>
            {
                this.statsDisplay?.close();
                this.statsDisplay = new RoomItemStats(this.app, this, () => { this.statsDisplay = null; });
                this.statsDisplay.show();
            }, () =>
            {
                this.statsDisplay?.close();
            });
        }
    }

    getDefaultAvatar(): string { return imgDefaultItem; }
    getRoomNick(): string { return this.roomNick; }
    getDisplayName(): string { return as.String(this.getProperties()[Pid.Label], this.roomNick); }
    getProviderId(): string { return this.app.getItemRepository().getItem(this.roomNick).getProviderId(); }
    getProperties(): ItemProperties { return this.app.getItemRepository().getItem(this.roomNick).getProperties(); }

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
        let vpRezzedX = -1;

        let newProviderId: string = '';
        let newProperties: ItemProperties = {};

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

        if (await BackgroundMessage.isBackpackItem(this.roomNick)) {
            newProperties = await BackgroundMessage.getBackpackItemProperties(this.roomNick);
            newProviderId = as.String(newProperties[Pid.Provider], '');
        } else {
            let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
            if (vpPropsNode) {
                let attrs = vpPropsNode.attrs;
                if (attrs) {
                    newProviderId = as.String(attrs.provider, '');

                    for (let attrName in attrs) {
                        newProperties[attrName] = attrs[attrName];
                    }
                }
            }
        }

        if (newProperties && newProviderId != '') {
            let item = this.app.getItemRepository().getItem(this.roomNick);
            if (item) {
                this.app.getItemRepository().getItem(this.roomNick).setProperties(newProperties);
            } else {
                this.app.getItemRepository().addItem(this.roomNick, newProviderId, newProperties);
            }
        }

        vpAnimationsUrl = as.String(newProperties[Pid.AnimationsUrl], '');
        vpImageUrl = as.String(newProperties[Pid.ImageUrl], '');
        vpRezzedX = as.Int(newProperties[Pid.RezzedX], -1);

        // Do someting with the data

        if (this.isFirstPresence) {
            let props = newProperties;
            if (as.Bool(props[Pid.ClaimAspect], false)) {
                // The new item has a claim
                let claimingRoomItem = this.room.getPageClaimItem();
                if (claimingRoomItem) {
                    // There already is a claim
                    if (await BackgroundMessage.isBackpackItem(this.roomNick)) {
                        // The new item is my own item
                        // Should remove the lesser one of my 2 claim items
                    } else {
                        // The new item is a remote item
                        if (!this.room.claimDefersToExisting(props)) {
                            // The new item is better
                            if (await BackgroundMessage.isBackpackItem(claimingRoomItem.getRoomNick())) {
                                // The existing claim is mine
                                await BackgroundMessage.derezBackpackItem(claimingRoomItem.getRoomNick(), this.room.getJid(), -1, -1, {});
                                new SimpleToast(this.app, 'ClaimDerezzed', Config.get('room.claimToastDurationSec', 15), 'notice', this.app.translateText('Toast.Your claim has been removed'), 'A stronger item just appeared').show();
                            }
                        }
                    }
                }
            }
        }

        if (this.isFirstPresence) {
            this.avatarDisplay = new Avatar(this.app, this, false);
            if (Config.get('backpack.enabled', false)) {
                this.avatarDisplay.addClass('n3q-item-avatar');
            }
            if (as.Bool(newProperties[Pid.ApplierAspect], false)) {
                this.avatarDisplay.makeDroppable();
            }
        }

        if (this.avatarDisplay) {
            if (vpAnimationsUrl != '') {
                let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://webex.vulcan.weblin.com/Avatar/InlineData?url={url}')).replace('{url}', encodeURIComponent(vpAnimationsUrl));
                this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
            } else {
                if (vpImageUrl != '') {
                    this.avatarDisplay?.updateObservableProperty('ImageUrl', vpImageUrl);
                }
            }
        }

        if (this.statsDisplay) {
            this.statsDisplay.update();
        }

        if (this.isFirstPresence) {
            if (as.Bool(this.getProperties()[Pid.ScreenAspect], false)) {
                this.screenUnderlay = new ItemFrameUnderlay(this.app, this);
                this.screenUnderlay.show();
            }
        } else {
            if (this.screenUnderlay) {
                this.screenUnderlay.update();
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

        if (this.isFirstPresence) {
            if (as.Bool(this.getProperties()[Pid.IframeAspect], false)) {
                if (as.Bool(this.getProperties()[Pid.IframeAuto], false)) {
                    let item = this.app.getItemRepository().getItem(this.roomNick);
                    if (item) {
                        item.openIframe(this.getElem());
                    }
                }
            }
        }

        // if (this.isFirstPresence) {
        //     if (this.room?.iAmAlreadyHere()) {
        //         this.room?.showChatMessage(this.getDisplayName(), 'appeared');
        //     } else {
        //         this.room?.showChatMessage(this.getDisplayName(), 'is present');
        //     }
        // }

        this.isFirstPresence = false;
    }

    onPresenceUnavailable(stanza: any): void
    {
        // this.room?.showChatMessage(this.getDisplayName(), 'disappeared');
        this.remove();
    }

    onMouseClickAvatar(ev: JQuery.Event): void
    {
        super.onMouseClickAvatar(ev);

        let item = this.app.getItemRepository().getItem(this.roomNick);
        if (item) {
            item.onClick(this.getElem(), new Point2D(ev.clientX, ev.clientY));
        }

        this.statsDisplay?.close();
    }

    onDragAvatarStart(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
        super.onDragAvatarStart(ev, ui);
        this.statsDisplay?.close();

        let item = this.app.getItemRepository().getItem(this.roomNick);
        if (item) {
            item.onDragStart(this.getElem(), new Point2D(ev.clientX, ev.clientY));
        }
    }

    onDragAvatarStop(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
        if (!this.isDerezzing) {
            let dX = ui.position.left - this.dragStartPosition.left;
            let newX = this.getPosition() + dX;
            this.onDraggedTo(newX);
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
            await this.sendItemActionCommand('Rezzed.MoveTo', { 'x': newX });
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
                if (Config.get('points.enabled', false)) {
                    /* await */ BackgroundMessage.pointsActivity(Pid.PointsChannelItemApply, 1);
                }
            } catch (ex) {
                // new SimpleErrorToast(this.app, 'Warning-' + error.fact + '-' + error.reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', error.fact, error.reason, error.detail).show();
                let fact = typeof ex.fact === 'number' ? ItemException.Fact[ex.fact] : ex.fact;
                let reason = typeof ex.reason === 'number' ? ItemException.Reason[ex.reason] : ex.reason;
                let detail = ex.detail;
                new SimpleErrorToast(this.app, 'Warning-' + fact + '-' + reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', fact, reason, detail).show();
            }

        } else {
            await this.sendItemActionCommand('Applier.Apply', { 'passive': passiveItemId });
        }
    }

    async sendItemActionCommand(action: string, params: any)
    {
        let itemId = this.roomNick;
        let item = this.app.getItemRepository().getItem(itemId);
        if (item) {
            let userId = await Memory.getSync(Utils.syncStorageKey_Id(), '');
            if (userId != '') {

                let cmd = {};
                cmd['xmlns'] = 'vp:cmd';
                cmd['method'] = 'itemAction';
                cmd['action'] = action;
                cmd['user'] = userId;
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

    endDerez(): void
    {
        this.isDerezzing = false;
    }

    sendsendMessageToScreenFrame(message: any)
    {
        this.screenUnderlay?.sendMessage(message);
    }
}
