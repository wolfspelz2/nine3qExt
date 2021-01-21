import log = require('loglevel');
import {ItemProperties} from './ItemProperties';
import {ContentApp} from "../contentscript/ContentApp";

export class BackpackShowItemData {
    constructor(public id: string, public properties: ItemProperties) {
    }
}

export class BackpackSetItemData {
    constructor(public id: string, public properties: ItemProperties) {
    }
}

export class BackpackRemoveItemData {
    constructor(public id: string,) {
    }
}

export class ContentMessage {
    static content: ContentApp;

    static sendMessage(tabId: number, message: any): void
    {
        if (ContentMessage.content) {
            ContentMessage.content.onDirectRuntimeMessage(message);
        } else {
            chrome.tabs.sendMessage(tabId, message);
        }
    }
}

export namespace ContentMessage {
    export enum Type {
        recvStanza,
        userSettingsChanged,
        onBackpackShowItem,
        onBackpackSetItem,
        onBackpackHideItem,
        sendPresence,
    }
}

