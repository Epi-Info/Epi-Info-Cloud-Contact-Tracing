using System.Threading.Tasks;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.MetadataServices.ProxiesService.Interface
{
    public interface IProjectMetadataProxy
    {
        Task<Template> GetProjectMetadataAsync(string projectId);//Read project
    }
}
