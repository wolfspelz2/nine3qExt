using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XmppComponent
{
    public static class AsyncUtilityExtension
    {
        public static void PerformAsyncTaskWithoutAwait(this Task task, Action<Task> exceptionHandler)
        {
            var dummy = task?.ContinueWith(t => exceptionHandler(t), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }
    }
}
