using System.Collections.Generic;

namespace n3q.Web
{
    public class WebConfigProduction : WebConfig
    {
        public void Load()
        {
            ConfigSequence += " WebConfigProduction";
            AdminTokens = new List<string> { "lgAkQAHJvxSm36ddWaMt" };
        }
    }
}