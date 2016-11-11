﻿using Epi.Cloud.MetadataServices.ProxiesService.Interface;
using System.Threading.Tasks;
using static Epi.Cloud.MetadataServices.DataTypes.Constants;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.MetadataServices.ProxiesService
{
    public class ProjectMetadataServiceProxy : MetadataProxy, IProjectMetadataProxy
    {

        //Forming url to call the DBAccess API
        public async Task<Template> GetProjectMetadataAsync(string projectId)
        {
            Template projectResponse= new Template();
            string url = string.Format("{0}?ID={1}", ApiEndPoints.Project, projectId ?? "0");
            if (url != null)
            {
                projectResponse = GetData<Template>(url);
            }
            return await Task.FromResult(projectResponse);
        }

        //Forming url to call the  API for pageDigest
        public async Task<PageDigest[][]> GetPageDigestMetadataAsync()
        {
            PageDigest[][] pageResponse = null;
            string url = string.Format("{0}", ApiEndPoints.PageDigest);
            if (url != null)
            {
                pageResponse = GetData<PageDigest[][]>(url);
            }
            return await Task.FromResult(pageResponse);
        }



    }
}
