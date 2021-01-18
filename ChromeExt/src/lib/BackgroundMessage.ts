import log = require('loglevel');
import { ItemException } from './ItemExcption';
import { ItemProperties } from './ItemProperties';

export class BackgroundResponse
{
    constructor(public ok: boolean, public status?: string, public statusText?: string, public ex?: ItemException) { }
}

export class BackgroundSuccessResponse extends BackgroundResponse
{
    constructor() { super(true); }
}

export class BackgroundErrorResponse extends BackgroundResponse
{
    constructor(public status: string, public statusText: string) { super(false, status, statusText); }
}

export class BackgroundItemExceptionResponse extends BackgroundResponse
{
    constructor(public ex: ItemException) { super(false, 'error', ex.fact.toString() + ' ' + ex.reason.toString() + ' ' + ex.detail, ex); }
}

export class FetchUrlResponse extends BackgroundResponse
{
    constructor(public data: string) { super(true); }
}

export class GetBackpackStateResponse extends BackgroundResponse
{
    constructor(public items: { [id: string]: ItemProperties; }) { super(true); }
}

export class IsBackpackItemResponse extends BackgroundResponse
{
    constructor(public isItem: boolean) { super(true); }
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

    static async getBackpackState(): Promise<GetBackpackStateResponse>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.getBackpackState.name }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static addBackpackItem(id: string, properties: ItemProperties): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.addBackpackItem.name, 'id': id, 'properties': properties }, response =>
                {
                    if (response.ok) {
                        resolve();
                    } else {
                        reject(response.ex);
                    }
                });
            } catch (error) { reject(error); }
        });
    }

    static setBackpackItemProperties(id: string, properties: ItemProperties): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.setBackpackItemProperties.name, 'id': id, 'properties': properties }, response =>
                {
                    if (response.ok) {
                        resolve();
                    } else {
                        reject(response.ex);
                    }
                });
            } catch (error) { reject(error); }
        });
    }

    static modifyBackpackItemProperties(id: string, changed: ItemProperties, deleted: Array<string>): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.modifyBackpackItemProperties.name, 'id': id, 'changed': changed, 'deleted': deleted }, response =>
                {
                    if (response.ok) {
                        resolve();
                    } else {
                        reject(response.ex);
                    }
                });
            } catch (error) { reject(error); }
        });
    }

    static rezBackpackItem(id: string, room: string, x: number, destination: string): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.rezBackpackItem.name, 'id': id, 'room': room, 'x': x, 'destination': destination }, response =>
                {
                    if (response.ok) {
                        resolve();
                    } else {
                        reject(response.ex);
                    }
                });
            } catch (error) { reject(error); }
        });
    }

    static derezBackpackItem(id: string, room: string, x: number, y: number): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.derezBackpackItem.name, 'id': id, 'room': room, 'x': x, 'y': y }, response =>
                {
                    if (response.ok) {
                        resolve();
                    } else {
                        reject(response.ex);
                    }
                });
            } catch (error) { reject(error); }
        });
    }

    static async isBackpackItem(id: string): Promise<boolean>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.isBackpackItem.name, 'id': id }, response =>
                {
                    if (response.ok) {
                        resolve((<IsBackpackItemResponse>response).isItem);
                    } else {
                        reject(response.ex);
                    }
                });
            } catch (error) { reject(error); }
        });
    }

}
