using System;
using System.Collections.Generic;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.CacheServices
{
    public interface IMetadataCache
    {
        bool PageMetadataExists(Guid projectId, Guid formId, int pageId);
        Page GetPageMetadata(Guid projectId, Guid Guid, int pageId);
        bool SetProjectTemplateMetadata(Template projectTemplateMetadata);
        void ClearProjectTemplateMetadataFromCache(Guid projectId);
        Guid GetDeployedProjectId();
        Dictionary<string, string> GetDeploymentProperties(Guid projectId);
        bool SetDeploymentProperties(Dictionary<string, string> deploymentProperties);
    }
}
