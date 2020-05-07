import { expect } from 'chai';
var markdown = require('markdown').markdown;

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
        let md = markdown.toHTML('Hello **World**');
        expect(md).to.equal('<p>Hello <strong>World</strong></p>');
    }
}