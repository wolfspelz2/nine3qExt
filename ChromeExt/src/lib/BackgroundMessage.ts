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
    constructor(public ex: ItemException) { super(false, 'error', ItemException.Fact[ex.fact] + ' ' + ItemException.Reason[ex.reason] + ' ' + (ex.detail ?? ''), ex); }
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

    static waitReady(): Promise<any>
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

    static getConfigTree(name: string): Promise<any>
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

    static getBackpackState(): Promise<GetBackpackStateResponse>
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

    static addBackpackItem(itemId: string, properties: ItemProperties, options: ItemChangeOptions): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.addBackpackItem.name, 'itemId': itemId, 'properties': properties, 'options': options }, response =>
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

    static setBackpackItemProperties(itemId: string, properties: ItemProperties, options: ItemChangeOptions): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.setBackpackItemProperties.name, 'itemId': itemId, 'properties': properties, 'options': options }, response =>
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

    static modifyBackpackItemProperties(itemId: string, changed: ItemProperties, deleted: Array<string>, options: ItemChangeOptions): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.modifyBackpackItemProperties.name, 'itemId': itemId, 'changed': changed, 'deleted': deleted, 'options': options }, response =>
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

    static rezBackpackItem(itemId: string, roomJid: string, x: number, destination: string, options: ItemChangeOptions): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.rezBackpackItem.name, 'itemId': itemId, 'roomJid': roomJid, 'x': x, 'destination': destination, 'options': options }, response =>
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

    static derezBackpackItem(itemId: string, roomJid: string, x: number, y: number, options: ItemChangeOptions): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.derezBackpackItem.name, 'itemId': itemId, 'roomJid': roomJid, 'x': x, 'y': y, 'options': options }, response =>
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

    static isBackpackItem(itemId: string): Promise<boolean>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.isBackpackItem.name, 'itemId': itemId }, response =>
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

    static executeBackpackItemAction(itemId: string, action: string, args: any, involvedIds: Array<string>): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            try {
                chrome.runtime?.sendMessage({ 'type': BackgroundMessage.executeBackpackItemAction.name, 'itemId': itemId, 'action': action, 'args': args, 'involvedIds': involvedIds }, response =>
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

}
