import log = require('loglevel');
import './contentscript.scss';
import * as $ from 'jquery';
import { Panic } from '../lib/Panic';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { ContentApp, ContentAppNotification } from './ContentApp';
import { ContentMessage } from '../lib/ContentMessage';
import { BackgroundMessage } from '../lib/BackgroundMessage';

let debug = Environment.isDevelopment();
console.log('weblin.io Content', 'dev', debug);

log.setLevel(log.levels.INFO);

if (debug) {
    log.setLevel(log.levels.DEBUG);
    // log.setLevel(log.levels.TRACE);
}

try {

    var app = null;
    let onTabChangeStay = false;

    let runtimeMessageHandlerWhileDeactivated: (message: any, sender: any, sendResponse: any) => any;
    function onRuntimeMessage(message, sender, sendResponse): any
    {
        if (message.type == ContentMessage.type_extensionActiveChanged && message.data && message.data.state) {
            activate();
        }
        sendResponse();
        return false;
    }

    function activate()
    {
        if (app == null) {
            if (Environment.isExtension() && chrome.runtime.onMessage && runtimeMessageHandlerWhileDeactivated) {
                chrome.runtime.onMessage.removeListener(runtimeMessageHandlerWhileDeactivated);
            }

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
                        deactivate();
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

            if (Environment.isExtension() && chrome.runtime.onMessage) {
                runtimeMessageHandlerWhileDeactivated = (message, sender, sendResponse) => onRuntimeMessage(message, sender, sendResponse);
                chrome.runtime.onMessage.addListener(runtimeMessageHandlerWhileDeactivated);
            }
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

    window.addEventListener('unload', function ()
    {
        deactivate();
    });

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
