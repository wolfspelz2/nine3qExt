using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using n3q.GrainInterfaces;
using n3q.StorageProviders;
using n3q.Tools;

namespace n3q.Grains
{
    [Serializable]
    public class CachedStringState
    {
        public string Data;
        public long Expires;
    }

    public class CachedStringGrain : Grain, ICachedString
    {
        readonly IPersistentState<CachedStringState> _state;

        public string Data
        {
            get { return _state.State.Data; }
            set { _state.State.Data = value; }
        }

        public long Expires
        {
            get { return _state.State.Expires; }
            set { _state.State.Expires = value; }
        }

        public DateTime Time { get; set; } = DateTime.MinValue;
        public CachedStringOptions.Persistence Persistence { get; set; } = CachedStringOptions.Persistence.Transient;

        public CachedStringGrain(
            [PersistentState("CachedString", AzureReflectingTableStorage.StorageProviderName)] IPersistentState<CachedStringState> state
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