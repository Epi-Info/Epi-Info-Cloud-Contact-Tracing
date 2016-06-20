using System.Configuration;

namespace Epi.Cloud.Common.Configuration
{
    public static class ConfigurationHelper
    {
        public static string GetEnvironmentResourceKey(string resourceName, string environmentKeyName = "Environment")
        {
            var environmentKey = ConfigurationManager.AppSettings[environmentKeyName];
            if (resourceName != null)
            {
                environmentKey = ConfigurationManager.AppSettings[environmentKeyName + resourceName] ?? environmentKey;
            }
            return string.IsNullOrWhiteSpace(environmentKey)? resourceName : resourceName + "@" + environmentKey ;
        }
    }
}
