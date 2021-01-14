import log = require('loglevel');
import { ItemProperties } from './ItemProperties';

export class BackpackShowItemData
{
    constructor(
        public id: string,
        public properties: ItemProperties
    ) { }
}

export class BackpackSetItemData
{
    constructor(
        public id: string,
        public properties: ItemProperties
    ) { }
}

export class BackpackRemoveItemData
{
    constructor(
        public id: string,
    ) { }
}

export class ContentMessage
{
    static type_recvStanza = 'recvStanza';
    static type_userSettingsChanged = 'userSettingsChanged';
    static type_onBackpackShowItem = 'onBackpackShowItem';
    static type_onBackpackSetItem = 'onBackpackSetItem';
    static type_onBackpackHideItem = 'onBackpackHideItem';
    static type_sendPresence = 'sendPresence';
}
