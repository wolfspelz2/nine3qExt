import log = require('loglevel');
import './contentscript.scss';
import * as $ from 'jquery';
import { Panic } from '../lib/Panic';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { ContentApp, ContentAppNotification } from './ContentApp';

let dev = Environment.isDevelopment();
console.log('Contentscript', 'dev', dev);

log.setLevel(log.levels.INFO);

if (dev) {
    // Config.getAllStatic().vp.deferPageEnterSec = 0;
    log.setLevel(log.levels.DEBUG);
}

var app = null;
let onTabChangeStay = false;

try {

    function activate()
    {
        if (app == null) {
            log.debug('Contentscript.activate');
            app = new ContentApp($('body').get(0), msg =>
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
                        restart();
                    } break;
                }
            });
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

    function restart()
    {
        setTimeout(restart_deactivate, 100);
    }

    function restart_deactivate()
    {
        deactivate();
        setTimeout(restart_activate, 100);
    }

    function restart_activate()
    {
        activate();
    }

    function onUnload()
    {
        if (app != null) {
            log.debug('Contentscript.onUnload');
            app.onUnload();
            app = null;
        }
    }

    Panic.onNow(onUnload);

    window.addEventListener('onbeforeunload', deactivate);

    window.addEventListener('visibilitychange', function ()
    {
        if (document.visibilityState === 'visible') {
            activate();
        } else {
            if (onTabChangeStay) {
                log.debug('staying');
            } else {
                deactivate();
            }
        }
    });

    if (document.visibilityState === 'visible') {
        activate();
    }

} catch (error) {
    log.info(error);
}
