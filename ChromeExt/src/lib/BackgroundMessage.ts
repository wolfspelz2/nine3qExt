import log = require('loglevel');
import { ItemChangeOptions } from './ItemChangeOptions';
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
    static test(): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.test.name }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static jsonRpc(url: string, jsonBodyData: any): Promise<FetchUrlResponse>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.jsonRpc.name, 'url': url, 'json': jsonBodyData }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static fetchUrl_nocache = '_nocache';
    static fetchUrl(url: string, version: string): Promise<FetchUrlResponse>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.fetchUrl.name, 'url': url, 'version': version }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static async waitReady(): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.waitReady.name }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static async getConfigTree(name: string): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.getConfigTree.name, 'name': name }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static async getSessionConfig(key: string, defaultValue: any): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.getSessionConfig.name, 'key': key }, response =>
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

    static setSessionConfig(key: string, value: any): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.setSessionConfig.name, 'key': key, 'value': value }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static sendStanza(stanza: any): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.sendStanza.name, 'stanza': stanza }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static pingBackground(): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.pingBackground.name }, response =>
                {
                    resolve(response);
                });
            } catch (error) {
                reject(error);
            }
        });
    }

    static userSettingsChanged(): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.userSettingsChanged.name }, response =>
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

    static setBackpackItemProperties(id: string, properties: ItemProperties, options: ItemChangeOptions): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.setBackpackItemProperties.name, 'id': id, 'properties': properties, 'options': options }, response =>
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

    static modifyBackpackItemProperties(id: string, changed: ItemProperties, deleted: Array<string>, options: ItemChangeOptions): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.modifyBackpackItemProperties.name, 'id': id, 'changed': changed, 'deleted': deleted, 'options': options }, response =>
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
