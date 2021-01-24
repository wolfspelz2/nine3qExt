import log = require('loglevel');
import { RpcProtocol } from './RpcProtocol';

export class RpcClient
{
    call(url: string, request: RpcProtocol.BackpackActionRequest): Promise<RpcProtocol.Response>
    {
        log.debug(RpcClient.name, RpcClient.call.name, url, request);
        return new Promise((resolve, reject) =>
        {
            try {
                fetch(url, {
                    method: 'POST',
                    cache: 'reload',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(request),
                    redirect: 'error'
                })
                    .then(httpResponse =>
                    {
                        log.debug(RpcClient.name + '.call', 'httpResponse', url, request, httpResponse);
                        if (httpResponse.ok) {
                            return httpResponse.text();
                        } else {
                            reject(httpResponse);
                        }
                    })
                    .then(text =>
                    {
                        let response = JSON.parse(text);
                        log.debug('BackgroundApp.handle_jsonRpc', 'response', url, response);
                        if (response.status == RpcProtocol.Response.status_ok) {
                            resolve(response);
                        } else {
                            reject(response);
                        }
                    })
                    .catch(ex =>
                    {
                        log.debug(RpcClient.name, RpcClient.call.name, 'catch', url, ex);
                        reject(ex);
                    });
            } catch (ex) {
                reject(ex);
            }
        });
    }
}
