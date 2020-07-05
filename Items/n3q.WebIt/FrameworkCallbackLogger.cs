using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using n3q.Tools;

namespace n3q.Web
{
    public class FrameworkCallbackLogger : ICallbackLogger
    {
        public ILogger _frameworkLogger;

        public FrameworkCallbackLogger(ILogger frameworkLogger)
        {
            _frameworkLogger = frameworkLogger;
        }

        internal enum Level
        {
            Silent,
            Error,
            Warning,
            Debug,
            User,
            Info,
            Verbose,
            Flooding,
        }

        public void Error(Exception ex, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.Error, GetContext(context, callerFilePath), "Exception: " + ExceptionDetail(ex)); } catch (Exception) { } }
        public void Warning(Exception ex, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.Warning, GetContext(context, callerFilePath), "Exception: " + ExceptionDetail(ex)); } catch (Exception) { } }
        public void Error(string message, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.Error, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        public void Warning(string message, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.Warning, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        public void Debug(string message, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.Debug, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        public void User(string message, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.User, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        public void Info(string message, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.Info, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        public void Verbose(string message, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.Verbose, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        public void Flooding(string message, [CallerMemberName] string context = null, [CallerFilePath] string callerFilePath = null) { try { DoLog(Level.Flooding, GetContext(context, callerFilePath), message); } catch (Exception) { } }

        public bool IsVerbose() { return true; }
        public bool IsFlooding() { return true; }

        internal void DoLog(Level level, string context, string message)
        {
            switch (level) {
                case Level.Silent:
                    break;
                case Level.Error:
                    _frameworkLogger.LogError($"{context} {message}");
                    break;
                case Level.Warning:
                    _frameworkLogger.LogWarning($"{context} {message}");
                    break;
                case Level.Debug:
                    _frameworkLogger.LogDebug($"{context} {message}");
                    break;
                case Level.User:
                    _frameworkLogger.LogInformation($"{context} {message}");
                    break;
                case Level.Info:
                    _frameworkLogger.LogInformation($"{context} {message}");
                    break;
                case Level.Verbose:
                    _frameworkLogger.LogInformation($"{context} {message}");
                    break;
                case Level.Flooding:
                    _frameworkLogger.LogTrace($"{context} {message}");
                    break;
            }
        }

        private static string MethodName(int skip = 0)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1 + skip);
            if (sf != null) {
                var mb = sf.GetMethod();
                if (mb != null && mb.DeclaringType != null) {
                    return mb.DeclaringType.FullName + "." + mb.Name;
                }
            }
            return "<unknown>";
        }

        private static string GetContext(string context, string callerFilePath)
        {
            if (context == null) {
                context = MethodName(3);
            }
            if (callerFilePath != null) {
                var guessedCallerTypeName = Path.GetFileNameWithoutExtension(callerFilePath);
                return guessedCallerTypeName + "." + context;
            }
            return null;
        }

        private static string ExceptionDetail(Exception ex)
        {
            return string.Join(" | ", AllExceptionMessages(ex).ToArray()) + " | " + string.Join(" | ", InnerExceptionDetail(ex).ToArray());
        }

        private static List<string> AllExceptionMessages(Exception self)
        {
            var result = new List<string>();

            var ex = self;
            var previousMessage = "";
            while (ex != null) {
                if (ex.Message != previousMessage) {
                    previousMessage = ex.Message;
                    result.Add(ex.Message);
                }
                ex = ex.InnerException;
            }

            return result;
        }

        private static List<string> InnerExceptionDetail(Exception self)
        {
            var result = new List<string>();

            var ex = self;
            if (self.InnerException != null) {
                ex = self.InnerException;
            }

            if (ex.Source != null) { result.Add("Source: " + ex.Source); }
            if (ex.StackTrace != null) { result.Add("Stack Trace: " + ex.StackTrace.Replace(Environment.NewLine, "\\n")); }
            if (ex.TargetSite != null) { result.Add("TargetSite: " + ex.TargetSite); }

            return result;
        }

    }
}