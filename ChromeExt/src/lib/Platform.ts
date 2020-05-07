import log = require('loglevel');
import { Panic } from './Panic';
import { Config } from './Config';

interface PlatformFetchUrlCallback { (ok: boolean, status: string, statusText: string, data: string): void }

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

    static fetchUrl(url: string, version: string, callback: PlatformFetchUrlCallback)
    {
        try {
            chrome.runtime?.sendMessage({ 'type': 'fetchUrl', 'url': url, 'version': version }, response =>
            {
                callback(response.ok, response.status, response.statusText, response.data);
            });
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    static async getConfig(name: string): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': 'getConfig', 'name': name }, response =>
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
