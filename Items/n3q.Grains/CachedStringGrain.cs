using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using n3q.GrainInterfaces;
using n3q.StorageProviders;
using n3q.Tools;

namespace n3q.Grains
{
    //public static class CachedStringState
    //{
    //    [Serializable]
    //    public class CachedString
    //    {
    //        public string Data;
    //        public long Expires;
    //    }
    //}

    public class CachedStringGrain : Grain, ICachedString
    {
        //private readonly IPersistentState<CachedStringState.CachedString> _state;
        readonly IPersistentState<KeyValueStorageData> _state;
        const string DATA = "Data";
        const string EXPIRES = "Expires";

        public string Data
        {
            get { return _state.State.Get(DATA, null); }
            set { _state.State[DATA] = value; }
        }

        public long Expires
        {
            get { return _state.State.Get(EXPIRES, 0L); }
            set { _state.State[EXPIRES] = value; }
        }

        public DateTime Time { get; set; } = DateTime.MinValue;
        public CachedStringOptions.Persistence Persistence { get; set; } = CachedStringOptions.Persistence.Transient;

        public CachedStringGrain(
            [PersistentState("CachedString", AzureKeyValueTableStorage.StorageProviderName)] IPersistentState<KeyValueStorageData> state
            )
        {
            _state = state;
        }

        #region Interface

        public async Task Set(string data, long timeout = CachedStringOptions.Timeout.Infinite, CachedStringOptions.Persistence persistence = CachedStringOptions.Persistence.Transient)
        {
            Data = data;
            Persistence = persistence;

            if (timeout == CachedStringOptions.Timeout.Infinite) {
                Expires = 0;
            } else {
                Expires = GetCurrentTime().AddSeconds(timeout).ToLong();
            }

            if (Persistence == CachedStringOptions.Persistence.Persistent) {
                await _state.WriteStateAsync();
            }
        }

        public async Task<string> Get()
        {
            string result = null;

            if (Data != null) {
                if (Expires == 0) {
                    result = Data;
                } else {
                    if (GetCurrentTime() < new DateTime().FromLong(Expires)) {
                        result = Data;
                    } else {
                        if (Persistence == CachedStringOptions.Persistence.Persistent) {
                            await _state.ClearStateAsync();
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Internal

        DateTime GetCurrentTime()
        {
            return (Time == DateTime.MinValue) ? DateTime.UtcNow : Time;
        }

        #endregion

        #region For tests

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        public async Task ReloadPersistentStorage()
        {
            await _state.ReadStateAsync();
        }

        public Task Deactivate()
        {
            base.DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            await _state.ReadStateAsync();

            if (!string.IsNullOrEmpty(Data)) {
                Persistence = CachedStringOptions.Persistence.Persistent;
            }
        }

        public async Task Unset()
        {
            bool wasSet = (Data != null);

            Data = null;
            Expires = 0;

            await _state.ClearStateAsync();
        }

        public Task SetTime(DateTime time)
        {
            Time = time;
            return Task.CompletedTask;
        }

        #endregion
    }
}