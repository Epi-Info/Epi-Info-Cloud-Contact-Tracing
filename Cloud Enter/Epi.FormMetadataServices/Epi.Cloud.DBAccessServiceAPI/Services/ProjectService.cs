using Epi.Cloud.DBAccessService.Proxy.Interfaces;
using System;
using Epi.Cloud.DBAccessService.Repository;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.DBAccessService.Services
{
    public class ProjectService : IProjectProxyService
    {
        public ProjectService()
        {

        }

        /// <summary>
        /// Get the meta data based on page id
        /// </summary>
        /// <param name="projectid"></param>
        /// <returns></returns>
        public ProjectTemplateMetadata GetProjectMetaData(string projectid)
        {
            GetmetadataDB getMetadata = new GetmetadataDB();
            var task = getMetadata.MetaDataAsync(Convert.ToInt32(projectid));
            return task.Result;
        }
    }
}