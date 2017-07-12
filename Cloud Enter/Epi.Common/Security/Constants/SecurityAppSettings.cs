using System.ComponentModel;
using Epi.Common.Configuration;

namespace Epi.Common.Security.Constants
{
    public static class SecurityAppSettings
    {
        public struct Key
        {
            #region Password Policy

            [DefaultValue(6)]
            public const string PasswordMinimumLength = "PasswordMinimumLength";

            [DefaultValue(10)]
            public const string PasswordMaximumLength = "PasswordMaximumLength";

            [DefaultValue(3)]
            public const string NumberOfTypesRequiredInPassword = "NumberOfTypesRequiredInPassword";

            [DefaultValue(4)]
            public const string TotalNumberOfTypesInPassword = "TotalNumberOfTypesInPassword";

            [DefaultValue(true)]
            public const string UseNumbers = "UseNumbers";

            [DefaultValue(true)]
            public const string UseUpperCase = "UseUpperCase";

            [DefaultValue(true)]
            public const string UseLowerCase = "UseLowerCase";

            [DefaultValue(true)]
            public const string UseSymbols = "UseSymbols";

            [DefaultValue("@#$|{}^")]
            public const string Symbols = "Symbols";

            [DefaultValue(true)]
            public const string RepeatCharacters = "RepeatCharacters";

            [DefaultValue(true)]
            public const string ConsecutiveCharacters = "ConsecutiveCharacters";

            [DefaultValue(true)]
            public const string UseUserIdInPassword = "UseUserIdInPassword";

            [DefaultValue(true)]
            public const string UseUserNameInPassword = "UseUserNameInPassword";

            #endregion Password Policy

            #region Cryptography

            [DefaultValue("jEz9wopRFHNx8R7OQSgmr0Ye6xBb9nPKKDZAydJ6fmp2 / jFJPEYDnz33TQqXz +/ qXjoYhWh5QD9MG / BBzDrjAskF2XaJX44LwceZC3yiuR5 / CPI013gYuffEsCPeTuo0VHeqxQ ==")]
            public const string KeyForConnectionStringPassphrase = "KeyForConnectionStringPassphrase";

            [DefaultValue("I3mi1ehgzE/9eGiWdTVrMxCQPWHgkOGVg9mZuIcF1XSnxG6dOOAtnbzeYsrnQHvSD1zh3V1eVBLuypTGP0vNw7lEo6FXCpnICGXy+yNH57i+JnT9MTBZuRc5BrBbQTPF64vANg==")]
            public const string KeyForConnectionStringSalt = "KeyForConnectionStringSalt";

            [DefaultValue("1PIJhCF67TY/ciQni/FHYOLH7q+bNajOm3gaHMl34dlYTauD9vBQpfI4uLM+4sz1t8o1LVVO0y6e9PaGPRkLZpr3d+2ubEmrMuNQihMcxXdOATDkulmiYhjH8l55mEQEMozPjw==")]
            public const string KeyForUserPasswordSalt = "KeyForUserPasswordSalt";

            [DefaultValue("G6up33hyUX5guTj+")]
            public const string KeyForConnectionStringVector = "KeyForConnectionStringVector";

            #endregion Cryptography
        }

        #region Helper functions

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

        #endregion
    }
}
