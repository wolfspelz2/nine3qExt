using n3q.Web;

namespace n3q.Web
{
    public class CommandlineService
    {
        public CommandlineService(ICommandlineSingletonInstance commandlineInstance)
        {
            SingletonInstanceCommandline = commandlineInstance;
        }

        public ICommandlineSingletonInstance SingletonInstanceCommandline { get; }
    }
}