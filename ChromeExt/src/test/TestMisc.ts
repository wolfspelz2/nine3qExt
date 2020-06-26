import { expect } from 'chai';
// import markdown = require('markdown');

export class TestMisc
{
    Map_delete()
    {
        let m: Map<string, number> = new Map<string, number>();
        m['a'] = 'b';
        m.delete('a');
        expect(m.size).to.equal(0);
    }

    Map_url_regex()
    {
        expect('x http://www.weblin.com y'.replace(/(https?:\/\/[^\s]+)/g, url => 'url')).to.equal('x url y');
        expect('http://www.weblin.com'.replace(/(https?:\/\/[^\s]+)/g, url => 'url')).to.equal('url');
        expect('x www.weblin.com y'.replace(/(www\.[^. ]+\.[^ ]+)/g, url => 'url')).to.equal('x url y');
        expect('www.a.b'.replace(/(www\.[^. ]+\.[^ ]+)/g, url => 'url')).to.equal('url');

        expect('x http://www.weblin.com www.weblin.com y'.replace(/(https?:\/\/[^\s]+|www\.[^. ]+\.[^ ]+)/g, url => 'url')).to.equal('x url url y');
        expect('http://www.weblin.com'.replace(/(https?:\/\/[^\s]+|www\.[^. ]+\.[^ ]+)/g, url => 'url')).to.equal('url');
        expect('www.weblin.com'.replace(/(https?:\/\/[^\s]+|www\.[^. ]+\.[^ ]+)/g, url => 'url')).to.equal('url');
        expect('weblin.com'.replace(/(https?:\/\/[^\s]+|www\.[^. ]+\.[^ ]+|[^. ]+\.(com|org|net|[a-z]{2}))/g, url => 'url')).to.equal('url');
        expect('x www.heise.de y'.replace(/(https?:\/\/[^\s]+|www\.[^. ]+\.[^ ]+|[^. ]+\.(com|org|net|[a-z]{2}))/g, url => 'url')).to.equal('x url y');
    }

    // markdown()
    // {
    //     let md = markdown.markdown.toHTML('Hello **World**');
    //     expect(md).to.equal('<p>Hello <strong>World</strong></p>');
    // }
}