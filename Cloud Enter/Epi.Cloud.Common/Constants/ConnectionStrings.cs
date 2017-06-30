using Epi.Cloud.Common.Attributes;
using Epi.Cloud.Common.Configuration;

namespace Epi.Cloud.Common.Constants
{
    public static class ConnectionStrings
    {
        static ConfigurationAttributesHelper AttributeHelper = new ConfigurationAttributesHelper(typeof(Key));

        public struct Key
        {
            [EncryptedValue(true)]
            public const string EWEEntities = "EWEEntities";

            [EncryptedValue(true)]
            public const string EWEADO = "EWEADO";

            [EncryptedValue(true)]
            public const string EPIInfo7Entities = "EPIInfo7Entities";

            [EncryptedValue(true)]
            public const string DBConnection = "DBConnection";

            [EncryptedValue(true)]
            public const string CacheConnectionString = "CacheConnectionString";

            [EncryptedValue(true)]
            public const string CollectedDataConnectionString = "CollectedDataConnectionString";

            [EncryptedValue(true)]
            public const string ServiceBusConnectionString = "ServiceBusConnectionString";

            [EncryptedValue(true)]
            public const string MetadataBlobStorage = "MetadataBlobStorage.ConnectionString";
        }

        public static bool IsValueEncrypted(string key)
        {
            return AttributeHelper.IsValueEncrypted(key);
        }

        public static string GetConnectionString(this string key, bool decryptIfEncrypted = true)
        {
            return AttributeHelper.GetConnectionString(key, decryptIfEncrypted);
        }
    }
}