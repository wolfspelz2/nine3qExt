using System;
using System.Threading.Tasks;
using Orleans;
using n3q.GrainInterfaces;
using n3q.Items;

namespace n3q.Aspects
{
    public class Aspect
    {
        protected Item self;
        protected IItem MyGrain => Grain(self);
        protected string Id => self.Id;
        protected IClusterClient Client => self.ClusterClient;

        public IItem Grain(Item item)
        {
            return item.ClusterClient.GetGrain<IItem>(item.Id);
        }

        protected Item GetItem(string itemId)
        {
            return new Item(Client, itemId);
        }

        protected async Task<bool> IsAspect(Pid pid)
        {
            return await MyGrain.GetBool(pid);
        }

        protected async Task AssertAspect(Pid pid)
        {
            if (!await IsAspect(pid)) {
                throw new Exception($"Item={self.Id} has no {pid.ToString()}");
            }
        }

        public static ContainerAspect Container(Item item)
        {
            return new ContainerAspect(item);
        }
    }
}
