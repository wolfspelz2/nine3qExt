export class ItemProperties { [pid: string]: string }

export class Pid
{
    static readonly Id = 'Id';
    static readonly IsRezzed = 'IsRezzed';
    static readonly RezzedX = 'RezzedX';
    static readonly RezzedLocation = 'RezzedLocation';
    static readonly RezzedDestination = 'RezzedDestination';
    static readonly InventoryX = 'InventoryX';
    static readonly InventoryY = 'InventoryY';
    static readonly Provider = 'Provider';
    static readonly IframeAspect = 'IframeAspect';
    static readonly IframeWidth = 'IframeWidth';
    static readonly IframeHeight = 'IframeHeight';
    static readonly IframeResizable = 'IframeResizable';
    static readonly IframeFrame = 'IframeFrame';
    static readonly IframeUrl = 'IframeUrl';
    static readonly TransferState = 'TransferState';
    static readonly ImageUrl = 'ImageUrl';
    static readonly AnimationsUrl = 'AnimationsUrl';
    static readonly Width = 'Width';
    static readonly Height = 'Height';
    static readonly ApplierAspect = 'ApplierAspect';

    static readonly TransferState_Source = 'Source';
    static readonly TransferState_Destination = 'Destination';
}

interface PropertyDefinition
{
    inPresence: boolean;
}

export class Property
{
    private static config: { [pid: string]: PropertyDefinition } = {
        [Pid.Id]: { inPresence: true },
        [Pid.Provider]: { inPresence: true },
        [Pid.ImageUrl]: { inPresence: true },
        [Pid.AnimationsUrl]: { inPresence: true },
        [Pid.Width]: { inPresence: true },
        [Pid.Height]: { inPresence: true },
        [Pid.RezzedX]: { inPresence: true },
    };

    static inPresence(pid: string): boolean
    {
        if (this.config[pid]) {
            return this.config[pid].inPresence;
        }
        return false;
    }
}
