import log = require('loglevel');
import { Panic } from './Panic';

export class FetchUrlResponse
{
    constructor(
        public ok: boolean,
        public status: string,
        public statusText: string,
        public data: string
    ) { }
}

export class BackgroundMessage
{
    static type_fetchUrl = 'fetchUrl';
    static fetchUrl(url: string, version: string): Promise<FetchUrlResponse>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_fetchUrl, 'url': url, 'version': version }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_waitReady = 'waitReady';
    static async waitReady(): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_waitReady }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_getConfigTree = 'getConfigTree';
    static async getConfigTree(name: string): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_getConfigTree, 'name': name }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_getSessionConfig = 'getSessionConfig';
    static async getSessionConfig(key: string, defaultValue: any): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_getSessionConfig, 'key': key }, response =>
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

    static type_setSessionConfig = 'setSessionConfig';
    static setSessionConfig(key: string, value: any): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_setSessionConfig, 'key': key, 'value': value }, response =>
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
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_sendStanza, 'stanza': stanza }, response =>
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
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_pingBackground }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_userSettingsChanged = 'userSettingsChanged';
    static userSettingsChanged(): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_userSettingsChanged }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

}
