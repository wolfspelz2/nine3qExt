import './test.css';
import { sut } from '../lib/sut';
import { sutGui } from '../lib/sutGui';
var s = new sut();

import { TestHelloWorld } from './TestHelloWorld'; s.addTestClass(TestHelloWorld);
import { TestAnimationsXml } from './TestAnimationsXml'; s.addTestClass(TestAnimationsXml);
import { TestApp } from './TestApp'; s.addTestClass(TestApp);

s.ignoreFailureForClass(TestHelloWorld);
s.run();
new sutGui().render(s, document.getElementsByTagName('body')[0]);