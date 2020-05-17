using System;

namespace nine3q.Items
{
    public class DummyTimerManager : ITimerManager
    {
        void ITimerManager.StartTimer(long id, string timer, TimeSpan interval)
        {
        }

        void ITimerManager.CancelTimer(long id, string timer)
        {
        }
    }
}
