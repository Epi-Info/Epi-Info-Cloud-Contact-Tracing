using System;

namespace Epi.Cloud.Common.Core
{
    public class NotifyConsistencyServiceAttribute : Attribute
    {
        public bool ShouldNotify {get; set; }
        public NotifyConsistencyServiceAttribute(bool shouldNotify = false)
        {
            ShouldNotify = shouldNotify;
        }
        public static readonly NotifyConsistencyServiceAttribute False = new NotifyConsistencyServiceAttribute(false);
    }
}
