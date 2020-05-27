using System;

namespace n3q.Tools
{
    public static class Don
    {
        // Compile & refactor, but do not execute and don't complain about unreachable statement like "if (false)"
        //public delegate void DoCodeDelegate();
        //public delegate void DoCodeDelegate1(bool dummy);
        //public static void Dont(DoCodeDelegate d) { }
        //public static void Dont(DoCodeDelegate1 d) { }
        //public static void Do(DoCodeDelegate d) { d(); }
        //public static void Do(DoCodeDelegate1 d) { d(true); }
        public static Action t { get; set; }
    }

    //public static class Do
    //{
    //    // Encapsulate "may be null warning" and improve syntax
    //    public static T As<T>(this object obj) where T : class { return obj as T; }
    //}
}
