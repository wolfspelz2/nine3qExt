namespace nine3q.Items
{
    public class PropertyValue
    {
        public enum AccessLevel
        {
            Owner = 0,
            Team,
            Group,
            Department,
            Organization,
            Holding,
            Alliance,
            Everyone,

            ModeMultiplier,
        }

        public enum AccessMode
        {
            Read = 0,
            Write,
            Execute,
        }

        public enum Access
        {
            NoAccess = 0,
            ReadOwner = 1 << (AccessLevel.Owner + AccessMode.Read * AccessLevel.ModeMultiplier),
            ReadTeam = 1 << (AccessLevel.Group + AccessMode.Read * AccessLevel.ModeMultiplier),
            ReadGroup = 1 << (AccessLevel.Group + AccessMode.Read * AccessLevel.ModeMultiplier),
            ReadDepartment = 1 << (AccessLevel.Department + AccessMode.Read * AccessLevel.ModeMultiplier),
            ReadOrganization = 1 << (AccessLevel.Organization + AccessMode.Read * AccessLevel.ModeMultiplier),
            ReadHolding = 1 << (AccessLevel.Holding + AccessMode.Read * AccessLevel.ModeMultiplier),
            ReadAlliance = 1 << (AccessLevel.Alliance + AccessMode.Read * AccessLevel.ModeMultiplier),
            ReadEveryone = 1 << (AccessLevel.Everyone + AccessMode.Read * AccessLevel.ModeMultiplier),

            WriteOwner = 1 << (AccessLevel.Owner + AccessMode.Write * AccessLevel.ModeMultiplier),
            WriteTeam = 1 << (AccessLevel.Group + AccessMode.Write * AccessLevel.ModeMultiplier),
            WriteGroup = 1 << (AccessLevel.Group + AccessMode.Write * AccessLevel.ModeMultiplier),
            WriteDepartment = 1 << (AccessLevel.Department + AccessMode.Write * AccessLevel.ModeMultiplier),
            WriteOrganization = 1 << (AccessLevel.Organization + AccessMode.Write * AccessLevel.ModeMultiplier),
            WriteHolding = 1 << (AccessLevel.Holding + AccessMode.Write * AccessLevel.ModeMultiplier),
            WriteAlliance = 1 << (AccessLevel.Alliance + AccessMode.Write * AccessLevel.ModeMultiplier),
            WriteEveryone = 1 << (AccessLevel.Everyone + AccessMode.Write * AccessLevel.ModeMultiplier),

            ExecuteOwner = 1 << (AccessLevel.Owner + AccessMode.Execute * AccessLevel.ModeMultiplier),
            ExecuteTeam = 1 << (AccessLevel.Group + AccessMode.Execute * AccessLevel.ModeMultiplier),
            ExecuteGroup = 1 << (AccessLevel.Group + AccessMode.Execute * AccessLevel.ModeMultiplier),
            ExecuteDepartment = 1 << (AccessLevel.Department + AccessMode.Execute * AccessLevel.ModeMultiplier),
            ExecuteOrganization = 1 << (AccessLevel.Organization + AccessMode.Execute * AccessLevel.ModeMultiplier),
            ExecuteHolding = 1 << (AccessLevel.Holding + AccessMode.Execute * AccessLevel.ModeMultiplier),
            ExecuteAlliance = 1 << (AccessLevel.Alliance + AccessMode.Execute * AccessLevel.ModeMultiplier),
            ExecuteEveryone = 1 << (AccessLevel.Everyone + AccessMode.Execute * AccessLevel.ModeMultiplier),
        }

        public enum TransferState
        {
            Unknown,
            Source,
            Destination,
        }

        public enum PasswordAlgorithm
        {
            SaltedPasswordSha256,
        }

        public enum Roles
        {
            Public,
            User,
            Moderator,
            LeadModerator,
            Janitor,
            LeadJanitor,
            Content,
            LeadContent,
            Admin,
            Developer,
            SecurityAdmin
        }

        public enum TestEnum
        {
            Unknown,
            Value1,
            Value2,
        }

    }
}
