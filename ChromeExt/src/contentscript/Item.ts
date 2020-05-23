import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { Nickname } from './Nickname';
import { Chatout } from './Chatout';
import { Chatin } from './Chatin';
import { Environment } from '../lib/Environment';

export class Item extends Entity
{
    private isFirstPresence: boolean = true;

    constructor(app: ContentApp, room: Room, display: HTMLElement, private nick: string, isSelf: boolean)
    {
        super(app, room, display, isSelf);

        $(this.getElem()).addClass('n3q-item');
    }

    remove(): void
    {
        this.avatarDisplay?.stop();
        super.remove();
    }

    // presence

    async onPresenceAvailable(stanza: any): Promise<void>
    {
        let presenceHasPosition: boolean = false;
        let newX: number = 123;
        let vpAnimationsUrl = '';
        let vpImageUrl = '';

        {
            let stateNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'firebat:avatar:state');
            if (stateNode != null) {
                let positionNode = stateNode.getChild('position');
                if (positionNode != undefined) {
                    newX = as.Int(positionNode.attrs.x, -1);
                    if (newX != -1) {
                        presenceHasPosition = true;
                    }
                }
            }
        }

        {
            let vpNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
            if (vpNode) {
                let attrs = vpNode.attrs;
                if (attrs) {
                    vpAnimationsUrl = as.String(attrs.animationsUrl, '');
                    vpImageUrl = as.String(attrs.imageUrl, '');
                }
            }
        }

        // vpAnimationsUrl = 'https://weblin-avatar.dev.sui.li/items/baum/avatar.xml';
        // vpAnimationsUrl = '';
        // vpImageUrl = 'https://weblin-avatar.dev.sui.li/items/baum/idle.png';
        // vpImageUrl = '';

        if (this.isFirstPresence) {
            this.isFirstPresence = false;

            if (!presenceHasPosition) {
                newX = this.app.getDefaultPosition();
            }
            if (newX < 0) { newX = 100; }
            this.setPosition(newX);

            {
                this.avatarDisplay = new Avatar(this.app, this, this.getCenterElem(), false);
                if (vpAnimationsUrl != '') {
                    let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://avatar.weblin.sui.li/avatar/?url={url}')).replace('{url}', encodeURIComponent(vpAnimationsUrl));
                    this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
                } else {
                    if (vpImageUrl != '') {
                        this.avatarDisplay?.updateObservableProperty('ImageUrl', vpImageUrl);
                    }
                }
            }

            this.show(true);

            if (this.room?.iAmAlreadyHere()) {
                this.room?.showChatMessage(this.nick, 'appeared');
            } else {
                this.room?.showChatMessage(this.nick, 'is present');
            }

        } else {

            if (presenceHasPosition) {
                if (this.getPosition() != newX) {
                    this.move(newX);
                }
            }
        }
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();

        this.room?.showChatMessage(this.nick, 'disappeared');
    }
}
