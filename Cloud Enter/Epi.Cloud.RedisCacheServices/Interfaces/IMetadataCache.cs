using System;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.CacheServices
{
    public interface IMetadataCache
    {
#if CacheFullProjectMetadata
        bool FullProjectTemplateMetadataExists(Guid projectId);
        Template GetFullProjectTemplateMetadata(Guid projectId);
        Template GetProjectTemplateMetadata(Guid projectId, Guid formId, int? pageId);
        Template GetProjectTemplateMetadataByPageNumber(Guid projectId, Guid formId, int? pageNumber);
#endif
        bool PageMetadataExists(Guid projectId, Guid formId, int pageId);
        Page GetPageMetadata(Guid projectId, Guid Guid, int pageId);
        bool SetProjectTemplateMetadata(Template projectTemplateMetadata);
        void ClearProjectTemplateMetadataFromCache(Guid projectId);
    }
}
