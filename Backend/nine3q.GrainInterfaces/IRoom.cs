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
        public enum Type { Nop, RezItem,
            DerezItem
        }
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

        Task RezItem(long itemId, long posX, string destinationUrl);
        Task OnItemRezzed(long itemId);

        Task DerezItem(long itemId);
        Task OnItemDerezzed(long itemId);
    }
}