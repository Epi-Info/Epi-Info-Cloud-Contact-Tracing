using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public interface IMetadataCache
    {
        bool FullProjectTemplateMetadataExists(string projectId);
        Template GetFullProjectTemplateMetadata(string projectId);
        Template GetProjectTemplateMetadata(string projectId, string formId, int? pageId);
        Template GetProjectTemplateMetadataByPageNumber(string projectId, string formId, int? pageNumber);
        bool PageMetadataExists(string projectId, string formId, int pageId);
        Page GetPageMetadata(string projectId, string formId, int pageId);
        bool SetProjectTemplateMetadata(Template projectTemplateMetadata);
        void ClearProjectTemplateMetadataFromCache(string projectId);
    }
}
