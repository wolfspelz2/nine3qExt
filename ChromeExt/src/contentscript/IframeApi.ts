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
        log.debug('IframeApi.onMessage', ev);

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
            case WeblinClientApi.WindowPositionRequest.type: {
                this.handle_PositionWindowRequest(<WeblinClientApi.WindowPositionRequest>request);
            } break;
            case WeblinClientApi.ScreenContentMessageRequest.type: {
                this.handle_ScreenContentMessageRequest(<WeblinClientApi.ScreenContentMessageRequest>request);
            } break;
            case WeblinClientApi.WindowOpenDocumentUrlRequest.type: {
                this.handle_WindowOpenDocumentUrlRequest(<WeblinClientApi.WindowOpenDocumentUrlRequest>request);
            } break;
            case WeblinClientApi.WindowCloseRequest.type: {
                this.handle_CloseWindowRequest(<WeblinClientApi.WindowCloseRequest>request);
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
            this.app.closeItemFrame(request.item);
        } catch (ex) {
            log.info('IframeApi.handle_CloseWindowRequest', ex);
        }
    }

    handle_WindowOpenDocumentUrlRequest(request: WeblinClientApi.WindowOpenDocumentUrlRequest)
    {
        try {
            this.app.openDocumentUrl(request.item);
        } catch (ex) {
            log.info('IframeApi.handle_WindowOpenDocumentUrlRequest', ex);
        }
    }

    handle_PositionWindowRequest(request: WeblinClientApi.WindowPositionRequest)
    {
        try {
            this.app.positionItemFrame(request.item, request.width, request.height, request.left, request.bottom);
        } catch (ex) {
            log.info('IframeApi.handle_PositionWindowRequest', ex);
        }
    }

    handle_ScreenContentMessageRequest(request: WeblinClientApi.ScreenContentMessageRequest)
    {
        try {
            this.app.sendMessageToScreenItemFrame(request.item, request.message);
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
        id?: string;
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

    export class WindowCloseRequest extends Request
    {
        static type = 'Window.Close';
        item: string;
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

    export class ScreenContentMessageRequest extends Request
    {
        static type = 'Screen.ContentMessage';
        item: string;
        message: any;
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
}
