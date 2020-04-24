import { expect } from 'chai';
import { App } from '../contentscript/App';
import { sut } from '../lib/sut';

export class TestApp
{
    getRoomJidFromLocationUrl()
    {
        const jid = App.getRoomJidFromLocationUrl('xmpp:d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
        expect(jid).to.equal('d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
    }
}