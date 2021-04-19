import log = require('loglevel');
import { as } from '../lib/as';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Config } from '../lib/Config';
import { ItemException } from '../lib/ItemExcption';
import { Pid } from '../lib/ItemProperties';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { SimpleErrorToast, SimpleToast } from './Toast';

export class IframeApi
{
    private messageHandler: (ev: any) => any;

    constructor(protected app: ContentApp) 
    {
    }

    start(): IframeApi
    {
        this.messageHandler = this.getMessageHandler();
        window.addEventListener('message', this.messageHandler)

        return this;
    }

    stop(): IframeApi
    {
        try {
            window.removeEventListener('message', this.messageHandler)
        } catch (error) {
            //            
        }

        return this;
    }

    getMessageHandler()
    {
        var self = this;
        function onMessageClosure(ev: any)
        {
            return self.onMessage(ev);
        }
        return onMessageClosure;
    }

    onMessage(ev: any): any
    {
        if (Config.get('log.iframeApi', false)) {
            log.debug('IframeApi.onMessage', ev);
        }

        let request = <WeblinClientApi.Request>ev.data;

        if (request[Config.get('iframeApi.messageMagicW2WMigration', 'hbv67u5rf_w2wMigrate')]) {
            let cid = (<any>request).cid;
            if (cid) {
                let nickname = as.String((<any>request).nickname, cid);
                /* await */ this.handle_W2WMigration(cid, nickname);
            }
            return;
        }

        if (request[Config.get('iframeApi.messageMagicCreateCryptoWallet', 'tr67rftghg_CreateCryptoWallet')]) {
            let address = (<any>request).address;
            let network = (<any>request).network;
            if (address != null && network != null) {
                /* await */ this.handle_CreateCryptoWallet(address, network);
            }
            return;
        }

        // if (request[Config.get('iframeApi.messageMagic', 'a67igu67puz_iframeApi')]) {
        switch (request.type) {
            case WeblinClientApi.ItemActionRequest.type: {
                /* await */ this.handle_ItemActionRequest(<WeblinClientApi.ItemActionRequest>request);
            } break;
            case WeblinClientApi.ItemGetPropertiesRequest.type: {
                this.handle_ItemGetPropertiesRequest(<WeblinClientApi.ItemGetPropertiesRequest>request);
            } break;
            case WeblinClientApi.ItemSetPropertyRequest.type: {
                this.handle_ItemSetPropertyRequest(<WeblinClientApi.ItemSetPropertyRequest>request);
            } break;
            case WeblinClientApi.ItemSetStateRequest.type: {
                this.handle_ItemSetStateRequest(<WeblinClientApi.ItemSetStateRequest>request);
            } break;
            case WeblinClientApi.ItemSetConditionRequest.type: {
                this.handle_ItemSetConditionRequest(<WeblinClientApi.ItemSetConditionRequest>request);
            } break;
            case WeblinClientApi.RoomGetParticipantsRequest.type: {
                this.handle_RoomGetParticipantsRequest(<WeblinClientApi.RoomGetParticipantsRequest>request);
            } break;
            case WeblinClientApi.RoomGetInfoRequest.type: {
                this.handle_RoomGetInfoRequest(<WeblinClientApi.RoomGetInfoRequest>request);
            } break;
            case WeblinClientApi.WindowPositionRequest.type: {
                this.handle_PositionWindowRequest(<WeblinClientApi.WindowPositionRequest>request);
            } break;
            case WeblinClientApi.ScreenContentMessageRequest.type: {
                this.handle_ScreenContentMessageRequest(<WeblinClientApi.ScreenContentMessageRequest>request);
            } break;
            case WeblinClientApi.WindowOpenDocumentUrlRequest.type: {
                this.handle_WindowOpenDocumentUrlRequest(<WeblinClientApi.WindowOpenDocumentUrlRequest>request);
            } break;

            // case WeblinClientApi.ClientNotificationRequest.type: new WeblinClientApi.ClientNotificationRequest(request).handle(); break;
            case WeblinClientApi.ClientNotificationRequest.type: {
                this.handle_ClientNotificationRequest(<WeblinClientApi.ClientNotificationRequest>request);
            } break;

            case WeblinClientApi.WindowCloseRequest.type: {
                this.handle_CloseWindowRequest(<WeblinClientApi.WindowCloseRequest>request);
            } break;
            case WeblinClientApi.WindowSetVisibilityRequest.type: {
                this.handle_WindowSetVisibilityRequest(<WeblinClientApi.WindowSetVisibilityRequest>request);
            } break;

            case WeblinClientApi.BackpackSetVisibilityRequest.type: {
                this.handle_BackpackSetVisibilityRequest(<WeblinClientApi.BackpackSetVisibilityRequest>request);
            } break;
        }
        // }
    }

    async handle_W2WMigration(cid: string, nickname: string)
    {
        try {
            let name = Utils.randomString(29);
            let nick = this.app.getRoom().getMyNick();
            let participant = this.app.getRoom().getParticipant(nick);
            let x = participant.getPosition() + 120;
            let props = await BackgroundMessage.createBackpackItemFromTemplate('Migration', { [Pid.MigrationCid]: cid });
            let itemId = props[Pid.Id];
            await BackgroundMessage.rezBackpackItem(itemId, this.app.getRoom().getJid(), x, this.app.getRoom().getDestination(), {});
            await BackgroundMessage.executeBackpackItemAction(itemId, 'Migration.CreateItems', {}, [itemId]);
            await BackgroundMessage.deleteBackpackItem(itemId, {});
        } catch (ex) {
            log.info('IframeApi.handle_W2WMigration', ex);
        }
    }

    async handle_CreateCryptoWallet(address: string, network: string)
    {
        try {

            let propSet = await BackgroundMessage.findBackpackItemProperties({ [Pid.Web3WalletAspect]: 'true', [Pid.Web3WalletAddress]: address, [Pid.Web3WalletNetwork]: network });
            for (let id in propSet) {
                let toast = new SimpleToast(this.app, 'backpack-duplicateWalletItem', Config.get('room.errorToastDurationSec', 8), 'warning', 'Duplicate item', this.app.translateText('Toast.This would create an identical item')).show();
                return;
            }

            let nick = this.app.getRoom().getMyNick();
            let participant = this.app.getRoom().getParticipant(nick);
            let x = participant.getPosition() + 120;
            let props = await BackgroundMessage.createBackpackItemFromTemplate('CryptoWallet', { [Pid.Web3WalletAddress]: address, [Pid.Web3WalletNetwork]: network });
            let itemId = props[Pid.Id];
            await BackgroundMessage.rezBackpackItem(itemId, this.app.getRoom().getJid(), x, this.app.getRoom().getDestination(), {});
            await BackgroundMessage.loadWeb3BackpackItems();
        } catch (ex) {
            log.info('IframeApi.handle_CreateCryptoWallet', ex);
        }
    }

    handle_CloseWindowRequest(request: WeblinClientApi.WindowCloseRequest)
    {
        try {
            let item = this.app.getRoom().getItem(request.item);
            if (item) {
                item.closeFrame();
            }
        } catch (ex) {
            log.info('IframeApi.handle_CloseWindowRequest', ex);
        }
    }

    handle_WindowSetVisibilityRequest(request: WeblinClientApi.WindowSetVisibilityRequest)
    {
        try {
            let item = this.app.getRoom().getItem(request.item);
            if (item) {
                item.setFrameVisibility(request.visible);
            }
        } catch (ex) {
            log.info('IframeApi.handle_WindowSetVisibilityRequest', ex);
        }
    }

    handle_BackpackSetVisibilityRequest(request: WeblinClientApi.BackpackSetVisibilityRequest)
    {
        try {
            let nick = this.app.getRoom().getMyNick();
            let participant = this.app.getRoom().getParticipant(nick);
            if (participant) {
                this.app.showBackpackWindow(participant.getElem());
            }
        } catch (ex) {
            log.info('IframeApi.handle_BackpackSetVisibilityRequest', ex);
        }
    }

    handle_ItemGetPropertiesRequest(request: WeblinClientApi.ItemGetPropertiesRequest)
    {
        try {
            let roomItem = this.app.getRoom().getItem(request.item);
            if (roomItem) {
                roomItem.sendPropertiesToScriptFrame(request.id);
            }
        } catch (ex) {
            log.info('IframeApi.handle_ItemGetPropertiesRequest', ex);
        }
    }

    handle_ItemSetPropertyRequest(request: WeblinClientApi.ItemSetPropertyRequest)
    {
        try {
            let roomItem = this.app.getRoom().getItem(request.item);
            if (roomItem) {
                roomItem.setItemProperty(request.pid, request.value);
            }

        } catch (ex) {
            log.info('IframeApi.handle_ItemSetPropertyRequest', ex);
        }
    }

    handle_ItemSetStateRequest(request: WeblinClientApi.ItemSetStateRequest)
    {
        try {
            let roomItem = this.app.getRoom().getItem(request.item);
            if (roomItem) {
                roomItem.setItemState(request.state);
            }

        } catch (ex) {
            log.info('IframeApi.handle_ItemSetStateRequest', ex);
        }
    }

    handle_ItemSetConditionRequest(request: WeblinClientApi.ItemSetConditionRequest)
    {
        try {
            let roomItem = this.app.getRoom().getItem(request.item);
            if (roomItem) {
                roomItem.setItemCondition(request.condition);
            }

        } catch (ex) {
            log.info('IframeApi.handle_ItemSetConditionRequest', ex);
        }
    }

    handle_RoomGetParticipantsRequest(request: WeblinClientApi.RoomGetParticipantsRequest)
    {
        try {
            let data = new Array<WeblinClientApi.ParticipantData>();
            let room = this.app.getRoom();
            let itemId = request.item;

            let participantIds = room.getParticipantIds();
            for (let i = 0; i < participantIds.length; i++) {
                let participant = room.getParticipant(participantIds[i]);
                let participantData = {
                    id: participant.getRoomNick(),
                    nickname: participant.getDisplayName(),
                    x: participant.getPosition(),
                    isSelf: participant.getIsSelf(),
                };
                data.push(participantData);
            }

            let roomItem = room.getItem(itemId);
            if (roomItem) {
                roomItem.sendParticipantsToScriptFrame(request.id, data);
            }
        } catch (ex) {
            log.info('IframeApi.handle_RoomGetParticipantsRequest', ex);
        }
    }

    handle_RoomGetInfoRequest(request: WeblinClientApi.RoomGetInfoRequest)
    {
        try {
            let data = new WeblinClientApi.RoomInfo();
            let room = this.app.getRoom();
            let itemId = request.item;

            data.destination = room.getDestination();
            data.jid = room.getJid();

            let roomItem = room.getItem(itemId);
            if (roomItem) {
                roomItem.sendRoomInfoToScriptFrame(request.id, data);
            }
        } catch (ex) {
            log.info('IframeApi.handle_RoomGetParticipantsRequest', ex);
        }
    }

    handle_WindowOpenDocumentUrlRequest(request: WeblinClientApi.WindowOpenDocumentUrlRequest)
    {
        try {
            let roomItem = this.app.getRoom().getItem(request.item);
            if (roomItem) {
                roomItem.openDocumentUrl(roomItem.getElem());
            }
        } catch (ex) {
            log.info('IframeApi.handle_WindowOpenDocumentUrlRequest', ex);
        }
    }

    handle_ClientNotificationRequest(request: WeblinClientApi.ClientNotificationRequest)
    {
        try {
            BackgroundMessage.clientNotification(as.String(request.target, 'notCurrentTab'), request);
        } catch (ex) {
            log.info('IframeApi.handle_ClientNotificationRequest', ex);
        }
    }

    handle_PositionWindowRequest(request: WeblinClientApi.WindowPositionRequest)
    {
        try {
            let roomItem = this.app.getRoom().getItem(request.item);
            if (roomItem) {
                roomItem.positionFrame(request.width, request.height, request.left, request.bottom);
            }
        } catch (ex) {
            log.info('IframeApi.handle_PositionWindowRequest', ex);
        }
    }

    handle_ScreenContentMessageRequest(request: WeblinClientApi.ScreenContentMessageRequest)
    {
        try {
            let roomItem = this.app.getRoom().getItem(request.item);
            if (roomItem) {
                roomItem.sendMessageToScreenItemFrame(request.message);
            }
        } catch (ex) {
            log.info('IframeApi.handle_ScreenContentMessageRequest', ex);
        }
    }

    async handle_ItemActionRequest(request: WeblinClientApi.ItemActionRequest)
    {
        try {
            let itemId = request.item;
            let actionName = request.action;
            let args = request.args;
            await BackgroundMessage.executeBackpackItemAction(itemId, actionName, args, [itemId]);
            // let props = await BackgroundMessage.getBackpackItemProperties(itemId);
            // this.app.updateItemFrame(itemId, props);
        } catch (ex) {
            // if (ex instanceof ItemException) {
            //     new ItemExceptionToast(this.app, Config.get('room.applyItemErrorToastDurationSec', 5), ex).show();
            // } else {
            //     new SimpleErrorToast(this.app, 'Warning-' + ex.fact + '-' + ex.reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', ex.fact, ex.reason, ex.detail).show();
            // }
            if (ex.fact) {
                let fact = typeof ex.fact === 'number' ? ItemException.Fact[ex.fact] : ex.fact;
                let reason = typeof ex.reason === 'number' ? ItemException.Reason[ex.reason] : ex.reason;
                let detail = ex.detail;
                new SimpleErrorToast(this.app, 'Warning-' + fact + '-' + reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', fact, reason, detail).show();
            } else {
                new SimpleErrorToast(this.app, 'Warning-UnknownError', Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', 'Error', 'UnknownReason', ex.message).show();
            }
        }
    }
}


export class WeblinClientIframeApi
{
    sendMessage(message: WeblinClientApi.Message)
    {
        window.parent.postMessage({ 'message': message }, '*');
    }
}

export namespace WeblinClientApi
{
    export class Message
    {
        type: string;
        version: string;
    }

    export class Request extends Message
    {
        id?: string; // request Id

        constructor(data: any)
        {
            super();
            Object.assign(this, data);
        }

        handle(): void
        {
            throw new Error('Method not implemented.');
        }
    }

    export class Response extends Message
    {
        ok: boolean;
        id?: string;
    }

    export class ErrorResponse extends Response
    {
        error: string;
    }

    export class ItemErrorResponse extends Response
    {
        fact: string;
        reason: string;
        detail: string;
    }

    export class WindowOpenDocumentUrlRequest extends Request
    {
        static type = 'Window.OpenDocumentUrl';
        item: string;
    }

    export class ClientNotificationRequest extends Request
    {
        static type = 'Client.Notification';

        title: string;
        text: string;

        target?: string; // ['currentTab'|'notCurrentTab'|'activeTab'|'allTabs']
        static defaultTarget = 'notCurrentTab';

        iconType?: string; // ['warning'|'notice'|'question']
        static defaultIcon = 'notice';

        links?: Array<any>;
        detail?: any;

        // handle(): void
        // {
        // }
    }

    export class WindowCloseRequest extends Request
    {
        static type = 'Window.Close';
        item: string;
    }

    export class WindowSetVisibilityRequest extends Request
    {
        static type = 'Window.SetVisibility';
        item: string;
        visible: boolean;
    }

    export class WindowPositionRequest extends Request
    {
        static type = 'Window.Position';
        item: string;
        width: number;
        height: number;
        left: number;
        bottom: number;
    }

    export class BackpackSetVisibilityRequest extends Request
    {
        static type = 'Backpack.SetVisibility';
        visible: boolean;
    }

    export class ScreenContentMessageRequest extends Request
    {
        static type = 'Screen.ContentMessage';
        item: string;
        message: any;
    }

    export class ItemGetPropertiesRequest extends Request
    {
        static type = 'Item.GetProperties';
        item: string;
    }

    export class ItemSetPropertyRequest extends Request
    {
        static type = 'Item.SetProperty';
        item: string;
        pid: string;
        value: any;
    }

    export class ItemSetStateRequest extends Request
    {
        static type = 'Item.SetState';
        item: string;
        state: string;
    }

    export class ItemSetConditionRequest extends Request
    {
        static type = 'Item.SetCondition';
        item: string;
        condition: string;
    }

    export class ItemActionRequest extends Request
    {
        static type = 'ItemAction';
        item: string;
        room: string;
        action: string;
        args: any;
    }

    export class ItemActionResponse extends Response
    {
        created: { [id: string]: { [pid: string]: string } };
        changed: { [id: string]: { [pid: string]: string } };
        deleted: string[];
    }

    export class RoomGetParticipantsRequest extends Request
    {
        static type = 'Room.GetParticipants';
        item: string;
        room: string;
    }

    export class RoomGetInfoRequest extends Request
    {
        static type = 'Room.GetInfo';
        item: string;
        room: string;
    }

    export class ParticipantData 
    {
        id: string;
        nickname: string;
        x: number;
        isSelf: boolean;
    }

    export class RoomInfo 
    {
        jid: string;
        destination: string;
    }
}
