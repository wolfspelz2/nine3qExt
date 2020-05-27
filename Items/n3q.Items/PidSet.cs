using System;
using System.Collections.Generic;
using System.Text;

namespace n3q.Items
{
    public class PidSet : HashSet<Pid>
    {
        public const PidSet All = null;
        public static PidSet Empty = new PidSet();
        public static PidSet Public { get; } = new PidSet { Pid.PublicAccess };
        public static PidSet Owner { get; } = new PidSet { Pid.OwnerAccess };
    }
}
