using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Epi.Common.Attributes;
using Epi.Common.Security;

namespace Epi.Common.Configuration
{
    public class ConfigurationAttributesHelper
    {
        public ConfigurationAttributesHelper(Type keyType)
        {
            _keyType = keyType;
            _fields = keyType.GetFields();
        }

        private FieldInfo[] _fields;
        private Type _keyType;

        public bool IsValueEncrypted(string key)
        {
            var encryptedAttribute = FindEncryptedAttribute(key);
            var isEncrypted = encryptedAttribute.IsEncrypted;

            return isEncrypted;
        }

        public bool GetBoolValue(string key)
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

        public int GetIntValue(string key)
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

        public string GetStringValue(string key, bool decryptIfEncrypted = true)
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

        public string GetConnectionString(string key, bool decryptIfEncrypted = true)
        {
            if (key != null)
            {
                string connectionString = null;
                try
                {
                    connectionString = ConfigurationManager.ConnectionStrings[key].ConnectionString;
                    if (decryptIfEncrypted && IsValueEncrypted(key))
                    {
                        var decryptedConnectionString = Cryptography.Decrypt(connectionString);
                        return decryptedConnectionString;
                    }
                    else
                    {
                        return connectionString;
                    }
                }
                catch (Exception ex)
                {
                    // If an exception occurrs assume that the value is not encrypted;
                    return connectionString;
                }
            }
            return null;
        }


        private Dictionary<string, EncryptedValueAttribute> _encryptedValueAttributes = new Dictionary<string, EncryptedValueAttribute>();

        private EncryptedValueAttribute FindEncryptedAttribute(string key)
        {
            EncryptedValueAttribute encryptedValueAttribute = EncryptedValueAttribute.False;
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
            return encryptedValueAttribute;
        }


        private DefaultValueAttribute FindDefaultValueAttribute(string key)
        {
            DefaultValueAttribute defaultValueAttribute = null;

            var field = _fields.Where(f => f.GetRawConstantValue().ToString() == key).SingleOrDefault();
            if (field != null)
            {
                defaultValueAttribute = field.GetCustomAttributes(false).Where(a => a.GetType() == typeof(DefaultValueAttribute)).FirstOrDefault() as DefaultValueAttribute;
            }
            return defaultValueAttribute;
        }

        private bool ReturnDefaultBoolValue(string key, Exception ex = null)
        {
            var defaultValueAttribute = FindDefaultValueAttribute(key);

            if (defaultValueAttribute != null)
            {
                var defaultValue = defaultValueAttribute.Value as bool?;
                if (defaultValue.HasValue) return defaultValue.Value;
            }

            throw new SettingsPropertyNotFoundException(key, ex);
        }

        private string ReturnDefaultStringValue(string key, Exception ex = null)
        {
            var defaultValueAttribute = FindDefaultValueAttribute(key);

            if (defaultValueAttribute != null)
            {
                var defaultValue = defaultValueAttribute.Value.ToString();
                if (defaultValue != null) return defaultValue;
            }

            throw new SettingsPropertyNotFoundException(key, ex);
        }

        private int ReturnDefaultIntValue(string key, Exception ex = null)
        {
            var defaultValueAttribute = FindDefaultValueAttribute(key);

            if (defaultValueAttribute != null)
            {
                var defaultValue = defaultValueAttribute.Value as int?;
                if (defaultValue.HasValue) return defaultValue.Value;
            }

            throw new SettingsPropertyNotFoundException(key, ex);
        }
    }
}
