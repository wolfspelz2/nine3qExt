import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Item } from './Item';

export class ItemRepository
{
    private items: { [id: string]: Item; } = {};

    constructor(private app: ContentApp) 
    {
    }

    hasItem(id: string): boolean
    {
        return this.items[id] != null;
    }

    addItem(id: string, providerId: string, properties: any): void
    {
        this.items[id] = new Item(this.app, id);
        this.items[id].setProviderId(providerId);
        this.items[id].setProperties(properties);
    }

    getItem(id: string): Item
    {
        return this.items[id];
    }

    deleteItem(id: string): void
    {
        delete this.items[id];
    }

    setProperty(id: string, key: string, value: any): void
    {
        if (this.hasItem(id)) {
            let props = this.items[id].getProperties();
            if (props) {
                props[key] = value;
            }
        }
    }

    getProperty(id: string, key: string): any
    {
        if (this.hasItem(id)) {
            let props = this.items[id].getProperties();
            if (props) {
                return props[key];
            }
        }
        return null;
    }
}
