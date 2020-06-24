using System.Collections.Generic;

namespace n3q.Web
{
    public class ConfigRoot : WebConfig
    {
        public void Load()
        {
            if (RunMode == RunModes.Development) {

                AdminTokens = new List<string> { "Token" };
                WebBaseUrl = "http://localhost:5000/";

            } else {

                var serverAddress = "localhost";
                WebBaseUrl = $"http://{serverAddress}/";

            }
        }
    }
}