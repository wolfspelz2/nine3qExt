import log = require('loglevel');
import * as $ from 'jquery';
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Config } from '../lib/Config';
import { IObserver, IObservable } from '../lib/ObservableProperty';
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
    private animations: AnimationsXml.AnimationsDefinition;
    private defaultGroup: string;
    private currentState: string = '';
    private currentAction: string = '';
    private inDrag: boolean = false;
    private currentSpeedPixelPerSec: number = as.Float(Config.get('room.defaultAvatarSpeedPixelPerSec', 100));
    private defaultSpeedPixelPerSec: number = as.Float(Config.get('room.defaultAvatarSpeedPixelPerSec', 100));

    private preventNextClick_a_hack_otherwise_draggable_clicks = false;
    private clickTimer: number = undefined;

    constructor(private app: ContentApp, private entity: Entity, private display: HTMLElement, private isSelf: boolean)
    {
        this.elem = <HTMLImageElement>$('<img class="n3q-base n3q-avatar" />').get(0);
        // var url = 'https://www.virtual-presence.org/images/wolf.png';
        // var url = app.getAssetUrl('default-avatar.png');
        var url = imgDefaultAvatar;
        this.elem.src = url;

        $(this.elem).on('click', ev =>
        {
            if (this.clickTimer == undefined) {
                if (this.preventNextClick_a_hack_otherwise_draggable_clicks) {
                    this.preventNextClick_a_hack_otherwise_draggable_clicks = false;
                } else {
                    this.clickTimer = <number><unknown>setTimeout(() =>
                    {
                        this.clickTimer = undefined;
                        this.entity.onMouseClickAvatar(ev);
                        //hw later app.zIndexTop(this.elem);
                    }, as.Float(Config.get('avatarDoubleClickDelaySec', 0.25)) * 1000);
                }
            } else {
                if (this.clickTimer != undefined) {
                    clearTimeout(this.clickTimer);
                    this.clickTimer = undefined;
                    this.entity.onMouseDoubleClickAvatar(ev);
                }
            }
        });

        $(this.elem).mouseenter((ev) => this.entity.onMouseEnterAvatar(ev));
        $(this.elem).mouseleave((ev) => this.entity.onMouseLeaveAvatar(ev));

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

    stop()
    {
        if (this.animationTimer != undefined) {
            clearTimeout(this.animationTimer);
            this.animationTimer = undefined;
        }
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

    hilite(on: boolean)
    {
        if (on) {
            $(this.elem).addClass('n3q-avatar-hilite');
        } else {
            $(this.elem).removeClass('n3q-avatar-hilite');
        }
    }

    updateObservableProperty(key: string, value: any): void
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

    async setAnimations(url: string): Promise<void>
    {
        let response = await BackgroundMessage.fetchUrl(url, '');
            if (response.ok) {
                try {
                    let parsed = AnimationsXml.AnimationsXml.parseXml(url, response.data);
                    this.onAnimations(parsed);
                } catch (error) {
                    log.info(error);
                }
        }
    }

    onAnimations(data: any): void
    {
        this.animations = data;
        this.defaultGroup = this.getDefaultGroup();

        //this.currentAction = 'wave';
        //this.currentState = 'moveleft';

        this.startNextAnimation();
    }

    private animationTimer: number = undefined;
    startNextAnimation(): void
    {
        this.currentSpeedPixelPerSec = this.defaultSpeedPixelPerSec;

        var once = true;
        var group = this.currentAction;
        this.currentAction = '';
        if (group == '') { group = this.currentState; once = false; }
        if (group == '') { group = this.defaultGroup; }

        var animation = this.getAnimationByGroup(group);
        if (animation == null || animation == undefined) {
            return;
        }

        var durationSec: number = animation.duration / 1000;
        if (durationSec < 0.1) {
            durationSec = 1.0;
        }

        this.currentSpeedPixelPerSec = Math.abs(animation.dx) / durationSec;

        this.elem.src = animation.url;

        if (this.animationTimer != undefined) {
            clearTimeout(this.animationTimer);
            this.animationTimer = undefined;
        }
        this.animationTimer = <number><unknown>setTimeout(() => { this.startNextAnimation(); }, durationSec * 1000);
    }

    getSpeedPixelPerSec(): Number
    {
        return this.currentSpeedPixelPerSec;
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

        if (groupAnimations[idx] !== undefined) {
            return new AvatarGetAnimationResult(groupAnimations[idx].url, groupAnimations[idx].weight, groupAnimations[idx].dx, groupAnimations[idx].duration, groupAnimations[idx].loop);
        }

        return null;
    }

    getDefaultGroup(): string
    {
        return as.String(this.animations.params[AnimationsXml.AvatarAnimationParam.defaultsequence], 'idle');
    }
}
