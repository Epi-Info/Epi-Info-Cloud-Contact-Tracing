using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public interface IMetadataCache
    {
        Template GetProjectTemplateMetadata(string projectId, int? pageId = null);
        Page GetPageMetadata(string projectId, int pageId);
        bool SetProjectTemplateMetadata(Template projectTemplateMetadata);
        void ClearProjectTemplateMetadataFromCache(string projectId);
    }
}
