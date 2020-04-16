import { Room } from './Room';

export class Entity
{
  private positionX: number = -1;

  constructor(private room: Room)
  {
  }

  setPosition(x: number): void
  {
    this.positionX = x;
    // this.elem.style.left = x + 'px';
  }

  getPosition(): number
  {
    return this.positionX; 
  }

}
