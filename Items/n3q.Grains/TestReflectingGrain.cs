using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using n3q.GrainInterfaces;
using n3q.StorageProviders;

namespace n3q.Grains
{
    [Serializable]
    public class TestReflectingState
    {
        public string StringField;
        public long LongField;
        public double DoubleField;
        public bool BoolField;
    }

    public class TestReflectingGrain : Grain, ITestReflecting
    {
        private readonly IPersistentState<TestReflectingState> _state;

        public TestReflectingGrain(
            [PersistentState("TestReflecting", ReflectingAzureTableStorage.StorageProviderName)] IPersistentState<TestReflectingState> state
            )
        {
            _state = state;
        }

        public async Task SetString(string value) { _state.State.StringField = value; await _state.WriteStateAsync(); }
        public async Task SetLong(long value) { _state.State.LongField = value; await _state.WriteStateAsync(); }
        public async Task SetDouble(double value) { _state.State.DoubleField = value; await _state.WriteStateAsync(); }
        public async Task SetBool(bool value) { _state.State.BoolField = value; await _state.WriteStateAsync(); }

        public Task<string> GetString() { return Task.FromResult(_state.State.StringField); }
        public Task<long> GetLong() { return Task.FromResult(_state.State.LongField); }
        public Task<double> GetDouble() { return Task.FromResult(_state.State.DoubleField); }
        public Task<bool> GetBool() { return Task.FromResult(_state.State.BoolField); }

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        public Task Deactivate()
        {
            base.DeactivateOnIdle();
            return Task.CompletedTask;
        }
    }
}