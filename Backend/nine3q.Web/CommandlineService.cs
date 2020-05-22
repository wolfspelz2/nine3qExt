using nine3q.Web;

namespace nine3q.Web
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