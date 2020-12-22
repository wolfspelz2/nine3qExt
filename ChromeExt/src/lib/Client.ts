import { Config } from './Config';
import { _Changes } from './_Changes';

export class Client
{
    static getDetails(): string
    {
        let d = {
            'client': Config.get('client.name', 'weblin.io'),
            'clientVariant': Config.get('client.variant', ''),
            'clientVersion': _Changes.data[0][0],
            'design': Config.get('design.name', ''),
            'designVersion': Config.get('design.version', ''),
        };
        // let s = '';
        // Object.entries(d).forEach(
        //     ([key, value]) =>
        //     {
        //         if (value != '') {
        //             s += key + '=' + value + '; ';
        //         }
        //     }
        // );
        let s = JSON.stringify(d);
        return s;
    }
}
