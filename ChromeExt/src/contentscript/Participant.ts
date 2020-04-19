const $ = require('jquery');
import { App } from './App';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { Identity } from './Identity';
import { as } from './as';

export class Participant extends Entity
{
  private avatar: Avatar;
  private firstPresence: boolean = true;
  private speedX: number = 80;
  private identity: Identity;

  constructor(private app: App, room: Room, display: HTMLElement, private nick: string, private isSelf: boolean)
  {
    super(room, display);
    $(this.getElem()).addClass('n3q-participant');
  }

  onPresenceAvailable(stanza: any)
  {
    var presenceHasPosition: boolean = false;
    var newX: number = 123;

    {
      newX = as.Int(
        stanza
          .getChildren('x')
          .find(stanzaChild => stanzaChild.attrs.xmlns === 'firebat:avatar:state')
          .getChild('position')
          .attrs.x
        , -1);

      if (newX != -1) {
        presenceHasPosition = true;
      }
    }

    {
      let identityAttrs = stanza
        .getChildren('x')
        .find(stanzaChild => stanzaChild.attrs.xmlns === 'firebat:user:identity')
        .attrs
        ;

      let url = as.String(identityAttrs.src, '');
      let digest = as.String(identityAttrs.digest, '');

      if (url != '' && digest != '') {
        if (this.identity == null) {
          this.identity = new Identity(url, digest);
        } else {
          this.identity.changed(url, digest);
        }
      }
    }

    if (this.firstPresence) {
      this.firstPresence = false;

      if (!presenceHasPosition) {
        newX = this.isSelf ? this.app.getSavedPosition() : this.app.getDefaultPosition();
      }
      if (newX < 0) { newX = 100; }
      this.setPosition(newX);

      this.avatar = new Avatar(this.app, this, this.getCenterElem());
      // this.nickname = new Nickname(this.app, this, this.getElem());
      // this.chatout = new Chatout(this.app, this, this.getElem());
      // this.chatin = new Chatin(this.app, this, this.getElem());

      this.show(true);

      // this.app.sendGetUserAttributesMessage(this.id, msg => this.onAttributes(msg));
    } else {

      if (presenceHasPosition) {
        if (this.getPosition() != newX) {
          this.move(newX, this.speedX);
        }
      }

    }
  }

  onPresenceUnavailable(stanza: any)
  {
    this.shutdown();
  }

  move(newX: number, speedX: number): void
  {
    if (newX < 0) { newX = 0; }

    if (this.isSelf) {
      this.app.savePosition(newX);
    }

    this.setPosition(this.getPosition());

    var oldX = this.getPosition();
    var diffX = newX - oldX;
    if (diffX < 0) {
      diffX = -diffX;
      this.avatar.setState('moveleft');
    } else {
      this.avatar.setState('moveright');
    }
    var duration = (diffX * 1000) / speedX;

    $(this.getElem())
      .stop(true)
      .animate(
        { left: newX + 'px' },
        duration,
        'linear',
        () => this.onMoveDestinationReached(newX)
      );
  }

  onMoveDestinationReached(newX: number): void
  {
    this.setPosition(newX);
    this.avatar.setState('');
  }

}
