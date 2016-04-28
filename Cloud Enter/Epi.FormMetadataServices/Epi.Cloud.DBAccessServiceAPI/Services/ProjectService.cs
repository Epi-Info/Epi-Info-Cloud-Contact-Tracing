using Epi.Cloud.DBAccessService.Proxy.Interfaces;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Epi.Cloud.SqlServer;
using Epi.Cloud.DBAccessService.Repository;
using System.Threading.Tasks;
using Epi.Cloud.MetadataServices.DataTypes;

namespace Epi.Cloud.DBAccessService.Services
{
    public class ProjectService:IProjectProxyService
    {
        public ProjectService()
        {

        } 

        /// <summary>
        /// Get the meta data based on page id
        /// </summary>
        /// <param name="projectid"></param>
        /// <returns></returns>
        public List<MetadataFieldAttribute> GetProjectMetaData(string projectid)
        {
            GetmetadataDB getMetadata = new GetmetadataDB();
            var task = getMetadata.MetaDataAsync(Convert.ToInt32(projectid));
            return task.Result;
        } 


    }
}