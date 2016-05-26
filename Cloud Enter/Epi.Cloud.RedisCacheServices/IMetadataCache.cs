using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public interface IMetadataCache
    {
        ProjectTemplateMetadata GetProjectTemplateMetadata(string projectName, int? pageId = null);
        PageMetadata GetPageMetadata(string projectName, int pageId);
        bool SetProjectTemplateMetadata(ProjectTemplateMetadata projectTemplateMetadata);
        void ClearProjectTemplateMetadataFromCache(string projectName);
    }
}
