using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.DBAccessService.Proxy.Interfaces
{
    public interface IProjectProxyService
    {
        ProjectTemplateMetadata GetProjectMetaData(string projectId);
    }
}