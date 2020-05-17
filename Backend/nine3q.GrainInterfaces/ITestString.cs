using System;
using System.Threading.Tasks;
using Orleans;

namespace nine3q.GrainInterfaces
{
    public class TestStringStream
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