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
import { BackpackItem } from './BackpackItem';

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
    private imageElem: HTMLImageElement;
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
    getElem(): HTMLElement { return this.elem; }

    private mousedownX: number;
    private mousedownY: number;

    constructor(protected app: ContentApp, private entity: Entity, private isSelf: boolean)
    {
        this.imageElem = <HTMLImageElement>$('<img class="n3q-base n3q-avatar-image" />').get(0);
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-avatar" />').get(0);
        $(this.elem).append(this.imageElem);

        // var url = 'https://www.virtual-presence.org/images/wolf.png';
        // var url = app.getAssetUrl('default-avatar.png');
        var url = entity.getDefaultAvatar();
        // this.elem.src = url;
        this.setImage(url);
        this.setSize(38, 38);
        this.isDefault = true;

        $(this.imageElem).on('mousedown', (ev: JQueryMouseEventObject) =>
        {
            this.mousedownX = ev.clientX;
            this.mousedownY = ev.clientY;
        });

        $(this.imageElem).on('click', (ev: JQueryMouseEventObject) =>
        {
            if (Math.abs(this.mousedownX - ev.clientX) > 2 || Math.abs(this.mousedownY - ev.clientY) > 2) {
                return;
            }

            let elem = this.elemBelowTransparentImageAtMouse(ev);
            if (elem) {
                let newEv = new jQuery.Event('click');
                newEv.clientX = ev.clientY;
                newEv.clientY = ev.clientY;
                $(elem).trigger('click', newEv);
                ev.stopPropagation();
            }
        });

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

        $(this.imageElem).draggable({
            scroll: false,
            stack: '.n3q-item',
            opacity: 0.5,
            distance: 4,
            // helper: 'clone',
            helper: () =>
            {
                let dragElem = $(this.elem).clone().get(0);
                let nick = Avatar.getEntityIdByAvatarElem(this.elem);
                $(dragElem).data('nick', nick);
                $(dragElem).detach();
                this.app.getDisplay().append(dragElem);
                this.app.toFront(dragElem, ContentApp.LayerDrag);
                return dragElem;
            },
            containment: 'document',
            start: (ev: JQueryMouseEventObject, ui: JQueryUI.DraggableEventUIParams) =>
            {
                let elem = this.elemBelowTransparentImageAtMouse(ev);
                if (elem) {
                    return false;
                }

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

                this.hackSuppressNextClickOtherwiseDraggableClicks = true;
                setTimeout(() => { this.hackSuppressNextClickOtherwiseDraggableClicks = false; }, 200);
            }
        });
    }

    elemBelowTransparentImageAtMouse(ev: JQueryMouseEventObject): Element
    {
        if (typeof ev.pageX === 'undefined') { return null; }

        let elemBelow: Element = null;
        let self = this.imageElem;
        let canvasElem = document.createElement('canvas');
        let ctx = canvasElem.getContext('2d');

        // Get click coordinates
        let x = ev.pageX - $(self).offset().left;
        let y = ev.pageY - $(self).offset().top;
        let w = ctx.canvas.width = $(self).width();
        let h = ctx.canvas.height = $(self).height();

        // Draw image to canvas
        // and read Alpha channel value
        ctx.drawImage(self, 0, 0, w, h);
        let alpha = ctx.getImageData(x, y, 1, 1).data[3]; // [0]R [1]G [2]B [3]A

        $(canvasElem).remove();

        // If pixel is transparent, retrieve the element underneath
        if (alpha === 0) {
            let imagePointerEvents = self.style.pointerEvents;
            let parentPointerEvents = self.parentElement.style.pointerEvents;
            self.style.pointerEvents = 'none';
            self.parentElement.style.pointerEvents = 'none';
            elemBelow = document.elementFromPoint(ev.clientX, ev.clientY);
            self.style.pointerEvents = imagePointerEvents;
            self.parentElement.style.pointerEvents = parentPointerEvents;
        }

        return elemBelow;
    }

    addClass(className: string): void
    {
        $(this.imageElem).addClass(className);
    }

    makeDroppable(): void
    {
        $(this.elem).droppable({
            hoverClass: 'n3q-avatar-drophilite',
            accept: (draggable) =>
            {
                if (draggable[0]) { draggable = draggable[0]; }
                if ($(draggable).hasClass('n3q-avatar-image')) {
                    if (Avatar.getEntityIdByAvatarElem(draggable) != Avatar.getEntityIdByAvatarElem(this.getElem())) {
                        return true;
                    }
                }
            },
            drop: async (ev: JQueryEventObject, ui: JQueryUI.DroppableEventUIParam) =>
            {
                let droppedElem = ui.draggable.get(0);
                let droppedRoomItem = this.getRoomItemByDomElem(droppedElem);

                if (droppedRoomItem) {
                    let droppedAvatar = droppedRoomItem.getAvatar();

                    let thisRoomItem = this.getRoomItemByDomElem(this.elem);
                    if (thisRoomItem) {
                        droppedAvatar?.ignoreDrag();
                        this.app.getRoom().applyItemToItem(thisRoomItem, droppedRoomItem);
                    } else {
                        droppedAvatar?.ignoreDrag();

                        let itemId = droppedRoomItem.getRoomNick();
                        if (await BackgroundMessage.isBackpackItem(itemId)) {
                            let thisParticipant = this.getParticipantByAvatarElem(this.elem);
                            this.app.getRoom().applyItemToParticipant(thisParticipant, droppedRoomItem);
                        }
                    }
                } else {
                    let droppedBackpackItem = this.getBackpackItemByDomElem(droppedElem);
                }
            }
        });
    }

    getBackpackItemByDomElem(elem: HTMLElement): BackpackItem
    {
        let itemId = $(elem).data('id');
        if (itemId) {
            return this.app.getBackpackWindow()?.getItem(itemId);
        }
    }

    getRoomItemByDomElem(elem: HTMLElement): RoomItem
    {
        let avatarEntityId = Avatar.getEntityIdByAvatarElem(elem);
        if (avatarEntityId) {
            return this.app.getRoom().getItem(avatarEntityId);
        }
    }

    getParticipantByAvatarElem(elem: HTMLElement): Participant
    {
        let avatarEntityId = Avatar.getEntityIdByAvatarElem(elem);
        if (avatarEntityId) {
            return this.app.getRoom().getParticipant(avatarEntityId);
        }
    }

    static getEntityIdByAvatarElem(elem: HTMLElement): string
    {
        if (elem) {
            let nick = $(elem).data('nick');
            if (nick) { if (nick != '') { return nick; } }

            let avatarElem = elem.parentElement;
            if (avatarElem) {
                if ($(avatarElem).hasClass('n3q-entity')) {
                    return $(avatarElem).data('nick');
                } else {
                    let avatarEntityElem = avatarElem.parentElement;
                    if (avatarEntityElem) {
                        return $(avatarEntityElem).data('nick');
                    }
                }
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
            $(this.imageElem).addClass('n3q-avatar-hilite');
        } else {
            $(this.imageElem).removeClass('n3q-avatar-hilite');
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
        let proxiedUrl = as.String(Config.get('avatars.dataUrlProxyUrlTemplate', 'https://webex.vulcan.weblin.com/Avatar/DataUrl?url={url}')).replace('{url}', encodeURIComponent(imageUrl));
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
            $(this.imageElem).attr('src', url);
        } else {
            try {
                this.getDataUrlImage(url).then(dataUrlImage =>
                {
                    $(this.imageElem).attr('src', dataUrlImage);
                });
            } catch (error) {
                $(this.imageElem).attr('src', url);
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
        let newSpeedPixelPerSec = Math.abs(animation.dx) / 1.0;
        if (newSpeedPixelPerSec > 0) {
            this.currentSpeedPixelPerSec = Math.abs(animation.dx) / 1.0;
        }

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
