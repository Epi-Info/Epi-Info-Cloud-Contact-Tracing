using System.Configuration;
using Epi.Cloud.Common.Constants;
using Epi.Common.Security;
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
                var connectionStringName = string.IsNullOrWhiteSpace(environmentKey) ? resourceName : resourceName + "@" + environmentKey;
                var ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
                var DecryptConnectionString = Cryptography.Decrypt(ConnectionString);
                return DecryptConnectionString;
            }
            return string.IsNullOrWhiteSpace(environmentKey) ? resourceName : resourceName + "@" + environmentKey;



        }
    }
}
