using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.DBAccessService.Proxy.Interfaces
{
    public interface IProjectProxyService
    {
        Template GetProjectMetaData(string projectId);
    }
}