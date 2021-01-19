import { ItemProperties } from './ItemProperties';

export class RpcProtocol
{
}

export namespace RpcProtocol
{
    export class Request
    {
        method: string;
    }

    export class Response
    {
        status: string;
        static status_ok = 'ok';
        static status_error = 'error';
        result: string;
        message: string;
    }

    export class BackpackRequest extends Request
    {
    }

    export class BackpackResponse extends Response
    {
    }

    export class BackpackTransactionRequest extends BackpackRequest
    {
        static method = 'ItemTransaction';
        user: string;
        item: string;
        room: string;
        action: string;
        args: any;
        items: { [id: string]: ItemProperties };
    }

    export class BackpackTransactionResponse extends BackpackResponse
    {
        created: { [id: string]: ItemProperties };
        changed: { [id: string]: ItemProperties };
        deleted: string[];
    }
}
