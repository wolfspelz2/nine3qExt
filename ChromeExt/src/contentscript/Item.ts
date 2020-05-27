import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { Room } from './Room';
import { Avatar } from './Avatar';
import { IframeWindow } from './IframeWindow';

import imgDefaultItem from '../assets/DefaultItem.png';

export class Item extends Entity
{
    private isFirstPresence: boolean = true;
    private properties: { [pid: string]: string } = {};
    private iframeWindow: IframeWindow;

    constructor(app: ContentApp, room: Room, display: HTMLElement, private nick: string, isSelf: boolean)
    {
        super(app, room, display, isSelf);

        $(this.getElem()).addClass('n3q-item');
    }

    getDefaultAvatar(): string { return imgDefaultItem; }

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

        let presenceHasCondition: boolean = false;
        let newCondition: string = '';

        let vpAnimationsUrl = '';
        let vpImageUrl = '';

        let newProperties: { [pid: string]: string } = {};

        // Collect info

        {
            let stateNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'firebat:avatar:state');
            if (stateNode) {
                let positionNode = stateNode.getChild('position');
                if (positionNode) {
                    newX = as.Int(positionNode.attrs.x, -1);
                    if (newX != -1) {
                        presenceHasPosition = true;
                    }
                }
                presenceHasCondition = true;
                let conditionNode = stateNode.getChild('condition');
                if (conditionNode) {
                    newCondition = as.String(conditionNode.attrs.status, '');
                }
            }
        }

        {
            let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
            if (vpPropsNode) {
                let attrs = vpPropsNode.attrs;
                if (attrs) {
                    let itemServiceId = as.String(attrs.service, '');
                    for (let attrName in attrs) {
                        let attrValue = attrs[attrName];
                        attrValue = Item.itemServiceUrlFilter(itemServiceId, attrName, attrValue);
                        newProperties[attrName] = attrValue;
                    }
                    // vpNickname = as.String(attrs.Nickname, '');
                    vpAnimationsUrl = as.String(attrs.AnimationsUrl, '');
                    vpImageUrl = as.String(attrs.ImageUrl, '');
                }
            }
        }

        // Do someting with the data

        // vpAnimationsUrl = 'https://weblin-avatar.dev.sui.li/items/baum/avatar.xml';
        // vpAnimationsUrl = '';
        // vpImageUrl = 'https://weblin-avatar.dev.sui.li/items/baum/idle.png';
        // vpImageUrl = '';

        {
            this.properties = newProperties;
            if (vpImageUrl == '') {
                if (this.properties.Image100Url) {
                    vpImageUrl = this.properties.Image100Url;
                }
            }
        }

        if (this.isFirstPresence) {
            this.avatarDisplay = new Avatar(this.app, this, this.getCenterElem(), this.isSelf);
        }

        if (this.avatarDisplay) {
            if (vpAnimationsUrl != '') {
                let proxiedAnimationsUrl = as.String(Config.get('avatars.animationsProxyUrlTemplate', 'https://avatar.weblin.sui.li/avatar/?url={url}')).replace('{url}', encodeURIComponent(vpAnimationsUrl));
                this.avatarDisplay?.updateObservableProperty('AnimationsUrl', proxiedAnimationsUrl);
            } else {
                if (vpImageUrl != '') {
                    this.avatarDisplay?.updateObservableProperty('ImageUrl', vpImageUrl);
                }
            }
        }

        if (presenceHasCondition) {
            this.avatarDisplay?.setCondition(newCondition);
        }

        if (this.isFirstPresence) {
            if (!presenceHasPosition) {
                newX = this.isSelf ? await this.app.getSavedPosition() : this.app.getDefaultPosition();
            }
            if (newX < 0) { newX = 100; }
            this.setPosition(newX);
        } else {
            if (presenceHasPosition) {
                if (this.getPosition() != newX) {
                    this.move(newX);
                }
            }
        }

        if (this.isFirstPresence) {
            this.show(true);
        }

        if (this.isFirstPresence) {
            if (this.room?.iAmAlreadyHere()) {
                this.room?.showChatMessage(this.nick, 'appeared');
            } else {
                this.room?.showChatMessage(this.nick, 'is present');
            }
        }

        this.isFirstPresence = false;
    }

    static itemServiceUrlFilter(itemServiceId: string, attrName: string, attrValue: string): any
    {
        if (itemServiceId) {
            var itemServices = Config.get('itemServices', []);
            let itemServiceTree = itemServices[itemServiceId];
            if (itemServiceTree) {
                let itemServiceConfig = itemServiceTree.config;
                if (itemServiceConfig) {
                    let propertyUrlFilter = itemServiceConfig.itemPropertyUrlFilter;
                    if (propertyUrlFilter) {
                        for (let i = 0; i < propertyUrlFilter.length; i++) {
                            if (propertyUrlFilter[i].key && propertyUrlFilter[i].value) {
                                attrValue = attrValue.replace(propertyUrlFilter[i].key, propertyUrlFilter[i].value);
                            }
                        }
                    }
                }
            }
        }
        return attrValue;
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();

        this.room?.showChatMessage(this.nick, 'disappeared');
    }

    onMouseClickAvatar(ev: JQuery.Event): void
    {
        super.onMouseClickAvatar(ev);
        if (this.properties) {
            if (this.properties.IframeAspect) {
                this.toggleIframe(this.getElem());
            }
        }
    }

    toggleIframe(aboveElem: HTMLElement)
    {
        if (this.iframeWindow) {
            this.iframeWindow.close();
        } else {
            this.iframeWindow = new IframeWindow(this.app, this.display);
            this.iframeWindow.show({
                above: aboveElem,
                resizable: as.Bool(this.properties.IframeResizable, true),
                titleText: as.String(this.properties.Label, 'Item'),
                url: as.String(this.properties.IframeUrl, 'https://example.com'),
                onClose: () => { this.iframeWindow = null; },
            });
        }
    }
}
