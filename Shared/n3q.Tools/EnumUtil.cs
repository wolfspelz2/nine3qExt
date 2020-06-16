using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace n3q.Tools
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
