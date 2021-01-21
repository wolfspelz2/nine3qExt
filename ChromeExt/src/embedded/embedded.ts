import log = require('loglevel');
import { Environment } from '../lib/Environment';
import { BackgroundApp } from '../background/BackgroundApp';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import '../contentscript/contentscript.scss';
import * as $ from 'jquery';
import { Panic } from '../lib/Panic';
import { Config } from '../lib/Config';
import { ContentApp, ContentAppNotification } from '../contentscript/ContentApp';
import {ContentMessage} from "../lib/ContentMessage";


console.log('Background');

let debug = Environment.isDevelopment();
console.log('Background', 'debug', debug);

log.setLevel(log.levels.INFO);

if (debug) {
    log.setLevel(log.levels.DEBUG);
    // log.setLevel(log.levels.TRACE);
}

var app = null;

async function activate()
{
    log.debug('Background.activate');
    if (app == null) {
        app = new BackgroundApp();
        BackgroundMessage.background = app;

        try {
            await app.start();
        }
        catch (error) {
            app = null;
        }
    }
}

function deactivate()
{
    if (app != null) {
        app.stop();
        app = null;
    }
}

window.addEventListener("message", (event) => {
    if (event.data.type === BackgroundMessage.userSettingsChanged.name) {
        if (app) {
            app.handle_userSettingsChanged();
        }
    }
}, false);


activate();


// contentscript


let dev = Environment.isDevelopment();
log.setLevel(log.levels.INFO);

if (dev) {
    console.log('Contentscript', 'dev', dev);
    // Config.getAllStatic().vp.deferPageEnterSec = 0;
    log.setLevel(log.levels.DEBUG);
}

var app2 = null;
let onTabChangeStay = false;

try {

    function activate2()
    {
        if (app2 == null) {
            log.debug('Contentscript.activate');
            app2 = new ContentApp($('body').get(0), msg =>
            {
                log.debug('Contentscript msg', msg.type);
                switch (msg.type) {
                    case ContentAppNotification.type_onTabChangeStay: {
                        onTabChangeStay = true;
                    } break;

                    case ContentAppNotification.type_onTabChangeLeave: {
                        onTabChangeStay = false;
                    } break;

                    case ContentAppNotification.type_stopped: {
                    } break;

                    case ContentAppNotification.type_restart: {
                        restart2();
                    } break;
                }
            });
            ContentMessage.content = app2;
            app2.start();
        }
    }

    function deactivate2()
    {
        if (app2 != null) {
            log.debug('Contentscript.deactivate');
            app2.stop();
            app2 = null;
        }
    }

    function restart2()
    {
        setTimeout(restart_deactivate2, 100);
    }

    function restart_deactivate2()
    {
        deactivate2();
        setTimeout(restart_activate2, 100);
    }

    function restart_activate2()
    {
        activate2();
    }

    function onUnload2()
    {
        if (app2 != null) {
            log.debug('Contentscript.onUnload');
            app2.onUnload();
            app2 = null;
        }
    }

    Panic.onNow(onUnload2);

    window.addEventListener('onbeforeunload', deactivate2);

    window.addEventListener('visibilitychange', function ()
    {
        if (document.visibilityState === 'visible') {
            activate2();
        } else {
            if (onTabChangeStay) {
                log.debug('staying');
            } else {
                deactivate2();
            }
        }
    });

    if (document.visibilityState === 'visible') {
        activate2();
    }

} catch (error) {
    log.info(error);
}
