using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public interface IMetadataCache
    {
        bool FullProjectTemplateMetadataExists(string projectId);
        bool PageMetadataExists(string projectId, int pageId);
        Template GetFullProjectTemplateMetadata(string projectId);
        Template GetProjectTemplateMetadata(string projectId, int? pageId);
        Template GetProjectTemplateMetadata(string projectId, string formId, int? pageNumber);
        Page GetPageMetadata(string projectId, int pageId);
        bool SetProjectTemplateMetadata(Template projectTemplateMetadata);
        void ClearProjectTemplateMetadataFromCache(string projectId);

        bool PageFieldAttributesExists(string projectId, string formId, int pageNumber);
        FieldAttributes[] GetPageFieldAttributes(string projectId, string formId, int pageNumber);
        bool SetPageFieldAttributes(FieldAttributes[] fieldAttributes, string projectId, string formId, int pageNumber);
        void ClearPageFieldAttributesFromCache(string projectId);
    }
}
