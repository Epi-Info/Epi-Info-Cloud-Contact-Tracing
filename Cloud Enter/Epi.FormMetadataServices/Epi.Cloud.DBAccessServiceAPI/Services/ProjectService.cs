using Epi.Cloud.DBAccessService.Proxy.Interfaces;
using System;
using System.Collections.Generic;
using Epi.Cloud.DBAccessService.Repository;
using Epi.Cloud.MetadataServices.DataTypes;

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
        public List<MetadataFieldAttributes> GetProjectMetaData(string projectid)
        {
            GetmetadataDB getMetadata = new GetmetadataDB();
            var task = getMetadata.MetaDataAsync(Convert.ToInt32(projectid));
            return task.Result;
        }

        public IEnumerable<string> GetDropdownValues(string TableName, string ColumnName)
        {
            GetmetadataDB getMetadata = new GetmetadataDB();
            IEnumerable<string> lstDropdownVal = getMetadata.GetDropDownValuesDb(TableName, ColumnName);
            return lstDropdownVal;
        }


    }
}