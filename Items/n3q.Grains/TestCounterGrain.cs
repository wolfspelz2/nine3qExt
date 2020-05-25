using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using n3q.GrainInterfaces;
using n3q.StorageProviders;

namespace n3q.Grains
{
    [Serializable]
    public class TestCounterState
    {
        public long Value;
    }

    public class TestCounterGrain : Grain, ITestCounter
    {
        private long _value = 0;

        private readonly IPersistentState<TestCounterState> _state;

        public TestCounterGrain(
            [PersistentState("TestCounter", JsonFileStorage.StorageProviderName)] IPersistentState<TestCounterState> state
            )
        {
            _state = state;
        }

        public async Task<long> Get()
        {
            _value++;

            {
                _state.State.Value = _value;
                await _state.WriteStateAsync();
            }

            return _value;
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            await _state.ReadStateAsync();
            _value = _state.State.Value;
        }
    }
}