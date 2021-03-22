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
import { Payload } from '../lib/Payload';
import { ItemExceptionToast, SimpleErrorToast, SimpleToast } from './Toast';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { RoomItemStats } from './RoomItemStats';
import { ItemFrameUnderlay } from './ItemFrameUnderlay';
import { ItemFrameWindow, ItemFrameWindowOptions } from './ItemFrameWindow';
import { ItemFramePopup } from './ItemFramePopup';
import { WeblinClientApi } from './IframeApi';

export class RoomItem extends Entity
{
    private properties: { [pid: string]: string } = {};
    private providerId: string;
    private frameWindow: ItemFrameWindow;
    private framePopup: ItemFramePopup;
    private scriptWindow: ItemFrameWindow;
    private isFirstPresence: boolean = true;
    protected statsDisplay: RoomItemStats;
    protected screenUnderlay: ItemFrameUnderlay;

    constructor(app: ContentApp, room: Room, roomNick: string, isSelf: boolean)
    {
        super(app, room, roomNick, isSelf);

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
    getProperties(): any { return this.properties; }
    setProperties(properties: { [pid: string]: string; })
    {
        this.properties = properties;
        if (as.Bool(this.properties[Pid.ScriptFrameAspect])) {
            this.sendPropertiesToScriptFrame(null);
        }
    }

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

        this.providerId = newProviderId;

        if (newProperties && newProviderId != '') {
            this.setProperties(newProperties);
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
                        if (! await this.room.propsClaimDefersToExistingClaim(props)) {
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
                    this.openIframe(this.getElem());
                }
            }
        }

        if (this.isFirstPresence) {
            if (as.Bool(this.getProperties()[Pid.ScriptFrameAspect], false)) {
                this.openScriptWindow(this.getElem());
            }
        }

        if (this.isFirstPresence) {
            if (this.room?.iAmAlreadyHere()) {
                if (Config.get('roomItem.chatlogItemAppeared', true)) {
                    this.room?.showChatMessage(this.getDisplayName(), 'appeared');
                }
            } else {
                if (Config.get('roomItem.chatlogItemIsPresent', true)) {
                    this.room?.showChatMessage(this.getDisplayName(), 'is present');
                }
            }
        }

        this.isFirstPresence = false;
    }

    onPresenceUnavailable(stanza: any): void
    {
        if (as.Bool(this.getProperties()[Pid.ScriptFrameAspect], false)) {
            this.closeScriptWindow();
        }

        if (Config.get('roomItem.chatlogItemDisappeared', true)) {
            this.room?.showChatMessage(this.getDisplayName(), 'disappeared');
        }
        this.remove();
    }

    onMouseClickAvatar(ev: JQuery.Event): void
    {
        super.onMouseClickAvatar(ev);

        if (as.Bool(this.properties[Pid.IframeAspect], false)) {
            let frame = as.String(JSON.parse(as.String(this.properties[Pid.IframeOptions], '{}')).frame, 'Window');
            if (frame == 'Popup') {
                if (this.framePopup) {
                    this.framePopup.close();
                } else {
                    this.openIframe(this.getElem());
                }
            } else {
                this.openIframe(this.getElem());
            }
        }

        this.statsDisplay?.close();
    }

    onDragAvatarStart(ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams): void
    {
        super.onDragAvatarStart(ev, ui);
        this.statsDisplay?.close();

        if (this.framePopup) {
            this.framePopup.close();
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

    onDraggedTo(newX: number): void
    {
        if (this.getPosition() != newX) {
            this.sendMoveMessage(newX);
        }
    }

    onQuickSlideReached(newX: number): void
    {
        super.onQuickSlideReached(newX);

        if (!this.isDerezzing) {
            this.sendMoveMessage(newX);
        }
    }

    async onMoveDestinationReached(newX: number): Promise<void>
    {
        super.onMoveDestinationReached(newX);

        let itemId = this.roomNick;
        if (await BackgroundMessage.isBackpackItem(itemId)) {
            let props = await BackgroundMessage.getBackpackItemProperties(itemId);
            if (as.Bool(props[Pid.ScriptFrameAspect], false)) {
                this.sendItemMovedToScriptFrame(newX);
            }
        }
    }

    async sendMoveMessage(newX: number): Promise<void>
    {
        let itemId = this.roomNick;
        if (await BackgroundMessage.isBackpackItem(itemId)) {
            BackgroundMessage.modifyBackpackItemProperties(itemId, { [Pid.RezzedX]: '' + newX }, [], {});
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

    positionItemFrame(itemId: string, width: number, height: number, left: number, bottom: number)
    {
        this.positionFrame(width, height, left, bottom);
    }

    sendMessageToScreenItemFrame(message: any)
    {
        this.screenUnderlay?.sendMessage(message);
    }

    async setItemProperty(pid: string, value: any)
    {
        if (await BackgroundMessage.isBackpackItem(this.roomNick)) {
            BackgroundMessage.modifyBackpackItemProperties(this.roomNick, { [pid]: value }, [], {});
        }
    }

    async openDocumentUrl(aboveElem: HTMLElement)
    {
        let url = as.String(this.properties[Pid.DocumentUrl], null);
        let room = this.app.getRoom();
        let apiUrl = Config.get('itemProviders.' + this.providerId + '.config.' + 'apiUrl', '');
        let userId = await Memory.getSync(Utils.syncStorageKey_Id(), '');

        if (url != '' && room && apiUrl != '' && userId != '') {
            let tokenOptions = {};
            if (await BackgroundMessage.isBackpackItem(this.roomNick)) {
                tokenOptions['properties'] = await BackgroundMessage.getBackpackItemProperties(this.roomNick);
            } else {
                tokenOptions['properties'] = this.properties;
            }
            let contextToken = await Payload.getContextToken(apiUrl, userId, this.roomNick, 600, { 'room': room.getJid() }, tokenOptions);
            url = url.replace('{context}', encodeURIComponent(contextToken));

            let documentOptions = JSON.parse(as.String(this.properties[Pid.DocumentOptions], '{}'));
            this.openIframeWindow(aboveElem, url, documentOptions);
        }
    }

    async openIframe(clickedElem: HTMLElement)
    {
        let iframeUrl = as.String(this.properties[Pid.IframeUrl], null);
        let room = this.app.getRoom();
        let apiUrl = Config.get('itemProviders.' + this.providerId + '.config.' + 'apiUrl', '');
        let userId = await Memory.getSync(Utils.syncStorageKey_Id(), '');

        if (iframeUrl != '' && room && apiUrl != '' && userId != '') {
            // iframeUrl = 'https://jitsi.vulcan.weblin.com/{room}#userInfo.displayName="{name}"';
            let roomJid = room.getJid();
            let tokenOptions = {};
            if (await BackgroundMessage.isBackpackItem(this.roomNick)) {
                tokenOptions['properties'] = await BackgroundMessage.getBackpackItemProperties(this.roomNick);
            } else {
                tokenOptions['properties'] = this.properties;
            }
            try {
                let contextToken = await Payload.getContextToken(apiUrl, userId, this.roomNick, 600, { 'room': roomJid }, tokenOptions);
                iframeUrl = iframeUrl
                    .replace('{context}', encodeURIComponent(contextToken))
                    .replace('{room}', encodeURIComponent(roomJid))
                    .replace('{name}', encodeURIComponent(this.getDisplayName()))
                    ;

                let iframeOptions = JSON.parse(as.String(this.properties[Pid.IframeOptions], '{}'));
                if (as.String(iframeOptions.frame, 'Window') == 'Popup') {
                    this.openIframePopup(clickedElem, iframeUrl, iframeOptions);
                } else {
                    this.openIframeWindow(clickedElem, iframeUrl, iframeOptions);
                }
            } catch (error) {
                log.info('RepositoryItem.openIframe', error);
            }
        }
    }

    openIframePopup(clickedElem: HTMLElement, iframeUrl: string, frameOptions: any)
    {
        if (this.framePopup == null) {
            this.framePopup = new ItemFramePopup(this.app);

            let options: ItemFrameWindowOptions = {
                item: this,
                elem: clickedElem,
                url: iframeUrl,
                onClose: () => { this.framePopup = null; },
                width: as.Int(frameOptions.width, 100),
                height: as.Int(frameOptions.height, 100),
                left: as.Int(frameOptions.left, -frameOptions.width / 2),
                bottom: as.Int(frameOptions.bottom, 50),
                resizable: as.Bool(frameOptions.rezizable, true),
                transparent: as.Bool(frameOptions.transparent, true)
            }

            this.framePopup.show(options);
        }
    }

    openIframeWindow(clickedElem: HTMLElement, iframeUrl: string, windowOptions: any)
    {
        if (this.frameWindow == null) {
            this.frameWindow = new ItemFrameWindow(this.app);

            let options: ItemFrameWindowOptions = {
                item: this,
                elem: clickedElem,
                url: iframeUrl,
                onClose: () => { this.frameWindow = null; },
                width: as.Int(windowOptions.width, 100),
                height: as.Int(windowOptions.height, 100),
                left: as.Int(windowOptions.left, -windowOptions.width / 2),
                bottom: as.Int(windowOptions.bottom, 50),
                resizable: as.Bool(windowOptions.rezizable, true),
                undockable: as.Bool(windowOptions.undockable, true),
                transparent: as.Bool(windowOptions.transparent, true),
                titleText: as.String(this.properties[Pid.Label], 'Item'),
            }

            this.frameWindow.show(options);
        }
    }

    async openScriptWindow(itemElem: HTMLElement)
    {
        if (this.scriptWindow != null) { return; }

        this.scriptWindow = new ItemFrameWindow(this.app);

        let iframeUrl = as.String(this.properties[Pid.ScriptFrameUrl], null);
        let room = this.app.getRoom();
        let apiUrl = Config.get('itemProviders.' + this.providerId + '.config.' + 'apiUrl', '');
        let userId = await Memory.getSync(Utils.syncStorageKey_Id(), '');

        if (iframeUrl != '' && room && apiUrl != '' && userId != '') {
            let tokenOptions = {};
            try {
                let contextToken = await Payload.getContextToken(apiUrl, userId, this.roomNick, 600, { 'room': room.getJid() }, tokenOptions);
                iframeUrl = iframeUrl.replace('{context}', encodeURIComponent(contextToken));

                let options: ItemFrameWindowOptions = {
                    item: this,
                    elem: itemElem,
                    url: iframeUrl,
                    width: 400,
                    height: 300,
                    left: -200,
                    bottom: 200,
                    resizable: true,
                    undockable: false,
                    transparent: false,
                    titleText: as.String(this.properties[Pid.Label], 'Item'),
                    onClose: () => { this.scriptWindow = null; },
                }

                this.scriptWindow.show(options);

            } catch (error) {
                log.info('RepositoryItem.openScriptFrame', error);
            }
        }
    }

    async closeScriptWindow()
    {
        this.scriptWindow?.close();
    }

    sendPropertiesToScriptFrame(requestId: string)
    {
        this.scriptWindow?.getIframeElem()?.contentWindow?.postMessage({ 'tr67rftghg_Rezactive': true, type: 'Item.Properties', id: requestId, properties: this.properties }, '*');
    }

    sendParticipantsToScriptFrame(requestId: string, participants: Array<WeblinClientApi.ParticipantData>)
    {
        this.scriptWindow?.getIframeElem()?.contentWindow?.postMessage({ 'tr67rftghg_Rezactive': true, type: 'Room.Participants', id: requestId, participants: participants }, '*');
    }

    sendParticipantMovedToScriptFrame(participant: WeblinClientApi.ParticipantData)
    {
        this.scriptWindow?.getIframeElem()?.contentWindow?.postMessage({ 'tr67rftghg_Rezactive': true, type: 'Participant.Moved', participant: participant }, '*');
    }

    sendParticipantChatToScriptFrame(participant: WeblinClientApi.ParticipantData, text: string)
    {
        this.scriptWindow?.getIframeElem()?.contentWindow?.postMessage({ 'tr67rftghg_Rezactive': true, type: 'Participant.Chat', participant: participant, text: text }, '*');
    }

    sendItemMovedToScriptFrame(newX: number)
    {
        this.scriptWindow?.getIframeElem()?.contentWindow?.postMessage({ 'tr67rftghg_Rezactive': true, type: 'Item.Moved', x: newX }, '*');
    }

    positionFrame(width: number, height: number, left: number, bottom: number)
    {
        this.framePopup?.position(width, height, left, bottom);
    }

    closeFrame()
    {
        if (this.framePopup) {
            this.framePopup.close();
            this.framePopup = null;
        } else if (this.frameWindow) {
            this.frameWindow.close();
            this.frameWindow = null;
        }
    }
}
