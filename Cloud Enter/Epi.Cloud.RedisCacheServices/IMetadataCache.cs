using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public interface IMetadataCache
    {
        ProjectTemplateMetadata GetProjectTemplateMetadata(string projectId, int? pageId = null);
        PageMetadata GetPageMetadata(string projectId, int pageId);
        bool SetProjectTemplateMetadata(ProjectTemplateMetadata projectTemplateMetadata);
        void ClearProjectTemplateMetadataFromCache(string projectId);
    }
}
