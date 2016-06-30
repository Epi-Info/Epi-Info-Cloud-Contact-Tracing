
using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.Cloud.Common.Metadata;
using MvcDynamicForms.Fields;

namespace Epi.Cloud.FormMetadataServices
{
    public interface IMetadataProvider
    {
        Task<IEnumerable<FieldAttributes>> GetMetadataAsync(string formId, int pageNumber);
        IEnumerable<FieldAttributes> GetFieldMedatadata(Template projectTemplateMetadata, string formId, int pageNumber);
    }
}