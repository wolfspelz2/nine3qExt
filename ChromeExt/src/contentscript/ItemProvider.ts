export class ItemProvider
{
    constructor(private config: any)
    {
    }

    getConfig(key: string, defaultValue: any): any
    {
        if (this.config[key]) {
            return this.config[key];
        }
        return defaultValue;
    }

    propertyUrlFilter(propValue: string): string
    {
        let propertyUrlFilter = this.getConfig('itemPropertyUrlFilter', null);
        if (propertyUrlFilter) {
            for (let key in propertyUrlFilter) {
                let value = propertyUrlFilter[key];
                if (key && value) {
                    propValue = propValue.replace(key, value);
                }
            }
        }
        return propValue;
    }

}
