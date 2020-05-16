using System;

namespace nine3q.Items
{
    public interface ITimerManager
    {
        void StartTimer(ItemId id, string timer, TimeSpan interval);
        void CancelTimer(ItemId id, string timer);
    }
}
