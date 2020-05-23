using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using nine3q.GrainInterfaces;
using nine3q.StorageProviders;
using nine3q.Tools;

namespace nine3q.Grains
{
    public class CachedStringState
    {
        [Serializable]
        public class CachedString
        {
            public string Data;
            public long Expires;
        }
    }

    public class CachedStringGrain : Grain, ICachedString
    {
        private readonly IPersistentState<CachedStringState.CachedString> _state;

        public DateTime Time { get; set; } = DateTime.MinValue;
        public CachedStringOptions.Persistence Persistence { get; set; } = CachedStringOptions.Persistence.Transient;

        public CachedStringGrain(
            [PersistentState("CachedString", JsonFileStorage.StorageProviderName)] IPersistentState<CachedStringState.CachedString> state
            )
        {
            _state = state;
        }

        #region Interface

        public async Task Set(string data, long timeout = CachedStringOptions.Timeout.Infinite, CachedStringOptions.Persistence persistence = CachedStringOptions.Persistence.Transient)
        {
            _state.State.Data = data;
            Persistence = persistence;

            if (timeout == CachedStringOptions.Timeout.Infinite) {
                _state.State.Expires = 0;
            } else {
                _state.State.Expires = GetCurrentTime().AddSeconds(timeout).ToLong();
            }

            if (Persistence == CachedStringOptions.Persistence.Persistent) {
                await _state.WriteStateAsync();
            }
        }

        public async Task<string> Get()
        {
            string result = null;

            if (_state.State.Data != null) {
                if (_state.State.Expires == 0) {
                    result = _state.State.Data;
                } else {
                    if (GetCurrentTime() < new DateTime().FromLong(_state.State.Expires)) {
                        result = _state.State.Data;
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

            if (!string.IsNullOrEmpty(_state.State.Data)) {
                Persistence = CachedStringOptions.Persistence.Persistent;
            }
        }

        public async Task Unset()
        {
            bool wasSet = (_state.State.Data != null);

            _state.State.Data = null;
            _state.State.Expires = 0;

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