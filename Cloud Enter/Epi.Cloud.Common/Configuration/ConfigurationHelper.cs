using System.Configuration;

namespace Epi.Cloud.Common.Configuration
{
    public static class ConfigurationHelper
    {
        public static string GetEnvironmentResourceKey(string resourceName)
        {
            var environmentKey = ConfigurationManager.AppSettings["Environment"];
            if (resourceName != null)
            {
                environmentKey = ConfigurationManager.AppSettings["Environment/" + resourceName] ?? environmentKey;
            }
            return string.IsNullOrWhiteSpace(environmentKey)? resourceName : resourceName + "@" + environmentKey ;
        }
    }
}
