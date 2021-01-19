import log = require('loglevel');
import { Utils } from './Utils';
import { SimpleRpc } from '../contentscript/SimpleRpc';

export class Payload
{
    static async getContextToken(api: string, user: string, item: string, ttlSec: number, params: any): Promise<string>
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
        let hash = await this.getPayloadHash(api, payload);
        let token = {
            'api': api,
            'payload': payload,
            'hash': hash
        }
        let tokenString = JSON.stringify(token);
        let tokenBase64Encoded = Utils.base64Encode(tokenString);
        return tokenBase64Encoded;
    }

    static async getPayloadHash(api: string, payload: any): Promise<string>
    {
        let response = await new SimpleRpc('getPayloadHash')
            .param('payload', payload)
            .send(api);
        if (response.ok) {
            return response.get('result', null);
        }
        throw response.message;
    }

}
