import log = require('loglevel');
import { Utils } from './Utils';
import { SimpleRpc } from './SimpleRpc';

export class Payload
{
    static async getToken(api: string, user: string, item: string, ttlSec: number, params: any): Promise<string>
    {
        let expires = 10000000000 + ttlSec;
        var payload = {
            'user': user,
            'item': item,
            'entropy': Utils.randomString(20),
            'expires': expires
        };
        for (let key in params) {
            payload[key] = params[key];
        }
        let hash = await this.getHash(api, payload);
        let token = {
            'api': api,
            'payload': payload,
            'hash': hash
        }
        let tokenString = JSON.stringify(token);
        let tokenBase64Encoded = Utils.base64Encode(tokenString);
        return tokenBase64Encoded;
    }

    static async getHash(api: string, payload: any): Promise<string>
    {
        let response = await new SimpleRpc('computePayloadHash')
            .param('payload', payload)
            .send(api);
        if (response.ok) {
            return response.get('result', null);
        }
        throw response.message;
    }

}
