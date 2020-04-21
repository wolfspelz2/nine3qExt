import './test.css';
const $ = require('jquery');
import { sut } from './sut';
import { sutAwsome } from './sutAwsome';
import { TestHelloWorld } from './TestHelloWorld';

var s = new sut();

s.addTestClass(TestHelloWorld);

s.ignoreFailureForClass(TestHelloWorld);
s.run();
new sutAwsome().render(s, $('body'));