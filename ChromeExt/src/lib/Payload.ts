import log = require('loglevel');
import { BackgroundMessage } from './BackgroundMessage';
import { Utils } from './Utils';

export class Payload
{
    static async getHash(providerId: string, user: string, payload: any): Promise<string>
    {
        var payloadJson = JSON.stringify(payload);
        var payloadJsonBase64 = atob(payloadJson);
        var url = 'http://localhost:5000/PayloadHash?user={user}&payload={payload}';
        url = url
            .replace('{user}', encodeURIComponent(user))
            .replace('{payload}', encodeURIComponent(payloadJsonBase64))
            ;
        try {
            var response = await BackgroundMessage.fetchUrl(url, BackgroundMessage.fetchUrl_nocache);
            if (response.ok) {
                return response.data;
            }
        } catch (error) {

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
