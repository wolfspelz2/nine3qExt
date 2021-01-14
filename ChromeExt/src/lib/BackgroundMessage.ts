import log = require('loglevel');
import { ItemProperties } from './ItemProperties';
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

export class GetBackpackStateResponse
{
    constructor(
        public ok: boolean,
        public status: string,
        public statusText: string,
        public items: { [id: string]: ItemProperties; }
    ) { }
}

export class BackgroundMessage
{
    static type_test = 'test';
    static test(): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_test }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_jsonRpc = 'jsonRpc';
    static jsonRpc(url: string, jsonBodyData: any): Promise<FetchUrlResponse>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_jsonRpc, 'url': url, 'json': jsonBodyData }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static fetchUrl_nocache = '_nocache';
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

    static type_getBackpackState = 'getBackpackState';
    static async getBackpackState(): Promise<GetBackpackStateResponse>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_getBackpackState }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_setBackpackItemProperties = 'setBackpackItemProperties';
    static setBackpackItemProperties(id: string, properties: ItemProperties): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_setBackpackItemProperties, 'id': id, 'properties': properties }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static type_rezBackpackItem = 'rezBackpackItem';
    static rezBackpackItem(id: string, room: string, x: number, destination: string): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.type_rezBackpackItem, 'id': id, 'room': room, 'x': x, 'destination': destination }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

}
