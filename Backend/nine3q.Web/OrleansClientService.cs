using nine3q.Web;

namespace nine3q.Web
{
    public class OrleansClientService
    {
        public OrleansClientService(IOrleansClientSingletonInstance OrleansClientInstance)
        {
            SingletonInstanceOrleansClient = OrleansClientInstance;
        }

        public IOrleansClientSingletonInstance SingletonInstanceOrleansClient { get; }
    }
}