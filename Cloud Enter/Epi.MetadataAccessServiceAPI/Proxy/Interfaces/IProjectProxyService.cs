using Epi.Cloud.Common.Metadata;
using Epi.FormMetadata.DataStructures;

namespace Epi.MetadataAccessService.Proxy.Interfaces
{
    public interface IProjectProxyService
    {
        Template GetProjectMetaData(string projectId);

        PageDigest[][] GetPageDigestMetaData();
    }
}