using System;

namespace nine3q.Items
{
    public class DummyTimerManager : ITimerManager
    {
        void ITimerManager.StartTimer(ItemId id, string timer, TimeSpan interval)
        {
        }

        void ITimerManager.CancelTimer(ItemId id, string timer)
        {
        }
    }
}
