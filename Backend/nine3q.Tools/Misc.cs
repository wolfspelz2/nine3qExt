using System;

namespace nine3q.Tools
{
    public static class Misc
    {
        // Compile, do not execute, but do not complain unreachable statement like "if (false)"
        //public delegate void DoCodeDelegate();
        //public delegate void DoCodeDelegate1(bool dummy);
        //public static void Dont(DoCodeDelegate d) { }
        //public static void Dont(DoCodeDelegate1 d) { }
        //public static void Do(DoCodeDelegate d) { d(); }
        //public static void Do(DoCodeDelegate1 d) { d(true); }
        public static Action Dont { get; set; }

        // Encapsulate "may be null warning" and improve syntax
        public static T As<T>(this object obj) where T : class { return obj as T; }

        public static string GetMethodName(int level = 0, bool fullName = false)
        {
            var st = new System.Diagnostics.StackTrace();
            var sf = st.GetFrame(1 + level);
            var mb = sf.GetMethod();
            if (fullName) {
                return mb.DeclaringType.FullName + "." + mb.Name;
            }
            return mb.Name;
        }

        public static string GetCallerName()
        {
            return GetMethodName(2);
        }
    }
}
