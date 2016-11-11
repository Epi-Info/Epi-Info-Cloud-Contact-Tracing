using Epi.Cloud.DBAccessService.Proxy.Interfaces;
using Epi.Cloud.DBAccessService.Repository;
using Epi.FormMetadata.DataStructures;
using Epi.FormMetadata.Extensions;

namespace Epi.Cloud.DBAccessService.Services
{
    public class ProjectService : IProjectProxyService
    {
        public ProjectService()
        {

        }

        /// <summary>
        /// Get the meta data based on project id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public Template GetProjectMetaData(string projectId)
        {
            GetmetadataDB getMetadata = new GetmetadataDB();
            var task = getMetadata.MetaDataAsync(projectId);
            return task.Result;
        }

        /// <summary>
        /// Get the PageDigest data based on project id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public PageDigest[][] GetPageDigestMetaData()
        {
            GetmetadataDB getMetadata = new GetmetadataDB();
            var template = getMetadata.MetaDataAsync("1").Result;
            var pageDigestData = template.ToPageDigests();
            return pageDigestData;

        }
       
    }
}