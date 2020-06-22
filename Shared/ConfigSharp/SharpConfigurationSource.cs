using Microsoft.Extensions.Configuration;

namespace ConfigSharp
{
    public class SharpConfigurationSource : IConfigurationSource
    {
        public string ConfigFile { get; set; }

        public SharpConfigurationSource(SharpConfigurationOptions options)
        {
            ConfigFile = options.ConfigFile;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SharpConfigurationProvider(this);
        }
    }
}
