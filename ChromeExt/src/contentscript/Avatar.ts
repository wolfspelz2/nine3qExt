const $ = require('jquery');
import { App } from './App';
import { Entity } from './Entity';

import imgDefaultAvatar from '../assets/DefaultAvatar.png';
// import imgDefaultAvatar from '../assets/Items/Admin/test.png';

export class Avatar
{
  elem: HTMLImageElement;

  constructor(private app: App, private entity: Entity, private display: HTMLElement)
  {
    this.elem = <HTMLImageElement>$('<img class="n3q-avatar" />')[0];
    // var url = 'https://www.virtual-presence.org/images/wolf.png';
    // var url = app.getAssetUrl('default-avatar.png');
    var url = imgDefaultAvatar;
    this.elem.src = url;
    display.appendChild(this.elem);
  }
}
