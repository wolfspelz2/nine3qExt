import log = require('loglevel');
import { ItemProperties } from './ItemProperties';

export class BackpackAddItemData
{
    constructor(
        public id: string,
        public properties: ItemProperties
    ) { }
}

export class BackpackChangeItemData
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
    static type_onBackpackAddItem = 'onBackpackAddItem';
    static type_onBackpackChangeItem = 'onBackpackChangeItem';
    static type_onBackpackRemoveItem = 'onBackpackRemoveItem';
}
