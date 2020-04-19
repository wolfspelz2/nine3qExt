const $ = require('jquery');
import { App } from './App';
import { Entity } from './Entity';
import { IObserver, IObservable } from './ObservableProperty';

import imgDefaultAvatar from '../assets/DefaultAvatar.png';

export class Avatar implements IObserver
{
    elem: HTMLImageElement;
    imageUrl: string;
    currentState: string = '';

    constructor(private app: App, private entity: Entity, private display: HTMLElement)
    {
        this.elem = <HTMLImageElement>$('<img class="n3q-avatar" />')[0];
        // var url = 'https://www.virtual-presence.org/images/wolf.png';
        // var url = app.getAssetUrl('default-avatar.png');
        var url = imgDefaultAvatar;
        this.elem.src = url;
        display.appendChild(this.elem);
    }

    update(key: string, value: any): void
    {
        switch (key) {
            case 'ImageUrl': this.setImage(value); break;
        }
    }

    setImage(url: string): void
    {
        this.imageUrl = url;
        this.elem.src = this.imageUrl;
    }

    setState(state: string): void
    {
        this.currentState = state;
        // this.startNextAnimation();
    }
}
