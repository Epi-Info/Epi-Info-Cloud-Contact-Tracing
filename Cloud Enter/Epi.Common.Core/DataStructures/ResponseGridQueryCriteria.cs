using System.Collections.Generic;
using System.Linq;
using Epi.Common.Core.Interfaces;
using Epi.FormMetadata.DataStructures;

namespace Epi.Common.Core.DataStructures
{
    public class ResponseGridQueryCriteria
    {
        public IResponseContext ResponseContext { get; set; }
        public ResponseAccessRuleContext ResponseAccessRuleContext { get; set; }
        public IDictionary<int, KeyValuePair<FieldDigest, string>> SearchQualifiers { get; set; }
        public IDictionary<int, FieldDigest> DisplayFields { get; set; }
        public int DisplayPageSize { get; set; }
        public int DisplayPageNumber { get; set; }
        public string SortByField { get; set; }
        public bool IsSortedAscending { get; set; }

        //Helpers
        public string[] TrueCaseFieldNames { get { return DisplayFields.Select(f => f.Value.TrueCaseFieldName).ToArray(); } }

        public KeyValuePair<FieldDigest, string>[] SearchFieldNameValueQualifiers { get { return SearchQualifiers != null && SearchQualifiers.Count > 0
                        ? SearchQualifiers.Values.ToArray()
                        : null; } }
    }
}
