import { expect } from 'chai';
import { HelloWorld } from '../contentscript/HelloWorld';
import { sut } from '../lib/sut';

// function sutMethod() {
//     return function (target: any, propertyKey: string, descriptor: PropertyDescriptor) {
//         console.log('sutMethod', target, propertyKey, descriptor);
//     };
// }

export class TestHelloWorld
{
    private aPrivateProperty: number = 42;

    private aPrivateMethod = function()
    {
       throw 'should not be called';
    }

    getText_fails()
    {
        const result = HelloWorld.getText();
        expect(result).to.equal('Expected Hello');
    }

    getText_fails_with_result()
    {
        const result = HelloWorld.getText();
        return 'error';
    }

    getText()
    {
        const result = HelloWorld.getText();
        expect(result).to.equal('Hello World');
    }
}