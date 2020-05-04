import './popup.scss';
import * as $ from 'jquery';
import log = require('loglevel');
import { PopupApp } from './PopupApp';

console.log('Popup');

let debug = true;
log.setLevel(log.levels.INFO);

if (debug) {
    log.setLevel(log.levels.DEBUG);
}

var app = null;

function activate()
{
    if (app == null) {
        app = new PopupApp($('body').get(0));
        // app.start();
        app.dev_start();
    }
}

function deactivate()
{
    if (app != null) {
        app.stop();
        app = null;
    }
}

activate();
