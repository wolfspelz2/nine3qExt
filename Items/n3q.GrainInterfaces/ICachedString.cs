using System;
using System.Threading.Tasks;
using Orleans;

namespace n3q.GrainInterfaces
{
    public interface ICachedString : IGrainWithStringKey
    {
        Task Set(string s, long timeout = CachedStringOptions.Timeout.Infinite, CachedStringOptions.Persistence persistence = CachedStringOptions.Persistence.Transient);
        Task<string> Get();
        Task Unset();

        Task SetTime(DateTime time);
        Task DeletePersistentStorage();
        Task ReloadPersistentStorage();
    }

    public static class CachedStringOptions
    {
        public static class Timeout
        {
            public const long Infinite = -1;
        }

        public enum Persistence
        {
            Transient,
            Persistent,
        }
    }
}
