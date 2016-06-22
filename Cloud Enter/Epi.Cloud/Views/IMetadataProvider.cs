
using System.Collections.Generic;
using System.Threading.Tasks;
using MvcDynamicForms.Fields;

namespace Epi.Cloud.FormMetadataServices
{
    public interface IMetadataProvider
    {
        Task<List<FieldAttributes>> GetMetadataAsync(string formId, int pageNumber);
    }
}