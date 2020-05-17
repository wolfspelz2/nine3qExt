using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using nine3q.GrainInterfaces;
using nine3q.StorageProviders;

namespace nine3q.Grains
{
    public class TestStringState
    {
        [Serializable]
        public class TestString
        {
            public string Data;
        }
    }

    public class TestStringGrain : Grain, ITestString
    {
        private string _data = "";

        private readonly IPersistentState<TestStringState.TestString> _state;

        private readonly Guid _streamId = Guid.NewGuid();
        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }

        public TestStringGrain(
            [PersistentState("TestString", JsonFileStorage.StorageProviderName)] IPersistentState<TestStringState.TestString> state
            )
        {
            _state = state;
        }

        public async Task Set(string value)
        {
            _data = value;

            {
                var streamProvider = GetStreamProvider(TestStringStream.Provider);
                var stream = streamProvider.GetStream<string>(_streamId, TestStringStream.Namespace);
                await stream.OnNextAsync(_data);
            }

            {
                _state.State.Data = _data;
                await _state.WriteStateAsync();
            }
        }

        public async Task<string> Get()
        {
            await Task.CompletedTask;

            return _data;
        }

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }
    }
}