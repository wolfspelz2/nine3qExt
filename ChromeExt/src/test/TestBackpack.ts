import { expect } from 'chai';
import { xml, jid } from '@xmpp/client';
import { BackgroundApp } from '../background/BackgroundApp';
import { Backpack } from '../background/Backpack';
import { as } from '../lib/as';
import { Pid } from '../lib/ItemProperties';
import { ItemChangeOptions } from '../lib/ItemChangeOptions';

export class TestBackpack
{
    Projector_stanzaOutFilter()
    {
        let ba = new BackgroundApp();
        let rep = new Backpack(ba);

        rep.addItem('item1', { 'a': 'b1', 'c': '41' }, { skipPersistentStorage: true });
        rep.addItem('item2', { 'a': 'b2', 'c': '42' }, { skipPersistentStorage: true });
        rep.addItem('item3', { 'a': 'b3', 'c': '43' }, { skipPersistentStorage: true });

        rep.rezItem('item1', 'room1@server', 41, 'd1', { skipPersistentStorage: true });
        rep.rezItem('item2', 'room1@server', 42, 'd2', { skipPersistentStorage: true });

        expect(rep.getItems()['item1'][Pid.IsRezzed]).to.equal('true');
        expect(rep.getItems()['item2'][Pid.IsRezzed]).to.equal('true');
        expect(as.Bool(rep.getItems()['item3'][Pid.IsRezzed], false)).to.equal(false);

        expect(rep.getItems()['item1'][Pid.RezzedX]).to.equal('41');
        expect(rep.getItems()['item2'][Pid.RezzedX]).to.equal('42');
        expect(as.Int(rep.getItems()['item3'][Pid.RezzedX], -1)).to.equal(-1);

        expect(rep.getItems()['item1'][Pid.RezzedLocation]).to.equal('room1@server');
        expect(rep.getItems()['item2'][Pid.RezzedLocation]).to.equal('room1@server');
        expect(as.String(rep.getItems()['item3'][Pid.RezzedLocation], '')).to.equal('');

        expect(rep.getItems()['item1'][Pid.RezzedDestination]).to.equal('d1');
        expect(rep.getItems()['item2'][Pid.RezzedDestination]).to.equal('d2');
        expect(as.String(rep.getItems()['item3'][Pid.RezzedDestination], '')).to.equal('');

        let stanza = xml('presence', { 'to': 'room1@server/nick' });
        stanza = rep.stanzaOutFilter(stanza);
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
