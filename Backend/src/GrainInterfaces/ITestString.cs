using System;
using System.Threading.Tasks;
using Orleans;

namespace Nine3Q.GrainInterfaces
{
    public class StringCacheStream
    {
        public const string Provider = "SMSProvider";
        public const string Namespace = "Value";
    }

    public interface ITestString : IGrainWithStringKey
    {
        Task Set(string value);
        Task<string> Get();
        Task<Guid> GetStreamId();
    }
}