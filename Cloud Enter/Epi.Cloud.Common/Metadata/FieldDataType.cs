using System;
using System.Collections.Generic;

namespace Epi.Cloud.Common.Metadata
{
    public enum FieldDataType
    {
        Undefined = -1,
        Object = 0,
        Number = 1,
        Text = 2,
        Date = 3,
        Time = 4,
        DateTime = 5,
        Boolean = 6,
        PhoneNumber = 7,
        YesNo = 8,
        Unknown = 9,
        Guid = 10,
        Class = 11,
        Function = 12
    }
}
