const $ = require('jquery');
import { Platform } from './Platform';

export class IdentityItem
{
  public digest: string;
  public contenttype: string;
  public src: string;
  public encoding: string;
  public mimetype: string;
  public order: string;
  public text: string;
}

export class Identity
{
  private isFetching: boolean = false;
  private isFetched: boolean = false;
  private items: { [id: string]: IdentityItem; } = {};

  constructor(private url: string, private digest: string)
  {
    this.fetch(this.url);
  }

  changed(url: string, digest: string): void
  {
    let changed = false;
    if (url != this.url || digest != this.digest) {
      this.url = url;
      this.digest = digest;
      changed = true;
    }

    if (changed) {
      this.fetch(this.url);
    }
  }

  fetch(url: string): void
  {
    if (!this.isFetching) {
      this.isFetching = true;
      Platform.fetchUrl(url, (ok, status, statusText, data) =>
      {
        this.isFetching = false;
        if (ok) {
          this.evaluate(data);
        }
      });
    }
  }

  evaluate(data: string): void
  {
    try {
      let doc = $.parseXML(data);
    } catch (ex) {
    }
  }
}
