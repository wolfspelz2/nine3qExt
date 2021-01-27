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
        NotDeleted,
        NotChanged,
        NoItemsReceived,
        NotExecuted,
        NotApplied,
        ClaimFailed
    }




    export enum Reason
    {
        UnknownReason,
        ItemAlreadyRezzed,
        ItemNotRezzedHere,
        ItemsNotAvailable,
        ItemDoesNotExist,
        NoUserToken,
        SeeDetail,
        NotYourItem,
        ItemMustBeStronger
    }
}

