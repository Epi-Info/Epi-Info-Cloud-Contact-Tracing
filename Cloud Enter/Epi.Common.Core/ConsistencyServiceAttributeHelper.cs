using System;
using System.Linq;
using System.Reflection;
using Epi.DataPersistence.Constants;

namespace Epi.Cloud.Common.Core
{
    public static class ConsistencyServiceAttributeHelper
    {
        private static FieldInfo[] _fields = typeof(RecordStatusChangeReason).GetFields();
        private static Type _keyType = typeof(RecordStatusChangeReason);

        public static bool ShouldNotifyConsistencyService(RecordStatusChangeReason recordStatusChangeReason)
        {
            var shouldNotify = false;
            NotifyConsistencyServiceAttribute notifyConsistencyServiceAttribute = NotifyConsistencyServiceAttribute.False;
            notifyConsistencyServiceAttribute = FindNotifyConsistencyServiceAttribute(recordStatusChangeReason);
            shouldNotify = notifyConsistencyServiceAttribute.ShouldNotify;

            return shouldNotify;
        }

        private static NotifyConsistencyServiceAttribute FindNotifyConsistencyServiceAttribute(RecordStatusChangeReason key)
        {
            NotifyConsistencyServiceAttribute notifyConsistencyServiceAttribute = null;
            var field = _fields.Where(f => f.Name == key.ToString()).SingleOrDefault();
            if (field != null)
            {
                notifyConsistencyServiceAttribute = field.GetCustomAttributes(false).Where(a => a.GetType() == typeof(NotifyConsistencyServiceAttribute)).FirstOrDefault() as NotifyConsistencyServiceAttribute;
            }
            notifyConsistencyServiceAttribute = notifyConsistencyServiceAttribute ?? NotifyConsistencyServiceAttribute.False;
            return notifyConsistencyServiceAttribute;
        }
    }
}
