using System;
using System.Collections.Generic;
using System.Text;

namespace n3q.Items
{
    public class PidList : List<Pid>
    {
        public const PidList All = null;
        public static PidList Empty = new PidList();
        public static PidList Public { get; } = new PidList { Pid.PublicAccess };
        public static PidList Owner { get; } = new PidList { Pid.OwnerAccess };
    }
}
