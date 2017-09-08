using System.ComponentModel;
using Epi.Common.Attributes;
using Epi.Common.Configuration;

namespace Epi.Common.EmailServices.Constants
{
    public static class EmailAppSettings
    {
        public struct Key
        {
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
            [EncryptedValue(true)]
            public const string EmailFrom = "EMAIL_FROM";

            // No Default Value
            [EncryptedValue(true)]
            public const string EmailUserName = "EMAIL_USERNAME";

            // No Default Value
            [EncryptedValue(true)]
            public const string EmailPassword = "EMAIL_PASSWORD";

            #endregion EMail Settings
        }

        #region Helper Functions

        static ConfigurationAttributesHelper AttributeHelper = new ConfigurationAttributesHelper(typeof(Key));

        public static bool IsValueEncrypted(string key)
        {
            return AttributeHelper.IsValueEncrypted(key);
        }

        public static bool GetBoolValue(this string key)
        {
            return AttributeHelper.GetBoolValue(key);
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
