import log = require('loglevel');
import * as $ from 'jquery';
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';
import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { IObserver, IObservable } from '../lib/ObservableProperty';
import * as AnimationsXml from './AnimationsXml';

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
    private elem: HTMLDivElement;
    private hasAnimation = false;
    private animations: AnimationsXml.AnimationsDefinition;
    private defaultGroup: string;
    private currentCondition: string = '';
    private currentState: string = '';
    private currentAction: string = '';
    private inDrag: boolean = false;
    private isDefault: boolean = true;
    private currentSpeedPixelPerSec: number = as.Float(Config.get('room.defaultAvatarSpeedPixelPerSec', 100));
    private defaultSpeedPixelPerSec: number = as.Float(Config.get('room.defaultAvatarSpeedPixelPerSec', 100));

    private clickDblClickSeparationTimer: number;
    private hackSuppressNextClickOtherwiseDraggableClicks: boolean = false;

    isDefaultAvatar(): boolean { return this.isDefault; }

    constructor(private app: ContentApp, private entity: Entity, private isSelf: boolean)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-avatar" />').get(0);

        // var url = 'https://www.virtual-presence.org/images/wolf.png';
        // var url = app.getAssetUrl('default-avatar.png');
        var url = entity.getDefaultAvatar();
        // this.elem.src = url;
        this.setImage(url);
        this.setSize(38, 94);
        this.isDefault = true;

        $(this.elem).on('click', ev =>
        {
            if (this.hackSuppressNextClickOtherwiseDraggableClicks) {
                this.hackSuppressNextClickOtherwiseDraggableClicks = false;
                return;
            }

            if (!this.clickDblClickSeparationTimer) {
                this.clickDblClickSeparationTimer = <number><unknown>setTimeout(() =>
                {
                    this.clickDblClickSeparationTimer = null;
                    this.entity.onMouseClickAvatar(ev);
                }, as.Float(Config.get('avatarDoubleClickDelaySec', 0.25)) * 1000);
            } else {
                if (this.clickDblClickSeparationTimer) {
                    clearTimeout(this.clickDblClickSeparationTimer);
                    this.clickDblClickSeparationTimer = null;
                    this.entity.onMouseDoubleClickAvatar(ev);
                }
            }
        });

        $(this.elem).mouseenter(ev => this.entity.onMouseEnterAvatar(ev));
        $(this.elem).mouseleave(ev => this.entity.onMouseLeaveAvatar(ev));

        $(entity.getElem()).append(this.elem);

        $(this.elem).draggable({
            scroll: false,
            stack: '.n3q-item',
            opacity: 0.5,
            distance: 4,
            helper: 'clone',
            // zIndex: 1100000000,
            containment: 'document',
            start: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                this.app.enableScreen(true);
                this.inDrag = true;
                this.entity.onDragAvatarStart(ev, ui);
            },
            drag: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                this.entity.onDragAvatar(ev, ui);
            },
            stop: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                this.entity.onDragAvatarStop(ev, ui);
                this.inDrag = false;
                this.app.enableScreen(false);
                $(this.elem).css('z-index', '');

                this.hackSuppressNextClickOtherwiseDraggableClicks = true;
                setTimeout(() => { this.hackSuppressNextClickOtherwiseDraggableClicks = false; }, 200);
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

    hilite(on: boolean)
    {
        if (on) {
            $(this.elem).addClass('n3q-avatar-hilite');
        } else {
            $(this.elem).removeClass('n3q-avatar-hilite');
        }
    }

    updateObservableProperty(key: string, value: string): void
    {
        switch (key) {
            case 'ImageUrl': {
                if (!this.hasAnimation) {
                    let defaultSize = Config.get('room.defaultStillimageSize', 80);
                    this.setSize(defaultSize, defaultSize);
                    this.setImage(value);
                }
            } break;
            case 'VCardImageUrl': {
                if (!this.hasAnimation) {
                    let maxSize = Config.get('room.defaultStillimageSize', 80);
                    let minSize = maxSize * 0.75;
                    let defaultSize = Utils.randomInt(minSize, maxSize);
                    this.setSize(defaultSize, defaultSize);
                    this.setImage(value);
                }
            } break;
            case 'AnimationsUrl': {
                this.hasAnimation = true;
                let defaultSize = Config.get('room.defaultAnimationSize', 100);
                this.setSize(defaultSize, defaultSize);
                this.setAnimations(value);
            } break;
        }
    }

    getImageBlob(url: string): Promise<Blob>
    {
        return new Promise(async resolve =>
        {
            let response = await fetch(url);
            let blob = response.blob();
            resolve(blob);
        });
    }

    blobToBase64(blob: Blob): Promise<string | ArrayBuffer>
    {
        return new Promise(resolve =>
        {
            let reader = new FileReader();
            reader.onload = function ()
            {
                let dataUrl = reader.result;
                resolve(dataUrl);
            };
            reader.readAsDataURL(blob);
        });
    }

    async getBase64Image(url: string): Promise<string | ArrayBuffer>
    {
        let blob = await this.getImageBlob(url);
        let base64 = await this.blobToBase64(blob);
        return base64;
    }

    setImage(url: string): void
    {
        if (url.startsWith('data:')) {
            $(this.elem).css({ 'background-image': 'url("' + url + '")' });
        } else {
            this.getBase64Image(url).then(base64Image =>
            {
                $(this.elem).css({ 'background-image': 'url("' + base64Image + '")' });
            });
        }
    }

    setSize(w: number, h: number)
    {
        $(this.elem).css({ 'width': w + 'px', 'height': h + 'px', 'left': -(w / 2) });
    }

    setCondition(condition: string): void
    {
        this.currentCondition = condition;
        this.startNextAnimation();
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

    onAnimations(data: AnimationsXml.AnimationsDefinition): void
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

        let once = true;
        let group = this.currentAction;
        this.currentAction = '';
        if (group == '') { group = this.currentCondition; once = false; }
        if (group == '') { group = this.currentState; once = false; }
        if (group == '') { group = this.defaultGroup; }

        let animation = this.getAnimationByGroup(group);
        if (!animation) {
            group = this.defaultGroup;
            animation = this.getAnimationByGroup(group);
            if (!animation) {
                return;
            }
        }

        let durationSec: number = animation.duration / 1000;
        if (durationSec < 0.1) {
            durationSec = 1.0;
        }

        // this.currentSpeedPixelPerSec = Math.abs(animation.dx) / durationSec;
        // dx means pixels per sec, not pixels per duration
        this.currentSpeedPixelPerSec = Math.abs(animation.dx) / 1.0;

        this.setImage(animation.url);

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
