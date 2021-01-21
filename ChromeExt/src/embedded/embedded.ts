import log = require('loglevel');
import { Environment } from '../lib/Environment';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { ContentMessage } from '../lib/ContentMessage';
import { BackgroundApp } from '../background/BackgroundApp';
import { ContentApp, ContentAppNotification } from '../contentscript/ContentApp';
import '../contentscript/contentscript.scss';
import * as $ from 'jquery';
import { Panic } from '../lib/Panic';
import { Config } from '../lib/Config';

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

window.addEventListener('message', (event) =>
{
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

var appContent = null;
let onTabChangeStay = false;

try {

    function activateContent()
    {
        if (appContent == null) {
            log.debug('Contentscript.activate');
            appContent = new ContentApp($('body').get(0), msg =>
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
                        restartContent();
                    } break;
                }
            });
            ContentMessage.content = appContent;
            appContent.start();
        }
    }

    function deactivateContent()
    {
        if (appContent != null) {
            log.debug('Contentscript.deactivate');
            appContent.stop();
            appContent = null;
        }
    }

    function restartContent()
    {
        setTimeout(restart_deactivateContent, 100);
    }

    function restart_deactivateContent()
    {
        deactivateContent();
        setTimeout(restart_activateContent, 100);
    }

    function restart_activateContent()
    {
        activateContent();
    }

    function onUnloadContent()
    {
        if (appContent != null) {
            log.debug('Contentscript.onUnload');
            appContent.onUnload();
            appContent = null;
        }
    }

    Panic.onNow(onUnloadContent);

    window.addEventListener('onbeforeunload', deactivateContent);

    window.addEventListener('visibilitychange', function ()
    {
        if (document.visibilityState === 'visible') {
            activateContent();
        } else {
            if (onTabChangeStay) {
                log.debug('staying');
            } else {
                deactivateContent();
            }
        }
    });

    if (document.visibilityState === 'visible') {
        activateContent();
    }

} catch (error) {
    log.info(error);
}
