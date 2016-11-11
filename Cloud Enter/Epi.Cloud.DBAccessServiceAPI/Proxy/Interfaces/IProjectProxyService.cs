using Epi.Cloud.Common.Metadata;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.DBAccessService.Proxy.Interfaces
{
    public interface IProjectProxyService
    {
        Template GetProjectMetaData(string projectId);

        PageDigest[][] GetPageDigestMetaData();
    }
}