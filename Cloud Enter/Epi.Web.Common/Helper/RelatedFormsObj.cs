using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;

namespace Epi.Web.Enter.Common.Helper
{
    public class RelatedFormsObj
    {
        public FieldDigest[] FieldDigests { get; set; }
        public FormResponseDetail ResponseDetail { get; set; }
    }
}