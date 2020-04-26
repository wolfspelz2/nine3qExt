import './contentscript.scss';
const $ = require('jquery');
import { App } from './App';

const isContentscript: boolean = true;
console.log('Contentscript', isContentscript);

var pageElem: HTMLElement = $('<div class="n3q-div" id="n3q-id-page" />')[0];
$('body').append(pageElem);
new App(pageElem).start();

// iframe test
// let html = '<html><head><title></title><style>.n3q-img{pointer-events:auto;}</style></head><body><p>This text will appear in the iframe!</p> <img class=\'n3q-img\' src=\'https://www.galactic-developments.de/images/post/Vegas-fbpost.jpg\'/></body></html>';
// var e: HTMLElement = $('<iframe id="n3q-frame" frameborder="0" srcdoc="' + html + '"></iframe>')[0];
// $('body').append(e);

