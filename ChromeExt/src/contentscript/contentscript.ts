import './contentscript.scss';
const $ = require('jquery');
import log = require('loglevel');
import { ContentApp } from './ContentApp';
import { Unbearable } from '../lib/Unbearable';

const isContentscript: boolean = true;
console.log('Contentscript', isContentscript);

log.setLevel(log.levels.DEBUG);
var app = null;

function activate()
{
    if (app == null) {
        app = new ContentApp($('body'));
        app.start();
    }
}

function deactivate()
{
    if (app != null) {
        app.stop();
        app = null;
    }
}

Unbearable.onProblem(deactivate);

window.addEventListener('onbeforeunload', deactivate);

window.addEventListener('visibilitychange', function ()
{
    if (document.visibilityState === 'visible') {
        activate();
    } else {
        deactivate();
    }
});

if (document.visibilityState === 'visible') {
    activate();
}

// iframe test
// let html = '<html><head><title></title><style>.n3q-img{pointer-events:auto;}</style></head><body><p>This text will appear in the iframe!</p> <img class=\'n3q-img\' src=\'https://www.galactic-developments.de/images/post/Vegas-fbpost.jpg\'/></body></html>';
// var e: HTMLElement = $('<iframe id="n3q-frame" frameborder="0" srcdoc="' + html + '"></iframe>')[0];
// $('body').append(e);

