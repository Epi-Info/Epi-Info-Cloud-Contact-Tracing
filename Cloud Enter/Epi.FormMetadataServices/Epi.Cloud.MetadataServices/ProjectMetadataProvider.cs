using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.Cloud.MetadataServices.DataTypes;
using Epi.Cloud.MetadataServices.ProxiesService;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.MetadataServices
{
    public class ProjectMetadataProvider
    {
        //Pass the page id and call the DBAccess API and get the project fileds.
        public async Task<ProjectTemplateMetadata> GetProjectMetadata(string projectId)
        {
            FieldAttributeServiceProxy serviceProxy = new FieldAttributeServiceProxy();
            var task = serviceProxy.GetProjectMetadataAsync(projectId);
            return await task;
        }
    }
}
