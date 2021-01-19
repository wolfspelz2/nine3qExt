export class ItemChangeOptions
{
    skipPresenceUpdate?: boolean;
    skipContentNotification?: boolean;
    skipPersistentStorage?: boolean;

    static empty: ItemChangeOptions = new ItemChangeOptions();
}
