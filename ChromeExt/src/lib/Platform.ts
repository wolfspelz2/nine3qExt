import log = require('loglevel');
import { Unbearable } from './Unbearable';

interface PlatformFetchUrlCallback { (ok: boolean, status: string, statusText: string, data: string): void }

export class Platform
{
    // Browser

    static getCurrentPageUrl(): string
    {
        return document.location.toString();
    }

    // Local storage

    private static tmpStorage: any = {};

    static setStorageString(key: string, value: string): void
    {
        // chrome.storage.local.set({ key: value }, function ()
        // {
        //   log.debug('setStorageString', key, value);
        // });

        this.tmpStorage[key] = value;
    }

    static getStorageString(key: string, defaultValue: string): string
    {
        // chrome.storage.local.get(key, (value) =>
        // {
        //   log.debug('getStorageString', value);
        // });
        // return '100';
        if (typeof this.tmpStorage[key] == typeof undefined) {
            return defaultValue;
        }
        return this.tmpStorage[key];
    }

    // Paths

    static getAssetUrl(filePath: string)
    {
        chrome.runtime?.getURL('assets/' + filePath);
    }

    // HTTP get

    static fetchUrl(url: string, callback: PlatformFetchUrlCallback)
    {
        try {
            chrome.runtime?.sendMessage({ 'type': 'fetchUrl', 'url': url }, (response) =>
            {
                callback(response.ok, response.status, response.statusText, response.data);
            });
        } catch (ex) {
            Unbearable.problem();
            // log.error(ex);
        }
    }

}
