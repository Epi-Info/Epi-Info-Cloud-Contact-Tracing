using System.ComponentModel;
using Epi.Cloud.Common.Attributes;
using Epi.Cloud.Common.Configuration;

namespace Epi.Cloud.Common.Constants
{
    public static class AppSettings
    {
        static ConfigurationAttributesHelper AttributeHelper = new ConfigurationAttributesHelper(typeof(Key));

        public struct Key
        {
            // No Default Value
            public const string BaseURL = "BaseURL";

            // No Default Value
            public const string URL = "URL";

            #region EMail Settings

            [DefaultValue(false)]
            public const string EmailUseAuthentication = "EMAIL_USE_AUTHENTICATION";

            // No Default Value
            public const string LoggingAdminEmailAddress = "LOGGING_ADMIN_EMAIL_ADDRESS";

            // No Default Value
            public const string EmailSubject = "EMAIL_SUBJECT";

            // No Default Value
            public const string LoggingEmailSubject = "LOGGING_EMAIL_SUBJECT";

            [DefaultValue(true)]
            public const string LoggingSendEmailNotification = "LOGGING_SEND_EMAIL_NOTIFICATION";

            [DefaultValue(false)]
            public const string EmailUseSSL = "EMAIL_USE_SSL";

            [EncryptedValue(true)]
            public const string SmtpHost = "SMTP_HOST";

            [DefaultValue(25)]
            public const string SmtpPort = "SMTP_PORT";

            // No Default Value
            public const string EmailFrom = "EMAIL_FROM";

            // No Default Value
            [EncryptedValue(true)]
            public const string EmailUserName = "EMAIL_USERNAME";

            // No Default Value
            [EncryptedValue(true)]
            public const string EmailPassword = "EMAIL_PASSWORD";

            #endregion EMail Settings

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

            #region Security

            // No Default Value
            public const string KeyForUserPasswordSalt = "KeyForUserPasswordSalt";

            // No Default Value
            public const string KeyForConnectionStringPassPhrase = "KeyForConnectionStringPassphrase";

            // No Default Value
            public const string KeyForConnectionStringSalt = "KeyForConnectionStringSalt";

            // No Default Value
            public const string KeyForConnectionStringVector = "KeyForConnectionStringVector";

            #endregion Security

            #region DocumentDB

            [DefaultValue("ResponseSnapshot")]
            public const string AttachmentId = "AttachmentId";

            #endregion DocumentDB

            #region Service Bus

            [DefaultValue("responseinfotopic")]
            public const string ServiceBusTopicName = "ServiceBusTopicName";

            [DefaultValue("ReadSurveyInfoSubscription")]
            public const string ServiceBusSubscriptionName = "ServiceBusSubscriptionName";

            #endregion Service Bus
        }

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
    }
}