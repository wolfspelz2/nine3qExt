import { expect } from 'chai';
import { SimpleRpc } from '../lib/SimpleRpc';
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

    // async SimpleRpc_echo()
    // {
    //     let result: any;
    //     try {
    //         let response = await new SimpleRpc('echo')
    //             .param('aString', 'Hello World')
    //             .param('aNumber', 3.14159265358979323)
    //             .param('aBool', true)
    //             .param('aLong', 42000000000)
    //             .param('aDate', new Date(Date.now()).toISOString())
    //             .send('http://localhost:5000/rpc');
    //         if (response.ok) {
    //             result = response.data;
    //         }
    //     } catch (error) {
    //         //
    //     }

    //     expect(result.aString).to.equal('Hello World');
    //     expect(result.aNumber).to.equal(3.14159265358979323);
    // }

    // markdown()
    // {
    //     let md = markdown.markdown.toHTML('Hello **World**');
    //     expect(md).to.equal('<p>Hello <strong>World</strong></p>');
    // }
}
