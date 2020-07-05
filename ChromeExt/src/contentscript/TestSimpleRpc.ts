import { expect } from 'chai';
import log = require('loglevel');
import { Payload } from '../lib/Payload';
import { SimpleRpc } from '../lib/SimpleRpc';
import { parseJSON } from 'jquery';

export class TestSimpleRpc
{
    async Payload_getToken()
    {
        let response = await new SimpleRpc('Echo')
            .param('aString', 'Hello World')
            .param('aNumber', 3.14159265358979323)
            .param('aBool', true)
            .param('aLong', 42000000000)
            .param('aDate', new Date(Date.now()).toISOString())
            .send('http://localhost:5000/rpc');
        if (response.ok) {
            log.debug('TEST', response.data);
        } else {
            log.debug('TEST', response.message);
        }
        expect(response.get('aString', null)).to.equal('Hello World');
        expect(response.get('aNumber', null)).to.equal(3.14159265358979323);
        expect(response.get('aBool', null)).to.equal(true);
        expect(response.get('aLong', null)).to.equal(42000000000);
    }
}
