using System.Configuration;
using Epi.Cloud.Common.Constants;
using Epi.Common.Security;
namespace Epi.Cloud.Common.Configuration
{
    public static class ConfigurationHelper
    {
        public static string GetEnvironmentResourceKey(string resourceName, string environmentKeyName = AppSettings.Key.Environment, bool isEncrypt = true)
        {
            var environmentKey = AppSettings.GetStringValue(environmentKeyName);
            if (resourceName != null)
            {
                environmentKey = ConfigurationManager.AppSettings[environmentKeyName + '/' + resourceName] ?? environmentKey;
                var connectionStringName = string.IsNullOrWhiteSpace(environmentKey) ? resourceName : resourceName + "@" + environmentKey;
                var ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
                if (isEncrypt)
                {
                    var DecryptConnectionString = Cryptography.Decrypt(ConnectionString);
                    return DecryptConnectionString;
                }
                else
                {
                    return ConnectionString;
                }
            }
            return string.IsNullOrWhiteSpace(environmentKey) ? resourceName : resourceName + "@" + environmentKey;
        }
        public static string GetConnectionString(string resourceName)
        {

            if (resourceName != null)
            {
                var ConnectionString = ConfigurationManager.ConnectionStrings[resourceName].ConnectionString;
                return ConnectionString;
            }
            return null;
        }

        public static string GetEnvironmentValueByKey(string resourceName)
        {
            if (resourceName != null)
            {
                var EnvironmentValue = ConfigurationManager.AppSettings[resourceName];
                return EnvironmentValue;
            }
            return null;
        }

        public static string GetValueByResourceKey(string resourceName, string environmentKeyName = AppSettings.Key.Environment, bool isEncrypt = true)
        {
            var environmentKey = AppSettings.GetStringValue(environmentKeyName);
            if (resourceName != null)
            {
                environmentKey = ConfigurationManager.AppSettings[environmentKeyName + '/' + resourceName] ?? environmentKey;
                var Key = string.IsNullOrWhiteSpace(environmentKey) ? resourceName : resourceName + "@" + environmentKey;
                var Value = ConfigurationManager.AppSettings[Key];
                return Value;
            }
            return null;
        }

        public static string GetConnectionStringByResourceKey(string environmentKeyName, bool isEncrypt = true)
        {

            if (environmentKeyName != null)
            {
                var ConnectionString = ConfigurationManager.ConnectionStrings[environmentKeyName].ConnectionString;
                if (isEncrypt)
                {
                    var DecryptConnectionString = Cryptography.Decrypt(ConnectionString);
                    return DecryptConnectionString;
                }
                else
                {
                    return ConnectionString;
                }
            }
            return null;

        }
    }
}
