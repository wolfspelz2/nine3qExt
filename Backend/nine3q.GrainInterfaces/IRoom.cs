using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace nine3q.GrainInterfaces
{
    public static class RoomStream
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
        public long long;

        public RoomEvent(Type type, string roomId, long long)
        {
            this.type = type;
            this.roomId = roomId;
            this.long = long;
        }
    }

    public interface IRoom : IGrainWithStringKey
    {
        Task<Guid> GetStreamId();
        Task<long> ReceiveItem(long long, Dictionary<string, string> props);
        Task RezItem(long long);
        Task<string> GetItemProperty(long long, string key);
    }
}