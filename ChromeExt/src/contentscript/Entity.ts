const $ = require('jquery');
import { Room } from './Room';

export class Entity
{
  private elem: HTMLElement;
  private centerElem: HTMLElement;
  private positionX: number = -1;
  private visible: boolean = false;

  constructor(private room: Room, protected display: HTMLElement)
  {
    this.elem = <HTMLDivElement>$('<div class="n3q-entity" />')[0];
    this.elem.style.display = 'none';
    this.display.appendChild(this.elem);

    // let avatarElement = <HTMLDivElement>$('<div class="n3q-avatar" />')[0];
    // entityElement.appendChild(avatarElement);
  }

  getElem(): HTMLElement { return this.elem; }

  show(visible: boolean): void
  {
    if (visible != this.visible)
    {
      this.elem.style.display = visible ? 'block' : 'none';
      this.visible = visible;
    }
  }

  setPosition(x: number): void
  {
    this.positionX = x;
    this.elem.style.left = x + 'px';
  }

  getPosition(): number
  {
    return this.positionX;
  }

}
