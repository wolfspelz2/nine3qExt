using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using n3q.GrainInterfaces;
using n3q.StorageProviders;

namespace n3q.Grains
{
    public class TestCounterGrain : Grain, ITestCounter
    {
        private readonly IPersistentState<KeyValueStorageData> _state;
        const string VALUE = "Expires";

        public long Value
        {
            get { return _state.State.Get(VALUE, 0L); }
            set { _state.State[VALUE] = value; }
        }

        public TestCounterGrain(
            [PersistentState("TestCounter", ItemAzureTableStorage.StorageProviderName)] IPersistentState<KeyValueStorageData> state
            )
        {
            _state = state;
        }

        public async Task<long> Get()
        {
            Value++;

            await _state.WriteStateAsync();

            return Value;
        }
    }
}