import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { BackgroundApp } from './BackgroundApp';
import { Platform } from '../lib/Platform';

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

const httpCache: Map<string, string> = new Map<string, string>();

chrome.runtime?.onMessage.addListener(
    function (message, sender, sendResponse)
    {
        switch (message.type) {
            case 'fetchUrl': {
                var url = message.url;
                var version = message.version;

                let key = version + url;
                let isCached = (httpCache[key] != undefined);

                log.debug('background fetchUrl', 'cached=', isCached, url, 'version=', version);

                if (isCached) {
                    return sendResponse({ 'ok': true, 'data': httpCache[key] });
                } else {
                    try {

                        fetch(url)
                            .then(httpResponse =>
                            {
                                log.debug('background fetchUrl response', url, httpResponse);
                                if (httpResponse.ok) {
                                    return httpResponse.text();
                                } else {
                                    throw { 'ok': false, 'status': httpResponse.status, 'statusText': httpResponse.statusText };
                                }
                            })
                            .then(text =>
                            {
                                httpCache[key] = text;
                                log.debug('background fetchUrl text', url, text.length, text.substr(0, 1000));
                                return sendResponse({ 'ok': true, 'data': text });
                            })
                            .catch(ex =>
                            {
                                log.debug('background fetchUrl catch', url, ex);
                                return sendResponse({ 'ok': false, 'status': ex.status, 'statusText': ex.statusText });
                            }
                            );

                    } catch (error) {
                        log.debug('background fetchUrl', url, error);
                        return sendResponse({ 'ok': false, 'status': error.status, 'statusText': error.statusText });
                    }
                }
            } break;

            case 'getConfig': {
                log.debug('background getConfig', message.name);
                switch (as.String(message.name, Config.onlineConfigName)) {
                    case Config.devConfigName:
                        return sendResponse(Config.getAllDev());
                    }
                return sendResponse(Config.getAllOnline());
            } break;

            case 'getLocalStorage': {
                let response = {};
                try {
                    let value = Config.get(message.key, undefined);
                    if (value != undefined) {
                        response[message.key] = value;
                    }
                } catch (error) {
                    log.warn('background getLocalStorage', error);
                }
                log.debug('background getLocalStorage', message.key, 'response', response);
                return sendResponse(response);
            } break;

            case 'setLocalStorage': {
                log.debug('background setLocalStorage', message.key, message.value);
                try {
                    Config.set(message.key, message.value);
                } catch (error) {
                    log.warn('background setLocalStorage', error);
                }
            } break;

            case 'sendStanza': {
                if (app != null) {
                    app.handle_sendStanza(message.stanza, sender.tab.id, sendResponse);
                }
            } break;

            case Platform.type_pingBackground: {
                log.debug('background pingBackground');
                if (app != null) {
                    app.handle_pingBackground();
                }
            } break;
        }

        return sendResponse({});
    }
);
