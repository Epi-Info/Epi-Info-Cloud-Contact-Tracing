using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Epi.Cloud.Common.Constants
{
    public static class AppSettings
    {

        public struct Key
        {
            public const string BaseURL = "BaseURL";
            public const string URL = "URL";

            #region EMail Settings
            [DefaultValue(true)]
            public const string EmailUseAuthentication = "EMAIL_USE_AUTHENTICATION";

            [DefaultValue(true)]
            public const string EmailSubject = "EMAIL_SUBJECT";
            public const string EmailFrom = "EMAIL_FROM";
            public const string EmailPassword = "EMAIL_PASSWORD";
            public const string SmtpPort = "SMTP_PORT";
            #endregion EMail Settings

            #region EMail Body Message Text
            [DefaultValue("n/a")]
            public const string EpiInfoDownloadURL = "EPI_INFO_DOWNLOAD_URL";

            [DefaultValue("No")]
            public const string WindowAuthentication = "WINDOW_AUTHENTICATION";

            public const string EndpointAddress = "ENDPOINT_ADDRESS";
            public const string BindingProtocol = "BINDING_PROTOCOL";
            #endregion EMail Body Message Text

            [DefaultValue(50)]
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

            public const string Environment = "Environment";
            public const string Environment_API = "Environment.API";
            public const string MetadataAccessServiceAPI = "MetadataAccessServiceAPI";

            #region Security
            public const string KeyForUserPasswordSalt = "KeyForUserPasswordSalt";
            public const string KeyForConnectionStringPassPhrase = "KeyForConnectionStringPassphrase";
            public const string KeyForConnectionStringSalt = "KeyForConnectionStringSalt";
            public const string KeyForConnectionStringVector = "KeyForConnectionStringVector";
            #endregion Security

            #region DocumentDB
            [DefaultValue("ResponseSnapshot")]
            public const string AttachmentId = "AttachmentId";
            #endregion DocumentDB
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

        public static string GetStringValue(this string key)
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