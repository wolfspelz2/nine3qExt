import { expect } from 'chai';
import markdown = require('markdown');

export class TestMisc
{
    Map_delete()
    {
        let m: Map<string, number> = new Map<string, number>();
        m['a'] = 'b';
        m.delete('a');
        expect(m.size).to.equal(0);
    }

    markdown()
    {
        let md = markdown.markdown.toHTML('Hello **World**');
        expect(md).to.equal('<p>Hello <strong>World</strong></p>');
    }
}