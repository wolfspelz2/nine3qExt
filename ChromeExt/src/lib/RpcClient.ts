import log = require('loglevel');
import { RpcProtocol } from './RpcProtocol';

export class RpcClient
{
    async call(url: string, request: RpcProtocol.BackpackTransactionRequest): Promise<RpcProtocol.Response>
    {
        // try {
        //     fetch(url, {
        //         method: 'POST',
        //         cache: 'reload',
        //         headers: { 'Content-Type': 'application/json' },
        //         body: JSON.stringify(request),
        //         redirect: 'error'
        //     })
        //         .then(httpResponse =>
        //         {
        //             log.debug(RpcClient.name + '.call', 'httpResponse', url, request, httpResponse);
        //             if (httpResponse.ok) {
        //                 return httpResponse.text();
        //             } else {
        //                 throw { 'ok': false, 'status': httpResponse.status, 'statusText': httpResponse.statusText };
        //             }
        //         })
        //         .then(text =>
        //         {
        //             let response = { 'ok': true, 'data': text };
        //             log.debug('BackgroundApp.handle_jsonRpc', 'response', url, text.length, response);
        //             sendResponse(response);
        //         })
        //         .catch(ex =>
        //         {
        //             log.debug('BackgroundApp.handle_jsonRpc', 'catch', url, ex);
        //             let status = ex.status;
        //             if (!status) { status = ex.name; }
        //             if (!status) { status = 'Error'; }
        //             let statusText = ex.statusText;
        //             if (!statusText) { status = ex.message; }
        //             if (!statusText) { status = ex; }
        //             sendResponse({ 'ok': false, 'status': status, 'statusText': statusText });
        //         });
        //     return true;
        // } catch (error) {
        //     log.debug('BackgroundApp.handle_jsonRpc', 'exception', url, error);
        //     sendResponse({ 'ok': false, 'status': error.status, 'statusText': error.statusText });
        // }
        // return false;

        return new RpcProtocol.Response();
    }
}
