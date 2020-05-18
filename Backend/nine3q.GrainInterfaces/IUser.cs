using System;
using System.Threading.Tasks;
using Orleans;

namespace nine3q.GrainInterfaces
{
    public class UserStream
    {
        public const string Provider = "SMSProvider";
        public const string Namespace = "Properties";
    }

    public interface IUser : IGrainWithStringKey
    {
        Task<Guid> GetStreamId();
        Task DropItem(long itemId, string roomId, long posX, string destinationUrl);
    }
}