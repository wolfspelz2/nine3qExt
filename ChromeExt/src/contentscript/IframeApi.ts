import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';

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
                this.handle_ItemActionRequest(<WeblinClientApi.ItemActionRequest>request);
            } break;
        }
    }

    handle_ItemActionRequest(request: WeblinClientApi.ItemActionRequest)
    {
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
