using System;

namespace nine3q.Items
{
    public interface ITimerManager
    {
        void StartTimer(long id, string timer, TimeSpan interval);
        void CancelTimer(long id, string timer);
    }
}
