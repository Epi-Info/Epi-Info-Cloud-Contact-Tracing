using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using Epi.Cloud.Common.Attributes;
using Epi.Common.Security;

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
            var encryptedAttribute = FindEncryptedAttribute(key);
            var isEncrypted = encryptedAttribute.IsEncrypted;

            return isEncrypted;
        }

        public static bool GetBoolValue(this string key)
        {
            bool value = false;
            try
            {
                var stringValue = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(stringValue) || !bool.TryParse(stringValue, out value))
                {
                    return ReturnDefaultBoolValue(key);
                }
                else
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                return ReturnDefaultBoolValue(key, ex);
            }
        }

        public static string GetStringValue(this string key, bool decryptIfEncrypted = true)
        {
            string value = string.Empty;
            try
            {
                value = ConfigurationManager.AppSettings[key];
                if (value == null)
                {
                    return ReturnDefaultStringValue(key);
                }
                else
                {
                    if (decryptIfEncrypted && IsValueEncrypted(key))
                    {
                        try
                        {
                            value = Cryptography.Decrypt(value);
                        }
                        catch (Exception ex)
                        {
                            // If an exception occurrs assume that the value is not encrypted;
                            _encryptedValueAttributes[key] = EncryptedValueAttribute.False;
                        }
                    }
                    return value;
                }
            }
            catch (Exception ex)
            {
                return ReturnDefaultStringValue(key, ex);
            }
        }

        public static int GetIntValue(this string key)
        {
            int value = 0;
            try
            {
                var stringValue = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(stringValue) || !Int32.TryParse(stringValue, out value))
                {
                    return ReturnDefaultIntValue(key);
                }
                else
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                return ReturnDefaultIntValue(key, ex);
            }
        }

        private static System.Reflection.FieldInfo[] _fields;

        private static Dictionary<string, EncryptedValueAttribute> _encryptedValueAttributes = new Dictionary<string, EncryptedValueAttribute>();

        private static EncryptedValueAttribute FindEncryptedAttribute(string key)
        {
            EncryptedValueAttribute encryptedValueAttribute = EncryptedValueAttribute.False;
            if (!_encryptedValueAttributes.TryGetValue(key, out encryptedValueAttribute))
            {
                _fields = _fields ?? typeof(Key).GetFields();
                var field = _fields.Where(f => f.GetRawConstantValue().ToString() == key).SingleOrDefault();
                if (field != null)
                {
                    encryptedValueAttribute = field.GetCustomAttributes(false).Where(a => a.GetType() == typeof(EncryptedValueAttribute)).FirstOrDefault() as EncryptedValueAttribute;
                }
                encryptedValueAttribute = encryptedValueAttribute ?? EncryptedValueAttribute.False;
                _encryptedValueAttributes.Add(key, encryptedValueAttribute);
            }
            return encryptedValueAttribute;
        }


        private static DefaultValueAttribute FindDefaultValueAttribute(string key)
        {
            DefaultValueAttribute defaultValueAttribute = null;

            _fields = _fields ?? typeof(Key).GetFields();

            var field = _fields.Where(f => f.GetRawConstantValue().ToString() == key).SingleOrDefault();
            if (field != null)
            {
                defaultValueAttribute = field.GetCustomAttributes(false).Where(a => a.GetType() == typeof(DefaultValueAttribute)).FirstOrDefault() as DefaultValueAttribute;
            }
            return defaultValueAttribute;
        }

        private static bool ReturnDefaultBoolValue(string key, Exception ex = null)
        {
            var defaultValueAttribute = FindDefaultValueAttribute(key);

            if (defaultValueAttribute != null)
            {
                var defaultValue = defaultValueAttribute.Value as bool?;
                if (defaultValue.HasValue) return defaultValue.Value;
            }

            throw new SettingsPropertyNotFoundException(key, ex);
        }

        private static string ReturnDefaultStringValue(string key, Exception ex = null)
        {
            var defaultValueAttribute = FindDefaultValueAttribute(key);

            if (defaultValueAttribute != null)
            {
                var defaultValue = defaultValueAttribute.Value.ToString();
                if (defaultValue != null) return defaultValue;
            }

            throw new SettingsPropertyNotFoundException(key, ex);
        }

        private static Int32 ReturnDefaultIntValue(string key, Exception ex = null)
        {
            var defaultValueAttribute = FindDefaultValueAttribute(key);

            if (defaultValueAttribute != null)
            {
                var defaultValue = defaultValueAttribute.Value as Int32?;
                if (defaultValue.HasValue) return defaultValue.Value;
            }

            throw new SettingsPropertyNotFoundException(key, ex);
        }
    }
}