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
import { RoomItem } from './RoomItem';
import { Participant } from './Participant';

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

    private ignoreNextDragFlag: boolean = false;
    ignoreDrag(): void { this.ignoreNextDragFlag = true; }

    isDefaultAvatar(): boolean { return this.isDefault; }

    constructor(protected app: ContentApp, private entity: Entity, private isSelf: boolean)
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

            if (ev.ctrlKey) {
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
            } else {
                this.entity.onMouseClickAvatar(ev);
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
                if (this.ignoreNextDragFlag) {
                    this.ignoreNextDragFlag = false;
                    return false;
                }

                this.entity.onDragAvatar(ev, ui);
            },
            stop: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                if (this.ignoreNextDragFlag) {
                    this.ignoreNextDragFlag = false;
                } else {
                    this.entity.onDragAvatarStop(ev, ui);
                }

                this.inDrag = false;
                this.app.enableScreen(false);
                $(this.elem).css('z-index', '');

                this.hackSuppressNextClickOtherwiseDraggableClicks = true;
                setTimeout(() => { this.hackSuppressNextClickOtherwiseDraggableClicks = false; }, 200);
            }
        });
    }

    makeDroppable(): void
    {
        $(this.elem).droppable({
            hoverClass: 'n3q-avatar-drophilite',
            drop: async (ev: JQueryEventObject, ui: JQueryUI.DroppableEventUIParam) =>
            {
                let droppedElem = ui.draggable.get(0);
                let droppedRoomItem = this.getRoomItemByAvatarElem(droppedElem);

                if (droppedRoomItem) {
                    let droppedAvatar = droppedRoomItem.getAvatar();

                    let thisRoomItem = this.getRoomItemByAvatarElem(this.elem);
                    if (thisRoomItem) {
                        droppedAvatar?.ignoreDrag();
                        this.app.getRoom().applyItemToItem(thisRoomItem, droppedRoomItem);
                    } else {
                        droppedAvatar?.ignoreDrag();

                        let itemId = droppedRoomItem.getNick();
                        let response = await BackgroundMessage.isBackpackItem(itemId);
                        if (response.ok && response.isItem) {
                            let thisParticipant = this.getParticipantByAvatarElem(this.elem);
                            this.app.getRoom().applyItemToParticipant(thisParticipant, droppedRoomItem);
                        }
                    }
                }
            }
        });
    }

    getRoomItemByAvatarElem(avatarElem: HTMLElement): RoomItem
    {
        let avatarEntityId = this.getEntityIdByAvatarElem(avatarElem);
        if (avatarEntityId) {
            return this.app.getRoom().getItem(avatarEntityId);
        }
    }

    getParticipantByAvatarElem(avatarElem: HTMLElement): Participant
    {
        let avatarEntityId = this.getEntityIdByAvatarElem(avatarElem);
        if (avatarEntityId) {
            return this.app.getRoom().getParticipant(avatarEntityId);
        }
    }

    getEntityIdByAvatarElem(avatarElem: HTMLElement): string
    {
        if (avatarElem) {
            let avatarEntityElem = avatarElem.parentElement;
            if (avatarEntityElem) {
                return $(avatarEntityElem).data('nick');
            }
        }
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

    async getDataUrlImage(imageUrl: string): Promise<string>
    {
        let proxiedUrl = as.String(Config.get('avatars.dataUrlProxyUrlTemplate', 'https://avatar.weblin.sui.li/avatar/?url={url}')).replace('{url}', encodeURIComponent(imageUrl));
        return new Promise(async (resolve, reject) =>
        {
            try {
                let response = await BackgroundMessage.fetchUrl(proxiedUrl, '');
                if (response.ok) {
                    resolve(response.data);
                }
            } catch (error) {
                reject(error);
            }
        });
    }

    setImage(url: string): void
    {
        if (url.startsWith('data:')) {
            $(this.elem).css({ 'background-image': 'url("' + url + '")' });
        } else {
            try {
                this.getDataUrlImage(url).then(dataUrlImage =>
                {
                    $(this.elem).css({ 'background-image': 'url("' + dataUrlImage + '")' });
                });
            } catch (error) {
                $(this.elem).css({ 'background-image': 'url("' + url + '")' });
            }
        }
    }

    setSize(width: number, height: number)
    {
        $(this.elem).css({ 'width': width + 'px', 'height': height + 'px', 'left': -(width / 2) });
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
                let width = as.Int(parsed.params['width'], -1);
                let height = as.Int(parsed.params['height'], -1);
                if (width > 0 && height > 0) {
                    this.setSize(width, height);
                }
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
