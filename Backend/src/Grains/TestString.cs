using System;
using System.Threading.Tasks;
using Orleans;
using Nine3Q.GrainInterfaces;

namespace Nine3Q.Grains
{
    public class TestString : Grain, ITestString
    {
        private string _data = "";

        private readonly Guid _streamId = Guid.NewGuid();
        public Task<Guid> GetStreamId() { return Task.FromResult(_streamId); }

        public async Task Set(string value)
        {
            _data = value;

            var streamProvider = GetStreamProvider(StringCacheStream.Provider);
            var stream = streamProvider.GetStream<string>(_streamId, StringCacheStream.Namespace);
            await stream.OnNextAsync(_data);
        }

        public async Task<string> Get()
        {
            await Task.CompletedTask;
            return _data;
        }
    }
}