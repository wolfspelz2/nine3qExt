import log = require('loglevel');
import { Utils } from './Utils';
import { SimpleRpc } from './SimpleRpc';

export class Payload
{
    static async getToken(api: string, user: string, item: string, ttlSec: number, params: any): Promise<string>
    {
        let expires = 10000000000 + ttlSec;
        var payload = {
            'api': api,
            'user': user,
            'item': item,
            'entropy': Utils.randomString(20),
            'expires': expires
        };
        for (let key in params) {
            payload[key] = params[key];
        }
        let payloadString = JSON.stringify(payload);
        let hash = await this.getHash(api, user, payloadString);
        let token = {
            'payload': payload,
            'hash': hash
        }
        let tokenString = JSON.stringify(token);
        let tokenBase64Encoded = Utils.base64Encode(tokenString);
        return tokenBase64Encoded;
    }

    static async getHash(api: string, user: string, payload: string): Promise<string>
    {
        let payloadBase64 = Utils.base64Encode(payload);
        let response = await new SimpleRpc('ComputePayloadHash')
            .param('user', user)
            .param('payload', payloadBase64)
            .send(api);
        if (response.ok) {
            return response.get('result', null);
        }
        throw response.message;
    }

}
