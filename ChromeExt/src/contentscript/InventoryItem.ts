import * as $ from 'jquery';
import { xml, jid } from '@xmpp/client';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Inventory } from './Inventory';

import imgDefaultItem from '../assets/DefaultIcon.png';

export class InventoryItem
{
    private isFirstPresence: boolean = true;
    private properties: { [pid: string]: string } = {};
    private elem: HTMLDivElement;
    private iconElem: HTMLImageElement;
    private x: number;
    private y: number;

    getPseudoRandomCoordinate(space: number, size: number, padding: number, id: string, mod: number): number
    {
        let result = 50;
        let hash = Utils.hash(id) % mod;
        let min = size / 2 + padding;
        let max = space - min;
        result = min + (max - min) / mod * hash;
        return result;
    }

    constructor(app: ContentApp, private inv: Inventory, private id: string)
    {
        let iconSize: number = Config.get('inventory.iconSize', 32);
        let borderPadding: number = Config.get('inventory.borderPadding', 4);

        let paneElem = this.inv.getDisplay();
        let paneWidth = paneElem.offsetWidth;
        let paneHeight = paneElem.offsetHeight;

        this.x = this.getPseudoRandomCoordinate(
            paneElem.offsetWidth,
            Config.get('inventory.iconSize', 32),
            Config.get('inventory.borderPadding', 4),
            id,
            3456);
        this.y = this.getPseudoRandomCoordinate(
            paneElem.offsetWidth,
            Config.get('inventory.iconSize', 32),
            Config.get('inventory.borderPadding', 4),
            id,
            9851);

        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-inventory-item" />').get(0);
        this.iconElem = <HTMLImageElement>$('<img class="n3q-base n3q-item-icon" />').get(0);
        $(this.elem).append(this.iconElem);

        var url = imgDefaultItem;
        this.iconElem.src = url;

        $(this.elem).css({ 'left': (this.x - iconSize / 2) + 'px', 'top': (this.y - iconSize / 2) + 'px', 'width': iconSize + 'px', 'height': iconSize + 'px' });

        paneElem.appendChild(this.elem);
    }

    remove(): void
    {
    }

    // presence

    onPresenceAvailable(stanza: any): void
    {
        let newProperties: { [pid: string]: string } = {};

        let vpPropsNode = stanza.getChildren('x').find(stanzaChild => (stanzaChild.attrs == null) ? false : stanzaChild.attrs.xmlns === 'vp:props');
        if (vpPropsNode) {
            let attrs = vpPropsNode.attrs;
            if (attrs) {
                let providerId = as.String(attrs.provider, '');
                for (let attrName in attrs) {
                    let attrValue = attrs[attrName];
                    if (attrName.endsWith('Url')) {
                        attrValue = ContentApp.itemProviderUrlFilter(providerId, attrName, attrValue);
                    }
                    newProperties[attrName] = attrValue;
                }
            }
        }

        if (newProperties.Icon32Url) {
            this.iconElem.src = newProperties.Icon32Url;
        }

        if (this.isFirstPresence) {

        }

        this.properties = newProperties;

        this.isFirstPresence = false;
    }

    onPresenceUnavailable(stanza: any): void
    {
        this.remove();
    }
}
