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
        public async Task<List<MetadataFieldAttributes>> GetProjectMetadataAsync(string PageId)
        {
            List<MetadataFieldAttributes> projectResponse = new List<MetadataFieldAttributes>();
            string url = string.Format("{0}?ID={1}", ApiEndPoints.Project, PageId);
            if (url != null)
            {
                projectResponse = GetData<List<MetadataFieldAttributes>>(url);
            }
            return await Task.FromResult(projectResponse);
        }
    }
}
