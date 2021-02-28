export class ItemProperties { [pid: string]: string }
export class ItemPropertiesSet { [id: string]: ItemProperties }

export class Pid
{
    static readonly Id = 'Id';
    static readonly Label = 'Label';
    static readonly Description = 'Description';
    static readonly Template: 'Template';
    static readonly OwnerId = 'OwnerId';
    static readonly OwnerName = 'OwnerName';
    static readonly IsRezable = 'IsRezable';
    static readonly IsTransferable = 'IsTransferable';
    static readonly IsRezzed = 'IsRezzed';
    static readonly RezzedX = 'RezzedX';
    static readonly RezzedLocation = 'RezzedLocation';
    static readonly RezzedDestination = 'RezzedDestination';
    static readonly InventoryX = 'InventoryX';
    static readonly InventoryY = 'InventoryY';
    static readonly Provider = 'Provider';
    static readonly Stats = 'Stats';
    static readonly IframeAspect = 'IframeAspect';
    static readonly IframeOptions = 'IframeOptions';
    static readonly IframeUrl = 'IframeUrl';
    static readonly DocumentOptions = 'DocumentOptions';
    static readonly DocumentUrl = 'DocumentUrl';
    static readonly DocumentText = 'DocumentText';
    static readonly DocumentTitle = 'DocumentTitle';
    static readonly MigrationAspect = 'MigrationAspect';
    static readonly MigrationCid = 'MigrationCid';
    static readonly IframeAuto = 'IframeAuto';
    static readonly TransferState = 'TransferState';
    static readonly ImageUrl = 'ImageUrl';
    static readonly AnimationsUrl = 'AnimationsUrl';
    static readonly Width = 'Width';
    static readonly Height = 'Height';
    static readonly ApplierAspect = 'ApplierAspect';
    static readonly ClaimAspect = 'ClaimAspect';
    static readonly ClaimStrength = 'ClaimStrength';
    static readonly ClaimUrl = 'ClaimUrl';
    static readonly PointsAspect = 'PointsAspect';
    static readonly SettingsAspect = 'SettingsAspect';
    static readonly AvatarAspect = 'AvatarAspect';
    static readonly NicknameAspect = 'NicknameAspect';
    static readonly NicknameText = 'NicknameText';
    static readonly AvatarAvatarId = 'AvatarAvatarId';
    static readonly AvatarAnimationsUrl = 'AvatarAnimationsUrl';
    static readonly PointsChannelEntered = 'PointsChannelEntered';
    static readonly PointsChannelChat = 'PointsChannelChat';
    static readonly PointsChannelEmote = 'PointsChannelEmote';
    static readonly PointsChannelGreet = 'PointsChannelGreet';
    static readonly PointsChannelNavigation = 'PointsChannelNavigation';
    static readonly PointsChannelItemRez = 'PointsChannelItemRez';
    static readonly PointsChannelItemApply = 'PointsChannelItemApply';
    static readonly PointsTotal = 'PointsTotal';
    static readonly ScreenAspect = 'ScreenAspect';
    static readonly ScreenOptions = 'ScreenOptions';
    static readonly ScreenUrl = 'ScreenUrl';
    static readonly DeactivatableIsInactive = 'DeactivatableIsInactive';
    static readonly Signed = 'Signed';
    static readonly SignatureRsa = 'SignatureRsa';

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
        [Pid.Label]: { inPresence: true },
        [Pid.Description]: { inPresence: true },
        [Pid.OwnerId]: { inPresence: true },
        [Pid.OwnerName]: { inPresence: true },
        [Pid.Provider]: { inPresence: true },
        [Pid.ImageUrl]: { inPresence: true },
        [Pid.AnimationsUrl]: { inPresence: true },
        [Pid.Width]: { inPresence: true },
        [Pid.Height]: { inPresence: true },
        [Pid.RezzedX]: { inPresence: true },
        [Pid.ClaimAspect]: { inPresence: true },
        [Pid.ClaimStrength]: { inPresence: true },
        [Pid.ClaimUrl]: { inPresence: true },
        [Pid.IframeAspect]: { inPresence: true },
        [Pid.IframeOptions]: { inPresence: true },
        [Pid.IframeUrl]: { inPresence: true },
        [Pid.IframeAuto]: { inPresence: true },
        [Pid.DocumentOptions]: { inPresence: true },
        [Pid.DocumentUrl]: { inPresence: true },
        [Pid.DocumentTitle]: { inPresence: true },
        [Pid.DocumentText]: { inPresence: true },
        [Pid.ScreenAspect]: { inPresence: true },
        [Pid.ScreenOptions]: { inPresence: true },
        [Pid.ScreenUrl]: { inPresence: true },
        [Pid.Stats]: { inPresence: true },
        [Pid.Signed]: { inPresence: true },
        [Pid.SignatureRsa]: { inPresence: true },
        [Pid.DeactivatableIsInactive]: { inPresence: true },

        // For unit test
        ['Test1']: { inPresence: true },
        ['Test2']: { inPresence: true },
        ['Test3']: { inPresence: false },
        // ['Test4']: { inPresence: true },
    };

    static inPresence(pid: string): boolean
    {
        if (this.config[pid]) {
            if (this.config[pid].inPresence) {
                return this.config[pid].inPresence;
            }
        }
        return false;
    }
}
