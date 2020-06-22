using Microsoft.Extensions.Configuration;

namespace ConfigSharp
{
    public class SharpConfigurationProvider : ConfigurationProvider
    {
        public SharpConfigurationSource Source { get; }

        public SharpConfigurationProvider(SharpConfigurationSource source)
        {
            Source = source;
        }

        public override void Load()
        {
            var bag = new SharpConfigurationBag();
            bag.Include(Source.ConfigFile);
            foreach (var pair in bag.Data) {
                Set(pair.Key, pair.Value);
            }
        }
    }
}
