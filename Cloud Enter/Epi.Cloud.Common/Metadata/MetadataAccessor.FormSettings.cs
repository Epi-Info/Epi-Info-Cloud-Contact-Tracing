using System.Collections.Generic;
using System.Linq;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.Common.Metadata
{
    public partial class MetadataAccessor
    {
        public List<string> GetAllColumnNames(string FormId)
        {
            var columnNameList = GetFieldDigests(FormId)
                .Where(f => !FieldDigest.NonDataFieldTypes.Any(t => f.FieldType == t))
                .Select(f => f.TrueCaseFieldName)
                .OrderBy(n => n).ToList();

            return columnNameList;
        }
    }
}
