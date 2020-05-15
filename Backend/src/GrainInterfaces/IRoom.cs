using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace GrainInterfaces
{
    public class RoomStream
    {
        public const string Provider = "SMSProvider";
        public const string NamespaceProperties = "Properties";
        public const string NamespaceEvents = "Events";
    }

    public class RoomEvent
    {
        public enum Type { Nop, Rez }
        public Type type = Type.Nop;
        public string roomId;
        public long itemId;

        public RoomEvent(Type type, string roomId, long itemId)
        {
            this.type = type;
            this.roomId = roomId;
            this.itemId = itemId;
        }
    }

    public interface IRoom : IGrainWithStringKey
    {
        Task<Guid> GetStreamId();
        Task<long> ReceiveItem(long itemId, Dictionary<string, string> props);
        Task RezItem(long itemId);
        Task<string> GetItemProperty(long itemId, string key);
    }
}