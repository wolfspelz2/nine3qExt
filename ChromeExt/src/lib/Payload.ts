import log = require('loglevel');
import { Utils } from './Utils';
import { SimpleRpc } from './SimpleRpc';

export class Payload
{
    static async getHash(providerId: string, user: string, payload: any): Promise<string>
    {
        let payloadJson = JSON.stringify(payload);
        let payloadJsonBase64 = atob(payloadJson);
        try {
            let response = await new SimpleRpc('computePayloadHash')
                .param('user', user)
                .param('payload', payloadJsonBase64)
                .send('http://localhost:5000/Client/Rpc');
            if (response.ok) {
                return response.data;
            }
        } catch (error) {
            //
        }
        return '';
    }

    static async getToken(providerId: string, user: string, item: string, ttlSec: number, params: any): Promise<string>
    {
        var token = '';
        let expires = 10000000000 + ttlSec;
        var payload = {
            'api': 'https://n3q-api.com/v1',
            'user': user,
            'item': item,
            'entropy': Utils.randomString(20),
            'expires': expires
        };
        for (let key in params) {
            payload[key] = params[key];
        }
        return token;
    }
}
