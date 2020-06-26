using System;
using System.Runtime.CompilerServices;

namespace n3q.Tools
{
    public interface ICallbackLogger
    {
        void Error(Exception ex, [CallerMemberName] string context = null, string callerFilePath = null);
        void Error(string message, [CallerMemberName] string context = null, string callerFilePath = null);
        void Warning(Exception ex, [CallerMemberName] string context = null, string callerFilePath = null);
        void Warning(string message, [CallerMemberName] string context = null, string callerFilePath = null);
        void Debug(string message, [CallerMemberName] string context = null, string callerFilePath = null);
        void User(string message, [CallerMemberName] string context = null, string callerFilePath = null);
        void Info(string message, [CallerMemberName] string context = null, string callerFilePath = null);
        void Verbose(string message, [CallerMemberName] string context = null, string callerFilePath = null);
        void Flooding(string message, [CallerMemberName] string context = null, string callerFilePath = null);
        bool IsVerbose();
        bool IsFlooding();
    }

    public interface ICallbackLogger<T> : ICallbackLogger
    {
    }

    public class NullCallbackLogger : ICallbackLogger
    {
        public void Debug(string message, string context = null, string callerFilePath = null)  { }
        public void Error(Exception ex, string context = null, string callerFilePath = null)  { }
        public void Error(string message, string context = null, string callerFilePath = null)  { }
        public void Flooding(string message, string context = null, string callerFilePath = null)  { }
        public void Info(string message, string context = null, string callerFilePath = null)  { }
        public void User(string message, string context = null, string callerFilePath = null)  { }
        public void Verbose(string message, string context = null, string callerFilePath = null)  { }
        public void Warning(Exception ex, string context = null, string callerFilePath = null)  { }
        public void Warning(string message, string context = null, string callerFilePath = null)  { }
        public bool IsFlooding() { return false; }
        public bool IsVerbose() { return false; }
    }
}
