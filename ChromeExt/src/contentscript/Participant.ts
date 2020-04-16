import { Entity } from './Entity';
import { Room } from './Room';

export class Participant extends Entity
{
  private firstPresence: boolean = true;

  constructor(room: Room, private nick: string, private isSelf: boolean)
  {
    super(room);
  }

  onPresence(stanza: any)
  {
    var presenceHasPosition: boolean = false;
    var newX: number = 123;

    {
      newX = parseInt('300');
      presenceHasPosition = true;
    }

    if (this.firstPresence)
    {
      this.firstPresence = false;

      // if (!presenceHasPosition)
      // {
      //   newX = this.isSelf ? this.app.getSavedPosition() : this.app.getDefaultPosition();
      // }
      // if (newX < 0) { newX = 100; }
      // this.setPosition(newX);

      // this.avatar = new Avatar(this.app, this, this.getCenterElem(), this.app.getImagesBaseUrl() + 'DefaultAvatar.png');
      // this.nickname = new Nickname(this.app, this, this.getElem());
      // this.chatout = new Chatout(this.app, this, this.getElem());
      // this.chatin = new Chatin(this.app, this, this.getElem());

      // this.show(true);

      // this.app.sendGetUserAttributesMessage(this.id, msg => this.onAttributes(msg));
    } else
    {

      if (presenceHasPosition)
      {
        if (this.getPosition() != newX)
        {
          // this.move(newX, this.speedX);
        }
      }

    }
  }
}
