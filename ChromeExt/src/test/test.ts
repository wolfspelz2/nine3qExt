import './test.scss';
import { sut } from '../lib/sut';
import { sutGui } from '../lib/sutGui';
var s = new sut();

import { TestHelloWorld } from './TestHelloWorld'; s.addTestClass(TestHelloWorld);
import { TestAnimationsXml } from './TestAnimationsXml'; s.addTestClass(TestAnimationsXml);
import { TestContentApp } from './TestContentApp'; s.addTestClass(TestContentApp);
import { TestUtils } from './TestUtils'; s.addTestClass(TestUtils);
import { TestConfig} from './TestConfig'; s.addTestClass(TestConfig);
import { TestTranslator} from './TestTranslator'; s.addTestClass(TestTranslator);
import { TestMisc} from './TestMisc'; s.addTestClass(TestMisc);

s.ignoreFailureForClass(TestHelloWorld);
s.run();
new sutGui().render(s, document.getElementsByTagName('body').get(0));