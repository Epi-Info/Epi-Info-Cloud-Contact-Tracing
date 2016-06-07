using Epi.Cloud.MetadataServices.ProxiesService.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.Cloud.MetadataServices.DataTypes;
using static Epi.Cloud.MetadataServices.DataTypes.Constants;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.MetadataServices.ProxiesService
{
    public class FieldAttributeServiceProxy : MetadataProxy, IFieldAttributeProxy
    {

        //Forming url to call the DBAccess API
        public async Task<ProjectTemplateMetadata> GetProjectMetadataAsync(string PageId)
        {
            ProjectTemplateMetadata projectResponse= new ProjectTemplateMetadata();
            string url = string.Format("{0}?ID={1}", ApiEndPoints.Project, PageId);
            if (url != null)
            {
                projectResponse = GetData<ProjectTemplateMetadata>(url);
            }
            return await Task.FromResult(projectResponse);
        }
    }
}
