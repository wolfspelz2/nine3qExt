import log = require('loglevel');
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Config } from '../lib/Config';
import { ItemException } from '../lib/ItemExcption';
import { ContentApp } from './ContentApp';
import { SimpleErrorToast } from './Toast';

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
        log.debug(IframeApi.name, this.onMessage.name, ev);

        let request = <WeblinClientApi.Request>ev.data;

        switch (request.type) {
            case WeblinClientApi.ItemActionRequest.type: {
                /* await */ this.handle_ItemActionRequest(<WeblinClientApi.ItemActionRequest>request);
            } break;
        }
    }

    async handle_ItemActionRequest(request: WeblinClientApi.ItemActionRequest)
    {
        try {
            let itemId = request.item;
            let actionName = request.action;
            let args = request.args;
            await BackgroundMessage.executeBackpackItemAction(itemId, actionName, args, [itemId]);
        } catch (ex) {
            // if (ex instanceof ItemException) {
            //     new ItemExceptionToast(this.app, Config.get('room.applyItemErrorToastDurationSec', 5), ex).show();
            // } else {
            //     new SimpleErrorToast(this.app, 'Warning-' + ex.fact + '-' + ex.reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', ex.fact, ex.reason, ex.detail).show();
            // }
            let fact = typeof ex.fact === 'number' ? ItemException.Fact[ex.fact]: ex.fact;
            let reason = typeof ex.reason === 'number' ? ItemException.Reason[ex.reason]: ex.reason;
            let detail = ex.detail;
            new SimpleErrorToast(this.app, 'Warning-' + fact + '-' + reason, Config.get('room.applyItemErrorToastDurationSec', 5), 'warning', fact, reason, detail).show();
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