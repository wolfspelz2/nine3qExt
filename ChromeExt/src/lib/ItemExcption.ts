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
        NotDerezzed
    }


    export enum Reason
    {
        UnknownReason,
        ItemAlreadyRezzed,
        ItemNotRezzedHere,
        NoItemsAvailable
    }
}

