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

    // HTTP get

    static type_fetchUrl = 'fetchUrl';
    static fetchUrl(url: string, version: string, callback: PlatformFetchUrlCallback)
    {
        try {
            chrome.runtime?.sendMessage({ 'type': Platform.type_fetchUrl, 'url': url, 'version': version }, response =>
            {
                log.debug('Platform.fetchUrl response', response);
                callback(response.ok, response.status, response.statusText, response.data);
            });
        } catch (error) {
            log.info(error);
            Panic.now();
        }
    }

    static type_getConfig = 'getConfig';
    static async getConfig(name: string): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': Platform.type_getConfig, 'name': name }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_getLocalStorage = 'getLocalStorage';
    static async getLocalStorage(key: string, defaultValue: any): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': Platform.type_getLocalStorage, 'key': key }, response =>
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

    static type_setLocalStorage = 'setLocalStorage';
    static setLocalStorage(key: string, value: any): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': Platform.type_setLocalStorage, 'key': key, 'value': value }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_sendStanza = 'sendStanza';
    static sendStanza(stanza: any): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': Platform.type_sendStanza, 'stanza': stanza }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_pingBackground = 'pingBackground';
    static pingBackground(): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': Platform.type_pingBackground }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

}
