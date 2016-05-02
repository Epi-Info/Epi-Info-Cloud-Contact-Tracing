
using Epi.Cloud.MetadataServices.DataTypes;
using System.Collections.Generic;

namespace Epi.Cloud.DBAccessService.Proxy.Interfaces
{
    public interface IProjectProxyService
    { 
        List<MetadataFieldAttributes> GetProjectMetaData(string projectID); 
    }
}