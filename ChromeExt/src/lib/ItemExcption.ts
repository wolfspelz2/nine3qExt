export class ItemException
{
    constructor(public fact: ItemException.Fact, public reason: ItemException.Reason, public detail: string = null)
    {
    }
}

export namespace ItemException
{
    export enum Fact
    {
        Error,
        NotRezzed,
        NotDerezzed,
        NotAdded,
        NotChanged,
        NoItemsReceived,
        NotExecuted
    }

    export enum Reason
    {
        UnknownReason,
        ItemAlreadyRezzed,
        ItemNotRezzedHere,
        ItemsNotAvailable,
        ItemDoesNotExist,
        NoUserToken,
        SeeDetail
    }
}

