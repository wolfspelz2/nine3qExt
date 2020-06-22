using System;
using Microsoft.Extensions.Configuration;

namespace ConfigSharp
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddSharpConfiguration(this IConfigurationBuilder configuration, Action<SharpConfigurationOptions> options)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            var sharpConfigurationOptions = new SharpConfigurationOptions();
            options(sharpConfigurationOptions);
            configuration.Add(new SharpConfigurationSource(sharpConfigurationOptions));
            return configuration;
        }
    }
}
