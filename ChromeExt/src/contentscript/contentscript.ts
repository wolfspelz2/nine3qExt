import log = require('loglevel');
import './contentscript.scss';
import * as $ from 'jquery';
import { Panic } from '../lib/Panic';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { ContentApp } from './ContentApp';

console.log('Contentscript');

let debug = Environment.isDevelopment();
console.log('Contentscript', 'debug', debug);

log.setLevel(log.levels.INFO);

if (debug) {
    // Config.getAllStatic().room.randomEnterPosXMin = 200;
    // Config.getAllStatic().room.randomEnterPosXMax= 200;
    // Config.getAllStatic().vp.deferPageEnterSec = 0;
    log.setLevel(log.levels.DEBUG);
}

var app = null;

try {

    function activate()
    {
        if (app == null) {
            log.debug('Contentscript.activate');
            app = new ContentApp($('body').get(0));
            app.start();
        }
    }

    function deactivate()
    {
        if (app != null) {
            log.debug('Contentscript.deactivate');
             app.stop();
            app = null;
        }
    }

    function kill()
    {
        if (app != null) {
            log.debug('Contentscript.kill');
            app.kill();
            app = null;
        }
    }

    Panic.onNow(kill);

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

} catch (error) {
    log.info(error);
}

// iframe test
// let html = '<html><head><title></title><style>.n3q-img{pointer-events:auto;}</style></head><body><p>This text will appear in the iframe!</p> <img class=\'n3q-img\' src=\'https://www.galactic-developments.de/images/post/Vegas-fbpost.jpg\'/></body></html>';
// var e: HTMLElement = $('<iframe id="n3q-frame" frameborder="0" srcdoc="' + html + '"></iframe>').get(0);
// $('body').append(e);

