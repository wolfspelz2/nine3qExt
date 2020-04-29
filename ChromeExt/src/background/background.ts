const isBackground = true;
console.log('Background', isBackground);

import log = require('loglevel');
import { BackgroundApp } from './BackgroundApp';

log.setLevel(log.levels.DEBUG);
var app = null;

function activate()
{
    if (app == null) {
        app = new BackgroundApp();
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

// window.addEventListener('onbeforeunload', deactivate);

// window.addEventListener('visibilitychange', function ()
// {
//     if (document.visibilityState === 'visible') {
//         activate();
//     } else {
//         deactivate();
//     }
// });

activate();

// chrome.runtime.onMessage.addListener(
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

let reInstalled = false;
chrome.runtime.onInstalled.addListener(function (details)
{
    // details = {previousVersion: "1.0.0", reason: update" }
    reInstalled = true;
})

chrome.runtime.onMessage.addListener(
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

                } catch (ex) {
                    log.trace('background fetchUrl ex', ex);
                    return sendResponse({ 'ok': false, 'status': ex.status, 'statusText': ex.statusText });
                }
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