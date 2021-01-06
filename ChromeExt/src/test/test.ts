import './test.scss';
import { sut } from '../lib/sut';
import { sutGui } from '../lib/sutGui';
var s = new sut();

import { TestHelloWorld } from './TestHelloWorld'; s.addTestClass(TestHelloWorld); s.ignoreFailureForClass(TestHelloWorld);
import { TestAnimationsXml } from './TestAnimationsXml'; s.addTestClass(TestAnimationsXml);
import { TestContentApp } from './TestContentApp'; s.addTestClass(TestContentApp);
import { TestUtils } from './TestUtils'; s.addTestClass(TestUtils);
import { TestConfig } from './TestConfig'; s.addTestClass(TestConfig);
import { TestTranslator } from './TestTranslator'; s.addTestClass(TestTranslator);
import { TestMisc } from './TestMisc'; s.addTestClass(TestMisc);
import { TestVpiResolver } from './TestVpiResolver'; s.addTestClass(TestVpiResolver);
import { TestItemProvider } from './TestItemProvider'; s.addTestClass(TestItemProvider);
import { TestAs } from './TestAs'; s.addTestClass(TestAs);
import { TestProjector } from './TestProjector'; s.addTestClass(TestProjector);

s.run().then(() =>
{
    new sutGui().render(s, document.getElementsByTagName('body')[0]);
});
