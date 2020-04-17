import { as } from './as';

export class Platform
{
  private static tmpStorage: any = {};

  static setStorageString(key: string, value: string): void
  {
    // chrome.storage.local.set({ key: value }, function ()
    // {
    //   console.log('setStorageString', key, value);
    // });

    this.tmpStorage[key] = value;
  }

  static getStorageString(key: string, defaultValue: string): string
  {
    // chrome.storage.local.get(key, (value) =>
    // {
    //   console.log('getStorageString', value);
    // });
    // return '100';
    if (typeof this.tmpStorage[key] == typeof undefined) {
      return defaultValue;
    }
    return this.tmpStorage[key];
  }
}
