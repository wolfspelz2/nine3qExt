export namespace WeblinClientApi
{
    export class Message { constructor(public type: string) { } }
    export class Request extends Message { constructor(type: string, public id: string) { super(type); } }
    export class Response extends Message { id: string; constructor(type: string, public ok: boolean) { super(type); } }
    export class ContentResponse extends Response { ok = true; constructor(type: string) { super(type, true); } }
    export class SuccessResponse extends Response { constructor() { super('Message.Success', true); } }
    export class ErrorResponse extends Response { constructor(public error: any) { super('Message.Error', false); } }

    export class ClientNotificationRequest extends Request
    {
        static type = 'Client.Notification';

        title: string;
        text: string;

        target?: string; // ['currentTab'|'notCurrentTab'|'activeTab'|'allTabs']
        static defaultTarget = 'currentTab';

        iconType?: string; // ['warning'|'notice'|'question']
        static defaultIcon = 'notice';

        links?: Array<any>;
        detail?: any;
    }
}
