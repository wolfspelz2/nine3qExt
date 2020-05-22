using System;
using System.Collections.Generic;

namespace nine3q.Tools
{
    public static class ExceptionExtensions
    {
        public static List<string> GetMessages(this Exception _this)
        {
            var messages = new List<string>();
            var ex = _this;
            var previousMessage = "";
            while (ex is AggregateException) {
                ex = ex.InnerException;
            }
            while (ex != null) {
                if (ex.Message != previousMessage) {
                    previousMessage = ex.Message;
                    messages.Add(ex.Message);
                }
                ex = ex.InnerException;
            }
            return messages;
        }
    }
}
