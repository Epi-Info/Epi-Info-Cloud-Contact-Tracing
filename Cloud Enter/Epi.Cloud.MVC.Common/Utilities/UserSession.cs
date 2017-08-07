using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Epi.Cloud.MVC.Constants;
using Epi.Common.Attributes;
using Epi.Common.Configuration;
using Epi.Common.Security;
using Newtonsoft.Json;

namespace Epi.Cloud.MVC.Constants
{
    public static class UserSession
    {
        public struct Key
        {
            public const string CurrentFormId = "CurrentFormId";
            public const string CurrentOrgId = "CurrentOrgId";
            public const string IsCurrentOrgHostOrg = "IsCurrentOrgHostOrg";
            public const string EditForm = "EditForm";
            public const string FormValuesHasChanged = "FormValuesHasChanged";
            public const string IsDemoMode = "IsDemoMode";
            public const string IsEditMode = "IsEditMode";
            public const string IsOwner = "IsOwner";
            public const string IsSqlProject = "IsSqlProject";
            public const string PageNumber = "PageNumber";
            public const string ProjectId = "ProjectId";
            public const string RecoverLastRecordVersion = "RecoverLastRecordVersion";
            public const string RelateButtonPageId = "RelateButtonPageId";
            public const string RequestedViewId = "RequestedViewId";
            public const string RequiredList = "RequiredList";
            public const string RootFormId = "RootFormId";
            public const string RootResponseId = "RootResponseId";
            public const string SearchCriteria = "SearchCriteria";
            public const string SearchModel = "SearchModel";
            public const string SelectedOrgId = "SelectedOrgId";
            public const string SortField = "SortField";
            public const string SortOrder = "SortOrder";
            public const string UGuid = "UGuid";
            public const string UserHighestRole = "UserHighestRole";
            public const string UserEmailAddress = "UserEmailAddress";
            public const string UserFirstName = "UserFirstName";

            [EncryptedValue(true)]
            public const string UserId = "UserId";

            public const string UserLastName = "UserLastName";
            public const string UsertRole = "UsertRole";
            public const string UserName = "UserName";
            public const string ResponseContext = "ResponseContext";
        }

        static ConfigurationAttributesHelper AttributeHelper = new ConfigurationAttributesHelper(typeof(UserSession.Key));

        static public bool IsValueEncrypted(string key)
        {
            return SessionHelper.IsValueEncrypted(key);
        }

        static public bool IsSessionValueNull(Controller controller, string key)
        {
            return controller.Session[key] == null;
        }

        static public bool GetBoolSessionValue(Controller controller, string key, bool? defaultValue = null, bool decryptIfEncrypted = true)
        {
            bool value = false;
            try
            {
                object sessionValue = controller.Session[key];
                if (sessionValue != null)
                {
                    if (sessionValue is bool)
                    {
                        return (bool)sessionValue;
                    }
                    else if (sessionValue is string)
                    {
                        var stringValue = SessionHelper.DecryptIfEncrypted(key, (string)sessionValue, decryptIfEncrypted);
                        return bool.TryParse(stringValue, out value) ? value : (defaultValue.HasValue ? defaultValue.Value : SessionHelper.ReturnDefaultBoolValue(key));
                    }
                }
                return defaultValue.HasValue ? defaultValue.Value : SessionHelper.ReturnDefaultBoolValue(key);
            }
            catch (Exception ex)
            {
                return SessionHelper.ReturnDefaultBoolValue(key, ex);
            }
        }

        static public int GetIntSessionValue(Controller controller, string key, int? defaultValue = null, bool decryptIfEncrypted = true)
        {
            int value = 0;
            try
            {
                object sessionValue = controller.Session[key];
                if (sessionValue != null)
                {
                    if (sessionValue is int)
                    {
                        return (int)sessionValue;
                    }
                    else if (sessionValue is string)
                    {
                        var stringValue = SessionHelper.DecryptIfEncrypted(key, (string)sessionValue, decryptIfEncrypted);
                        return int.TryParse(stringValue, out value) ? value : (defaultValue.HasValue ? defaultValue.Value : SessionHelper.ReturnDefaultIntValue(key));
                    }
                }

                return defaultValue.HasValue ? defaultValue.Value : SessionHelper.ReturnDefaultIntValue(key);
            }
            catch (Exception ex)
            {
                return SessionHelper.ReturnDefaultIntValue(key, ex);
            }
        }

        static public string GetStringSessionValue(Controller controller, string key, string defaultValue = "~~~", bool decryptIfEncrypted = true)
        {
            string value = string.Empty;
            try
            {
                object sessionValue = controller.Session[key];
                if (sessionValue != null)
                {
                    if (sessionValue is string)
                    {
                        return SessionHelper.DecryptIfEncrypted(key, (string)sessionValue, decryptIfEncrypted);
                    }
                    else
                    {
                        return sessionValue.ToString();
                    }
                }
                return defaultValue != "~~~" ? defaultValue : SessionHelper.ReturnDefaultStringValue(key);
            }
            catch (Exception ex)
            {
                return SessionHelper.ReturnDefaultStringValue(key, ex);
            }
        }

        static public object GetSessionValue(Controller controller, string key, object defaultValue = null)
        {
            var value = controller.Session[key];
            if (value == null) value = defaultValue;
            return value;
        }
        static public T GetSessionValue<T>(Controller controller, string key, T defaultValue = default(T)) where T : new()
        {
            var value = controller.Session[key];
            if (value is string && !string.IsNullOrWhiteSpace((string)value))
            {
                if (IsValueEncrypted(key))
                {
                    string stringValue = Cryptography.Decrypt((string)value);
                    T objValue = JsonConvert.DeserializeObject<T>(stringValue);
                    return objValue;
                }
                else
                {
                    return (T)value;
                }
            }
            if (value == null) value = defaultValue;
            return (T)value;
        }

        static public void SetSessionValue<T>(Controller controller, string key, T value, bool dontEncrypt = false)
        {
            controller.Session[key] = dontEncrypt ? value : SessionHelper.EncryptIfShouldBeEncrypted(key, value);
        }

        static public void SetSessionObjectValue<T>(Controller controller, string key, T value) where T : new()
        {
            if (value != null)
            {
                if (IsValueEncrypted(key))
                {
                    var json = JsonConvert.SerializeObject(value);
                    var encryptedValue = Cryptography.Encrypt(json);
                    controller.Session[key] = encryptedValue;
                }
                else
                {
                    controller.Session[key] = value;
                }
            }
        }

        static public void RemoveSessionValue(Controller controller, string key)
        {
            controller.Session.Remove(key);
        }

        static public void ClearSession(Controller controller)
        {
            controller.Session.Clear();
        }
    }
}
public static class SessionHelper
{
    private static FieldInfo[] _fields = typeof(UserSession.Key).GetFields();
    private static Dictionary<string, EncryptedValueAttribute> _encryptedValueAttributes = new Dictionary<string, EncryptedValueAttribute>();

    public static string DecryptIfEncrypted(string key, string value, bool decryptIfEncrypted = true)
    {
        if (decryptIfEncrypted && IsValueEncrypted(key))
        {
            try
            {
                value = Cryptography.Decrypt(value);
            }
            catch (Exception ex)
            {
                lock (_encryptedValueAttributes)
                {
                    // If an exception occurrs assume that the value is not encrypted;
                    _encryptedValueAttributes[key] = EncryptedValueAttribute.False;
                }
            }
        }

        return value;
    }

    public static object EncryptIfShouldBeEncrypted(string key, object value)
    {
        if (value != null && IsValueEncrypted(key))
        {
            value = Cryptography.Encrypt(value.ToString());
        }
        return value;
    }

    public static bool IsValueEncrypted(string key)
    {
        var encryptedAttribute = FindEncryptedAttribute(key);
        var isEncrypted = encryptedAttribute.IsEncrypted;

        return isEncrypted;
    }

    public static EncryptedValueAttribute FindEncryptedAttribute(string key)
    {
        EncryptedValueAttribute encryptedValueAttribute = EncryptedValueAttribute.False;
        lock (_encryptedValueAttributes)
        {
            if (!_encryptedValueAttributes.TryGetValue(key, out encryptedValueAttribute))
            {
                var field = _fields.Where(f => f.GetRawConstantValue().ToString() == key).SingleOrDefault();
                if (field != null)
                {
                    encryptedValueAttribute = field.GetCustomAttributes(false).Where(a => a.GetType() == typeof(EncryptedValueAttribute)).FirstOrDefault() as EncryptedValueAttribute;
                }
                encryptedValueAttribute = encryptedValueAttribute ?? EncryptedValueAttribute.False;
                _encryptedValueAttributes.Add(key, encryptedValueAttribute);
            }
        }
        return encryptedValueAttribute;
    }

    private static DefaultValueAttribute FindDefaultValueAttribute(string key)
    {
        DefaultValueAttribute defaultValueAttribute = null;

        var field = _fields.Where(f => f.GetRawConstantValue().ToString() == key).SingleOrDefault();
        if (field != null)
        {
            defaultValueAttribute = field.GetCustomAttributes(false).Where(a => a.GetType() == typeof(DefaultValueAttribute)).FirstOrDefault() as DefaultValueAttribute;
        }
        return defaultValueAttribute;
    }

    public static bool ReturnDefaultBoolValue(string key, Exception ex = null)
    {
        var defaultValueAttribute = FindDefaultValueAttribute(key);

        if (defaultValueAttribute != null)
        {
            var defaultValue = defaultValueAttribute.Value as bool?;
            if (defaultValue.HasValue) return defaultValue.Value;
        }

        return false;
    }

    public static string ReturnDefaultStringValue(string key, Exception ex = null)
    {
        var defaultValueAttribute = FindDefaultValueAttribute(key);

        if (defaultValueAttribute != null)
        {
            var defaultValue = defaultValueAttribute.Value.ToString();
            if (defaultValue != null) return defaultValue;
        }

        return null;
    }

    public static int ReturnDefaultIntValue(string key, Exception ex = null)
    {
        var defaultValueAttribute = FindDefaultValueAttribute(key);

        if (defaultValueAttribute != null)
        {
            var defaultValue = defaultValueAttribute.Value as int?;
            if (defaultValue.HasValue) return defaultValue.Value;
        }

        return 0;
    }
}
