using System.Threading.Tasks;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.MetadataServices.Common.ProxyService.Interfaces
{
    public interface IProjectMetadataProxy
    {
        Task<Template> GetProjectMetadataAsync(string projectId);//Read project

        Task<PageDigest[][]> GetPageDigestMetadataAsync();//Read project


    }
}
