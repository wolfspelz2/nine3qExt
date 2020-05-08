import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { BackgroundApp } from './BackgroundApp';
import { BackgroundMessage } from '../lib/BackgroundMessage';

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
    if (app == null) {
        app = new BackgroundApp();

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

activate();

// chrome.runtime?.onMessage.addListener(
//     function (request, sender, sendResponse)
//     {
//         if (app != null) {
//             return app.runtimeOnMessage(request, sender, function (response)
//             {
//                 sendResponse(response);
//             });
//         }
//         return true;
//     }
// );

// chrome.runtime?.onInstalled.addListener(function (details)
// {
//     // details = {previousVersion: "1.0.0", reason: update" }
//     chrome.tabs.query({}, function (tabs)
//     {
//         tabs.forEach(tab =>
//         {
//             if (tab.url != undefined) {
//                 if (!tab.url.match(/(chrome):\/\//gi)) {
//                     chrome.tabs.sendMessage(tab.id, { 'type': 'backgroundInstalled' });
//                 }
//             }
//         });
//     });
// })

const httpCacheData: Map<string, string> = new Map<string, string>();
const httpCacheTime: Map<string, number> = new Map<string, number>();

chrome.runtime?.onMessage.addListener(
    function (message, sender, sendResponse)
    {
        switch (message.type) {
            case BackgroundMessage.type_fetchUrl: {
                var url = message.url;
                var version = message.version;

                let key = version + url;
                let isCached = (httpCacheData[key] != undefined);

                log.debug('background', message.type, 'cached=', isCached, url, 'version=', version);

                if (isCached) {
                    sendResponse({ 'ok': true, 'data': httpCacheData[key] });
                } else {
                    try {
                        fetch(url)
                            .then(httpResponse =>
                            {
                                // log.debug('background', message.type, 'httpResponse', url, httpResponse);
                                if (httpResponse.ok) {
                                    return httpResponse.text();
                                } else {
                                    throw { 'ok': false, 'status': httpResponse.status, 'statusText': httpResponse.statusText };
                                }
                            })
                            .then(text =>
                            {
                                httpCacheData[key] = text;
                                httpCacheTime[key] = Date.now();
                                let response = { 'ok': true, 'data': text };
                                log.debug('background', message.type, 'response', url, text.length, response);
                                sendResponse(response);
                            })
                            .catch(ex =>
                            {
                                log.debug('background', message.type, 'catch', url, ex);
                                sendResponse({ 'ok': false, 'status': ex.status, 'statusText': ex.statusText });
                            });
                    } catch (error) {
                        log.debug('background', message.type, 'exception', url, error);
                        sendResponse({ 'ok': false, 'status': error.status, 'statusText': error.statusText });
                    }
                    return true;
                }
            } break;

            case BackgroundMessage.type_getConfig: {
                log.debug('background', message.type, message.name);
                switch (as.String(message.name, Config.onlineConfigName)) {
                    case Config.devConfigName:
                        sendResponse(Config.getAllDev());
                        break;
                        sendResponse(Config.getAllOnline());
                        default:
                }
            } break;

            case BackgroundMessage.type_getLocalStorage: {
                let response = {};
                try {
                    let value = Config.get(message.key, undefined); // return true; if async
                    if (value != undefined) {
                        response[message.key] = value;
                    }
                } catch (error) {
                    log.warn('background', message.type, error);
                }
                log.debug('background', message.type, message.key, 'response', response);
                sendResponse(response);
                // return true; 
            } break;

            case BackgroundMessage.type_setLocalStorage: {
                log.debug('background', message.type, message.key, message.value);
                try {
                    Config.set(message.key, message.value); // return true; if async
                } catch (error) {
                    log.warn('background setLocalStorage', error);
                }
                sendResponse({});
                // return true; 
            } break;

            case BackgroundMessage.type_sendStanza: {
                if (app != null) {
                    app.handle_sendStanza(message.stanza, sender.tab.id, sendResponse);
                }
                sendResponse({});
            } break;

            case BackgroundMessage.type_pingBackground: {
                log.debug('background', message.type);
                if (app != null) {
                    app.handle_pingBackground();
                }
                sendResponse({});
            } break;

            default: {
                log.debug('background unhandled', message);
            } break;
        }
    }
);
