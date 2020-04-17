import { as } from './as';

interface PlatformFetchUrlCallback { (ok: boolean, status: string, statusText: string, data: string): void }

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

  // $.get('https://storage.zweitgeist.com/index.php/295')
  // .done((data) =>
  // {
  //   console.log('done', data);
  // })
  // .fail(() =>
  // {
  //   console.log('fail');
  // })
  // .always(() =>
  // {
  //   console.log('always');
  // });

  // Platform.fetchUrl('https://storage.zweitgeist.com/index.php/295', (ok, status, statusText, data) =>
  // {
  //   console.log('Platform.fetchUrlCallback', ok, status, statusText, data);
  //   alert(data);
  // });
  static fetchUrl(url: string, callback: PlatformFetchUrlCallback)
  {
    console.log('Platform.fetchUrl', url);

    chrome.runtime.sendMessage({ 'type': 'fetchUrl', 'url': url }, response =>
    {
      // console.log('Platform.fetchUrl', response);
      callback(response.ok, response.status, response.statusText, response.data);
    });
  }
}
