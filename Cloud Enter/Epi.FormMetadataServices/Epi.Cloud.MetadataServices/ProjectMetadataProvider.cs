using System.Threading.Tasks;
using Epi.Cloud.MetadataServices.ProxiesService;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.MetadataServices
{
    public class ProjectMetadataProvider : IProjectMetadataProvider
    {
        public ProjectMetadataProvider()
        {
        }

        //Pass the project id and call the DBAccess API and get the project metadata.
        public async Task<Template> GetProjectMetadataAsync(string projectId)
        {
            FieldAttributeServiceProxy serviceProxy = new FieldAttributeServiceProxy();
            var templateMetadata = await serviceProxy.GetProjectMetadataAsync(projectId);
            return  templateMetadata;
        }
    }
}
