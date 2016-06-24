using Epi.Cloud.Common.Metadata;
using Epi.Cloud.MetadataServices.DataTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epi.Cloud.MetadataServices.ProxiesService.Interface
{
    public interface IProjectMetadataProxy
    {
        Task<Template> GetProjectMetadataAsync(string projectId);//Read project
    }
}
