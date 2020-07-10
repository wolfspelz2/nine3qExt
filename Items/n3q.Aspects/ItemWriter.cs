using System.Threading.Tasks;
using n3q.Items;

namespace n3q.Aspects
{
    public class ItemWriter : ItemReader
    {
        public ItemWriter(IItemClient itemClient) : base(itemClient) { }
        public ItemWriter(IItemClient itemClient, ITransaction transaction) : base(itemClient, transaction) { }

        public async Task Modify(PropertySet modified, PidSet deleted) { AssertTransaction(); await Grain.ModifyProperties(modified, deleted, Transaction.Id); }
        public async Task AddToList(Pid pid, PropertyValue value) { AssertTransaction(); await Grain.AddToListProperty(pid, value, Transaction.Id); }
        public async Task RemoveFromList(Pid pid, PropertyValue value) { AssertTransaction(); await Grain.RemoveFromListProperty(pid, value, Transaction.Id); }

        public async Task Delete() { AssertTransaction(); await Grain.Delete(Transaction.Id); }

        public async Task Set(Pid pid, PropertyValue value) { AssertTransaction(); await Grain.ModifyProperties(new PropertySet(pid, value), PidSet.Empty, Transaction.Id); }
        public async Task Unset(Pid pid) { AssertTransaction(); await Grain.ModifyProperties(PropertySet.Empty, new PidSet { pid }, Transaction.Id); }
    }
}
