using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.MetadataServices
{
    public interface IProjectMetadataProvider
    {
        Task<Template> GetProjectMetadataAsync(string projectId, ProjectScope scope = ProjectScope.TemplateWithAllPages);
        //Pass the page id and call the DBAccess API and get the project fileds.
        Task<Template> GetProjectMetadataAsync(string projectId, string formId, ProjectScope scope = ProjectScope.TemplateWithNoPages);
        Task<Template> GetProjectMetadataWithPageByPageIdAsync(string projectId, int pageId);
        Task<Template> GetProjectMetadataWithPageByPageNumberAsync(string projectId, string formId, int? pageNumber);
        Task<ProjectDigest[]> GetProjectDigestAsync(string projectId);
        Task<FieldDigest> GetFieldDigestAsync(string projectId, string formId, string fieldName);
        Task<FieldDigest[]> GetFieldDigestAsync(string projectId, string formId, IEnumerable<string> fieldNames);
    }

    public enum ProjectScope
    {
        TemplateWithAllPages = -1,
        TemplateWithNoPages = 0
    }
}
