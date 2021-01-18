export class ItemChangeOptions
{
    skipPresenceUpdate?: boolean;
    skipContentNotification?: boolean;

    static empty: ItemChangeOptions = new ItemChangeOptions();
}
