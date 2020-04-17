const $ = require('jquery');
import { App } from './App';
import { Entity } from './Entity';

export class Avatar
{
  elem: HTMLElement;

  constructor(private app: App, private entity: Entity, private display: HTMLElement)
  {
    this.elem = <HTMLElement>$('<div class="n3q-avatar" />')[0];
    display.appendChild(this.elem);
  }
}
