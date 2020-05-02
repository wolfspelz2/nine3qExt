import { expect } from 'chai';
import { Utils } from '../lib/Utils';
import { xml } from '@xmpp/client';
import { Room } from '../contentscript/Room';

export class TestMisc
{
    Map_delete()
    {
        let m: Map<string, number> = new Map<string, number>();
        m['a'] = 'b';
        m.delete('a');
        expect(m.size).to.equal(0);
    }
}