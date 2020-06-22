using System.Collections.Generic;

namespace ConfigSharp
{
    public class SharpConfigurationBag : ConfigBag
    {
        public Dictionary<string, string> Data = new Dictionary<string, string>();
    }
}
