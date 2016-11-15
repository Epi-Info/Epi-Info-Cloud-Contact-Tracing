using Epi.MetadataAccessService.Proxy.Interfaces;
using Epi.MetadataAccessService.Repository;
using Epi.FormMetadata.DataStructures;
using Epi.FormMetadata.Extensions;

namespace Epi.MetadataAccessService.Services
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