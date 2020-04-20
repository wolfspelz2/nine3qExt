import { expect } from 'chai';
import 'mocha';

import { HelloWorld } from '../src/contentscript/HelloWorld';

describe('Hello function', () =>
{

    it('should return Hello World', () =>
    {
        // const result = 'Hello World';
        const result = HelloWorld.getText();
        expect(result).to.equal('Hello World');
    });

});
