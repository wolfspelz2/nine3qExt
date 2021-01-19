import log = require('loglevel');
import { Utils } from './Utils';

export class Memory
{
    private static sessionConfig: any = {};

    static getSession(key: string, defaultValue: any): any
    {
        if (this.sessionConfig[key]) {
            return this.sessionConfig[key];
        }
        return defaultValue;
    }

    static setSession(key: string, value: any): void
    {
        this.sessionConfig[key] = value;
    }

    static async getSync(key: string, defaultValue: any): Promise<any>
    {
        return new Promise((resolve, reject) =>
        {
            if (chrome.storage != undefined) {
                chrome.storage.sync.get([key], result =>
                {
                    if (result[key] != undefined) {
                        resolve(result[key]);
                    } else {
                        resolve(defaultValue);
                    }
                });
            } else {
                reject('chrome.storage undefined');
            }
        });
    }

    static async setSync(key: string, value: any): Promise<void>
    {
        return new Promise((resolve, reject) =>
        {
            let dict = {};
            dict[key] = value;
            if (chrome.storage != undefined) {
                chrome.storage.sync.set(dict, () => { resolve(); });
            } else {
                reject('chrome.storage undefined');
            }
        });
    }

    static async getLocal(key: string, defaultValue: any): Promise<any>
    {
        return new Promise(resolve =>
        {
            chrome.storage.local.get([key], result =>
            {
                if (result[key] != undefined) {
                    resolve(result[key]);
                } else {
                    resolve(defaultValue);
                }
            });
        });
    }

    static async setLocal(key: string, value: any): Promise<void>
    {
        return new Promise(resolve =>
        {
            let dict = {};
            dict[key] = value;
            chrome.storage.local.set(dict, () => { resolve(); });
        });
    }

    static async deleteLocal(key: string): Promise<void>
    {
        return new Promise(resolve =>
        {
            chrome.storage.local.remove(key, () => { resolve(); });
        });
    }
}
