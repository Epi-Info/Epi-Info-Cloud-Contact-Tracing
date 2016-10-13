using System;
using System.Collections.Generic;

namespace Epi.Cloud.Common
{
    public class CaseInsensitiveEqualityComparer : IEqualityComparer<string>
    {
        public static readonly IEqualityComparer<string> Instance = new CaseInsensitiveEqualityComparer();

        public bool Equals(string x, string y)
        {
            return string.Compare(x, y, true) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj != null ? obj.GetHashCode() : 0;
        }
    }
}
