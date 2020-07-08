namespace n3q.Aspects
{
    public class ItemReader : ItemStub
    {
        public ItemReader(IItemClient itemClient, ITransaction transaction = null) : base(itemClient, transaction) { }
    }
}
