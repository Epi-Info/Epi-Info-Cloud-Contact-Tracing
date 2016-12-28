using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.Common.DTO
{
    public class RelatedFormsInfoDTO
    {
        public FieldDigest[] FieldDigests { get; set; }
        public FormResponseDetail ResponseDetail { get; set; }
    }
}