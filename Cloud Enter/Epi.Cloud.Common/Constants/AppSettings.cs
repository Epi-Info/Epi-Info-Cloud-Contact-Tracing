using System.ComponentModel;
using Epi.Common.Configuration;

namespace Epi.Cloud.Common.Constants
{
    public static class AppSettings
    {
        public struct Key
        {
            // No Default Value
            public const string BaseURL = "BaseURL";

            // No Default Value
            public const string URL = "URL";

            #region EMail Body Message Text

            [DefaultValue("n/a")]
            public const string EpiInfoDownloadURL = "EPI_INFO_DOWNLOAD_URL";

            [DefaultValue("No")]
            public const string WindowAuthentication = "WINDOW_AUTHENTICATION";

            // No Default Value
            public const string EndpointAddress = "ENDPOINT_ADDRESS";

            // No Default Value
            public const string BindingProtocol = "BINDING_PROTOCOL";

            #endregion EMail Body Message Text

            #region Settings

            [DefaultValue(20)]
            public const string ResponsePageSize = "RESPONSE_PAGE_SIZE";

            [DefaultValue(10)]
            public const string MobileResponsePageSize = "RESPONSE_PAGE_SIZE_Mobile";

            [DefaultValue(true)]
            public const string SendEmailToAssignedUsers = "SEND_EMAIL_TO_ASSIGNED_USERS";

            [DefaultValue(false)]
            public const string IsDemoMode = "IsDemoMode";

            [DefaultValue("ProjectMetadataTemplate")]
            public const string MetadataBlogContainerName = "MetadataBlog.ContainerName";

            [DefaultValue(false)]
            public const string IsLocalReleaseBuild = "IsLocalReleaseBuild";

            // No Default Value
            public const string Environment = "Environment";

            // No Default Value
            public const string Environment_API = "Environment.API";

            // No Default Value
            public const string MetadataAccessServiceAPI = "MetadataAccessServiceAPI";

            #endregion Settings

            #region CosmosDB

            [DefaultValue("ResponseSnapshot")]
            public const string AttachmentId = "AttachmentId";

            #endregion CosmosDB

            #region Service Bus

            [DefaultValue("responseinfotopic")]
            public const string ServiceBusTopicName = "ServiceBusTopicName";

            [DefaultValue("ReadSurveyInfoSubscription")]
            public const string ServiceBusSubscriptionName = "ServiceBusSubscriptionName";

            #endregion Service Bus
        }

        #region Helper Functions

        static ConfigurationAttributesHelper AttributeHelper = new ConfigurationAttributesHelper(typeof(Key));

        public static bool IsValueEncrypted(string key)
        {
            return AttributeHelper.IsValueEncrypted(key);
        }

        public static bool GetBoolValue(this string key)
        {
            return AttributeHelper.IsValueEncrypted(key);
        }

        public static int GetIntValue(this string key)
        {
            return AttributeHelper.GetIntValue(key);
        }

        public static string GetStringValue(this string key, bool decryptIfEncrypted = true)
        {
            return AttributeHelper.GetStringValue(key, decryptIfEncrypted);
        }

        #endregion Helper Functions
    }
}