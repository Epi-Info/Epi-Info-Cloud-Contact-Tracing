using System;

namespace Epi.Cloud.Common.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class EncryptedValueAttribute : Attribute
    {
        public EncryptedValueAttribute(bool isEncrypted = true)
        {
            IsEncrypted = isEncrypted;
        }
        public bool IsEncrypted { get; private set; }

        public static readonly EncryptedValueAttribute False = new EncryptedValueAttribute(false);
    }
}
