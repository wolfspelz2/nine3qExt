import log = require('loglevel');
import { ItemProperties } from './ItemProperties';

export class BackpackShowItemData
{
    constructor(public id: string, public properties: ItemProperties) { }
}

export class BackpackSetItemData
{
    constructor(public id: string, public properties: ItemProperties) { }
}

export class BackpackRemoveItemData
{
    constructor(public id: string,) { }
}

export class ContentMessage
{
}

export namespace ContentMessage
{
    export enum Type
    {
        recvStanza,
        userSettingsChanged,
        onBackpackShowItem,
        onBackpackSetItem,
        onBackpackHideItem,
        sendPresence,
    }
}
