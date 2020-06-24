namespace n3q.Web
{
    public class ConfigRoot : WebConfig
    {
        public void Load()
        {
            Data["GrainBName"] = "b";

            if (RunMode == RunModes.Development) {

                Data[nameof(WebConfig.AdminTokens)] = "Token";
                Data[nameof(WebConfig.WebBaseUrl)] = "http://localhost:5000/";

            } else {

                var serverAddress = "localhost";
                Data[nameof(WebConfig.WebBaseUrl)] = $"http://{serverAddress}/";

            }
        }
    }
}