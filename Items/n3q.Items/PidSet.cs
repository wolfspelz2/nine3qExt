using System;
using System.Collections.Generic;
using System.Text;

namespace n3q.Items
{
    public class PidSet : HashSet<Pid>
    {
        public const PidSet All = null;
        public static PidSet Empty = new PidSet();

        public PidSet()
        {
        }

        public PidSet(IEnumerable<Pid> pids) : base(pids)
        {
        }

        public static PidSet Public { get; } = new PidSet { Pid.MetaPublicAccess };
        public static PidSet Owner { get; } = new PidSet { Pid.MetaOwnerAccess };
        public static PidSet Aspects { get; } = new PidSet { Pid.MetaAspectGroup };
    }
}
