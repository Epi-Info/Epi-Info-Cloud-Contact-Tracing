
using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.Interfaces.MetadataInterfaces
{
    public interface IFieldAttributesProvider
    {
        Task<IDictionary<string, FieldAttributes>> GetPageFieldAttributesAsync(string formId, int pageNumber);
        Task<IDictionary<int, IDictionary<string, FieldAttributes>>> GetFormPageFieldAttributesForAllPagesAsync(string formId);
    }
}