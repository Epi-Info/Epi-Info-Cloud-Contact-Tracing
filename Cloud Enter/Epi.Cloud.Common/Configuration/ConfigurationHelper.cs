using System.Configuration;
using Epi.Cloud.Common.Constants;

namespace Epi.Cloud.Common.Configuration
{
    public static class ConfigurationHelper
    {
        public static string GetEnvironmentResourceKey(string resourceName, string environmentKeyName = AppSettings.Key.Environment)
        {
            var environmentKey = AppSettings.GetStringValue(environmentKeyName);
            if (resourceName != null)
            {
                environmentKey = ConfigurationManager.AppSettings[environmentKeyName + '/' + resourceName] ?? environmentKey;
            }
            return string.IsNullOrWhiteSpace(environmentKey)? resourceName : resourceName + "@" + environmentKey ;
        }
    }
}
