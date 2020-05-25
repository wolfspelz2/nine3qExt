using System;
using System.Threading.Tasks;
using n3q.GrainInterfaces;
using n3q.Items;

namespace n3q.Aspects
{
    public class ItemAspect
    {
        protected Item self;
        protected IItem MyGrain => Grain(self);
        protected string Id => self.Id;

        public IItem Grain(Item item)
        {
            return item.ClusterClient.GetGrain<IItem>(item.Id);
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
