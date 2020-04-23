import * as $ from 'jquery';
import 'jqueryui';
import { as } from './as';
import { Log } from './Log';
import { App } from './App';
import { Entity } from './Entity';
import { Platform } from './Platform';
import { IObserver, IObservable } from './ObservableProperty';
import * as AnimationsXml from './AnimationsXml';

import imgDefaultAvatar from '../assets/DefaultAvatar.png';

class AvatarGetAnimationResult
{
    constructor(
        public url: string,
        public weight: number,
        public dx: number,
        public duration: number,
        public loop: boolean
    ) { }
}

export class Avatar implements IObserver
{
    private elem: HTMLImageElement;
    private imageUrl: string;
    private hasAnimation = false;
    private animationsUrl: string;
    private animationsUrlBase: string;
    private animations: AnimationsXml.AnimationsDefinition;
    private defaultGroup: string;
    private currentState: string = '';
    private currentAction: string = '';
    private animationTimer: NodeJS.Timeout;
    private inDrag: boolean = false;
    private speedInPixelPerMsec: number = 0.1;
    private defaultSpeedInPixelPerMsec: number = 0.1;
    private doubleClickDelay: number = 20;

    private preventNextClick_a_hack_otherwise_draggable_clicks = false;
    private clickTimer: number = null;

    constructor(private app: App, private entity: Entity, private display: HTMLElement, private isSelf: boolean)
    {
        this.elem = <HTMLImageElement>$('<img class="n3q-avatar" />')[0];
        // var url = 'https://www.virtual-presence.org/images/wolf.png';
        // var url = app.getAssetUrl('default-avatar.png');
        var url = imgDefaultAvatar;
        this.elem.src = url;

        display.appendChild(this.elem);

        $(this.elem).draggable({
            scroll: false,
            stack: '.n3q-participant',
            opacity: 0.5,
            distance: 4,
            helper: 'clone',
            zIndex: 1100000000,
            containment: 'document',
            start: (ev: JQueryMouseEventObject, ui) =>
            {
                this.app.enableScreen(true);
                this.inDrag = true;
                this.a_hack_otherwise_draggable_clicks_start(ev);
                this.entity.onStartDragAvatar(ev, ui);
            },
            drag: (ev: JQueryMouseEventObject, ui) =>
            {
                this.entity.onDragAvatar(ev, ui);
            },
            stop: (ev: JQueryMouseEventObject, ui) =>
            {
                this.entity.onStopDragAvatar(ev, ui);
                this.a_hack_otherwise_draggable_clicks_stop(ev);
                this.inDrag = false;
                this.app.enableScreen(false);
            }
        });

    }

    a_hack_otherwise_draggable_clicks_x: number;
    a_hack_otherwise_draggable_clicks_start(ev: JQueryMouseEventObject): void
    {
        this.a_hack_otherwise_draggable_clicks_x = $(this.elem).offset().left;
    }
    a_hack_otherwise_draggable_clicks_stop(ev: JQueryMouseEventObject): void
    {
        if (ev.clientX > this.a_hack_otherwise_draggable_clicks_x && ev.clientX <= this.a_hack_otherwise_draggable_clicks_x + this.elem.width) {
            this.preventNextClick_a_hack_otherwise_draggable_clicks = true;
        }
    }

    update(key: string, value: any): void
    {
        switch (key) {
            case 'ImageUrl': {
                if (!this.hasAnimation) {
                    this.setImage(value);
                }
            } break;
            case 'AnimationsUrl': {
                this.hasAnimation = true;
                this.setAnimations(value);
            } break;
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
        this.startNextAnimation();
    }

    setAction(action: string): void
    {
        this.currentAction = action;
        this.startNextAnimation();
    }

    setAnimations(url: string): void
    {
        this.animationsUrl = url;

        Platform.fetchUrl(url, (ok, status, statusText, data) =>
        {
            if (ok) {
                let parsed = AnimationsXml.AnimationsXml.parseXml(url, data);
                this.onAnimations(parsed);
            }
        });
    }

    onAnimations(data: any): void
    {
        this.animations = data;
        this.defaultGroup = this.getDefaultGroup();

        //this.currentAction = 'wave';
        //this.currentState = 'moveleft';

        this.startNextAnimation();
    }

    startNextAnimation(): void
    {
        this.speedInPixelPerMsec = this.defaultSpeedInPixelPerMsec;

        var once = true;
        var group = this.currentAction;
        this.currentAction = '';
        if (group == '') { group = this.currentState; once = false; }
        if (group == '') { group = this.defaultGroup; }

        var animation = this.getAnimationByGroup(group);
        if (animation == null || typeof animation == typeof undefined) {
            return;
        }

        var duration: number = animation.duration;
        if (as.Int(duration, 0) < 10) {
            duration = 1000;
        }

        this.speedInPixelPerMsec = Math.abs(animation.dx) / duration;

        this.elem.src = animation.url;

        if (this.animationTimer != undefined) {
            clearTimeout(this.animationTimer);
        }
        this.animationTimer = setTimeout(() => this.startNextAnimation(), duration);
    }

    getSpeedInPixelPerMsec(): Number
    {
        return this.speedInPixelPerMsec;
    }

    getAnimationByGroup(group: string): AvatarGetAnimationResult
    {
        if (this.animations == null) { return null; }

        var groupAnimations: AvatarGetAnimationResult[] = [];
        var nWeightSum: number = 0;

        for (var name in this.animations.sequences) {
            var animation = this.animations.sequences[name];
            if (animation.group == group) {
                var nWeight = as.Int(animation.weight, 1);
                nWeightSum += nWeight;
                groupAnimations.push(new AvatarGetAnimationResult(animation.url, nWeight, as.Int(animation.dx, 0), as.Int(animation.duration, 1000), as.Bool(animation.loop, false)));
            }
        }

        var nRnd = Math.random() * nWeightSum;
        var idx = 0;

        var nCurrentSum = 0;
        for (var i = 0; i < groupAnimations.length; i++) {
            nCurrentSum += groupAnimations[i].weight;
            if (nRnd < nCurrentSum) {
                idx = i;
                break;
            }
        }

        if (typeof groupAnimations[idx] !== typeof undefined) {
            return new AvatarGetAnimationResult(groupAnimations[idx].url, groupAnimations[idx].weight, groupAnimations[idx].dx, groupAnimations[idx].duration, groupAnimations[idx].loop);
        }

        return null;
    }

    getDefaultGroup(): string
    {
        return as.String(this.animations.params[AnimationsXml.AvatarAnimationParam.defaultsequence], 'idle');
    }
}
