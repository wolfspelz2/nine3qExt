import { expect } from 'chai';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Projector } from '../background/Projector';
import { BackgroundApp } from '../background/BackgroundApp';

export class TestProjector
{
    Projector_stanzaOutFilter()
    {
        let ba = new BackgroundApp();
        let p = new Projector(ba);
        p.projectItem('room1@server', 'item1', { 'a': 'b1', 'c': '41' });
        p.projectItem('room1@server', 'item2', { 'a': 'b2', 'c': '42' });
        let stanza = xml('presence', { 'to': 'room1@server/nick' });
        stanza = p.stanzaOutFilter(stanza);
        expect(stanza.name).to.equal('presence');
        expect(stanza.attrs.to).to.equal('room1@server/nick');
        let vpDependent = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:dependent');
        let dependentPresences = vpDependent.getChildren('presence');
        expect(dependentPresences.length).to.equal(2);
        let pres1 = dependentPresences[0].getChildren('x')[0];
        expect(pres1.attrs['xmlns']).to.equal('vp:props');
        expect(pres1.attrs['type']).to.equal('item');
        expect(pres1.attrs['provider']).to.equal('nine3q');
        expect(pres1.attrs['a']).to.equal('b1');
        expect(pres1.attrs['c']).to.equal('41');
        let pres2 = dependentPresences[1].getChildren('x')[0];
        expect(pres2.attrs['xmlns']).to.equal('vp:props');
        expect(pres2.attrs['type']).to.equal('item');
        expect(pres2.attrs['provider']).to.equal('nine3q');
        expect(pres2.attrs['a']).to.equal('b2');
        expect(pres2.attrs['c']).to.equal('42');
    }
}
