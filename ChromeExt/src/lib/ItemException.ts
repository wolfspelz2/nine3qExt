export class ItemException
{
    constructor(public fact: ItemException.Fact, public reason: ItemException.Reason, public detail: string = null)
    {
    }

    static factFromString(s: string): ItemException.Fact
    {
        let o: object = ItemException.Fact;
        if (o[s]) { return o[s]; }
        return ItemException.Fact.Error;
    }

    static reasonFromString(s: string): ItemException.Reason
    {
        let o: object = ItemException.Reason;
        if (o[s]) { return o[s]; }
        return ItemException.Reason.UnknownReason;
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
        ClaimFailed,
        NotTransferred
    }


    export enum Reason
    {
        UnknownReason,
        ItemAlreadyRezzed,
        ItemNotRezzedHere,
        ItemsNotAvailable,
        ItemDoesNotExist,
        NoUserId,
        SeeDetail,
        NotYourItem,
        ItemMustBeStronger,
        ItemIsNotTransferable,
        InternalError,
        ItemIsNotRezable,
        NotStarted,
        ItemCapacityLimit,
        ServiceUnavailable,
        ItemIsNotMovable,
        ItemDepleted,
        IdenticalItems,
        StillInCooldown,
        MissingPropertyValue,
        NoSuchItem,
        InvalidItemAddress,
        NoSuchTemplate,
        TransferFailed,
        InvalidCommandArgument,
        NoSuchAspect,
        InvalidPropertyValue,
        AccessDenied,
        NoMatch,
        NoSuchProperty,
        InvalidArgument,
        InvalidSignature,
        PropertyMismatch,
        Ambiguous,
        Insufficient,
        StillInProgress,
        MissingResource,
    }
}

