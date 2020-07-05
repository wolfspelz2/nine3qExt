import log = require('loglevel');
import { BackgroundMessage } from './BackgroundMessage';
import { Utils } from './Utils';
import { parseJSON } from 'jquery';

export class SimpleRpcResponse
{
    constructor(public ok: boolean, public data: any, public message: string = null)
    {
    }

    get(key: string, defaultValue: any): any
    {
        if (this.data[key]) {
            return this.data[key];
        }
        return defaultValue;
    }
}

export class SimpleRpc
{
    constructor(public rpcMethod: string, public params: any = {})
    {
    }

    method(method: string): SimpleRpc { this.rpcMethod = method; return this; }
    param(key: string, value: any): SimpleRpc { this.params[key] = value; return this; }

    async send(url: string): Promise<SimpleRpcResponse>
    {
        this.params['method'] = this.rpcMethod;
        try {
            var response = await BackgroundMessage.jsonRpc(url, this.params);
            if (response.ok) {
                let data = parseJSON(response.data);
                if (data.status == 'ok') {
                    return new SimpleRpcResponse(true, data)
                } else {
                    return new SimpleRpcResponse(false, {}, data.message);
                }
            } else {
                return new SimpleRpcResponse(false, {}, response.status + ' ' + response.statusText);
            }
        } catch (error) {
            return new SimpleRpcResponse(false, {}, JSON.stringify(error));
        }
        return new SimpleRpcResponse(false, {}, 'unknown error');
    }
}
