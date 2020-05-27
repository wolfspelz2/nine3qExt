import log = require('loglevel');
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
