import './test.css';
const $ = require('jquery');
import { sut } from '../lib/sut';
import { sutGui } from '../lib/sutGui';
var s = new sut();

import { TestHelloWorld } from './TestHelloWorld'; s.addTestClass(TestHelloWorld);

s.ignoreFailureForClass(TestHelloWorld);
s.run();
new sutGui().render(s, $('body'));