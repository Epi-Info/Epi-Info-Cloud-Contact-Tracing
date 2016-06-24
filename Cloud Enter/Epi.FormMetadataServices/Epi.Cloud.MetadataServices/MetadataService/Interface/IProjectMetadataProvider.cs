using System.Threading.Tasks;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.MetadataServices
{
    public interface IProjectMetadataProvider
    {
        //Pass the page id and call the DBAccess API and get the project fileds.
        Task<Template> GetProjectMetadataAsync(string projectId, ProjectElement elements = ProjectElement.Full);
        Task<Template> GetProjectMetadataWithPageByPageIdAsync(string projectId, int pageId);
        Task<Template> GetProjectMetadataWithPageByPageNumberAsync(string projectId, string formId, int pageNumber);
    }

    public enum ProjectElement
    {
        Full = -1,
        TemplateWithoutPages = 0
    }
}
