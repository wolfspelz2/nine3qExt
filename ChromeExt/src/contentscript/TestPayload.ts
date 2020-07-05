import { expect } from 'chai';
import log = require('loglevel');
import { Payload } from '../lib/Payload';
import { SimpleRpc } from '../lib/SimpleRpc';
import { parseJSON } from 'jquery';

export class TestPayload
{
    async Payload_getToken()
    {
        let token = await Payload.getToken(
            'http://localhost:5000/rpc',
            'user1',
            'item1',
            3600,
            {
                room: '9ca05afb1a49f26fb59642305c481661f8b370bd@muc4.virtual-presence.org',
            }
        );
        log.info('TEST', 'Payload_getToken', token);
        var record = JSON.parse(token);
        expect(record.payload.user).to.equal('user1');
        expect(record.payload.item).to.equal('item1');
        expect(record.payload.room).to.equal('9ca05afb1a49f26fb59642305c481661f8b370bd@muc4.virtual-presence.org');
    }
}
