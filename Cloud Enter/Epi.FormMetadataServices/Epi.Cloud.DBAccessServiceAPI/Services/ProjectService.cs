using Epi.Cloud.DBAccessService.Proxy.Interfaces;
using Epi.Cloud.DBAccessService.Repository;
using Epi.FormMetadata.DataStructures;

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
    }
}