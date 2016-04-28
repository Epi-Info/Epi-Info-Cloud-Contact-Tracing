using Epi.Cloud.MetadataServices.ProxiesService.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Cloud.MetadataServices.DataTypes;
using static Epi.Cloud.MetadataServices.DataTypes.Constants;

namespace Epi.Cloud.MetadataServices.ProxiesService
{
    public class FieldAttributeServiceProxy : MetadataProxy, IFieldAttributeProxy
    {

        //Forming url to call the DBAccess API
        public async Task<List<MetadataFieldAttribute>> GetProjectMetadataAsync(string PageId)
        {
            List<MetadataFieldAttribute> projectResponse = new List<MetadataFieldAttribute>();
            string url = string.Format("{0}?ID={1}", ApiEndPoints.Project, PageId);
            if (url != null)
            {
                projectResponse = GetData<List<MetadataFieldAttribute>>(url);
            }
            return await Task.FromResult(projectResponse);
        }
    }
}
