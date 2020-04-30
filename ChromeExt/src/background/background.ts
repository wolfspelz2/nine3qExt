import log = require('loglevel');
import { Config } from '../lib/Config';
import { Panic } from '../lib/Panic';
import { BackgroundApp } from './BackgroundApp';

const isBackground = true;
console.log('Background', isBackground);

let debug = true;
log.setLevel(log.levels.INFO);

if (debug) {
    log.setLevel(log.levels.DEBUG);
}

log.setLevel(log.levels.DEBUG);
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

chrome.runtime?.onMessage.addListener(
    function (message, sender, sendResponse)
    {
        switch (message.type) {
            case 'fetchUrl': {
                var url = message.url;
                console.debug('background fetchUrl', url);
                try {

                    fetch(url)
                        .then(httpResponse =>
                        {
                            log.trace('background fetchUrl response', httpResponse);
                            if (httpResponse.ok) {
                                return httpResponse.text();
                            } else {
                                throw { 'ok': false, 'status': httpResponse.status, 'statusText': httpResponse.statusText };
                            }
                        })
                        .then(text =>
                        {
                            log.trace('background fetchUrl text', text);
                            return sendResponse({ 'ok': true, 'data': text });
                        })
                        .catch(ex =>
                        {
                            log.trace('background fetchUrl catch', ex);
                            return sendResponse({ 'ok': false, 'status': ex.status, 'statusText': ex.statusText });
                        }
                        );

                } catch (error) {
                    log.trace('background fetchUrl error', error);
                    return sendResponse({ 'ok': false, 'status': error.status, 'statusText': error.statusText });
                }
            } break;

            case 'getConfig': {
                return sendResponse(Config.getAllOnline());
            } break;

            case 'sendStanza': {
                if (app != null) {
                    return app.handle_sendStanza(message.stanza, sender.tab.id, sendResponse);
                }
            } break;
        }

        return true;
    }
);
