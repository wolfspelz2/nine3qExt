import log = require('loglevel');
import { Panic } from './Panic';

interface PlatformFetchUrlCallback { (ok: boolean, status: string, statusText: string, data: string): void }
interface PlatformGetConfigCallback { (config: any): void }

export class Platform
{
    // Browser

    static getCurrentPageUrl(): string
    {
        return document.location.toString();
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
            chrome.runtime?.sendMessage({ 'type': 'fetchUrl', 'url': url }, response =>
            {
                callback(response.ok, response.status, response.statusText, response.data);
            });
        } catch (error) {
            Panic.now();
            // log.error(ex);
        }
    }

    static async getConfig(): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': 'getConfig' }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static async getLocalStorage(key: string, defaultValue: any): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': 'getLocalStorage', 'key': key }, response =>
                {
                    if (response != undefined && response[key] != undefined) {
                        resolve(response[key]);
                    } else {
                        resolve(defaultValue);
                    }
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static setLocalStorage(key: string, value: any): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': 'setLocalStorage', 'key': key, 'value': value }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

}
