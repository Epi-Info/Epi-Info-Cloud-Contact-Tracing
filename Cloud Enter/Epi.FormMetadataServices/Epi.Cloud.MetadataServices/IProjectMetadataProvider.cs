using System.Threading.Tasks;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.MetadataServices
{
    public interface IProjectMetadataProvider
    {
        //Pass the page id and call the DBAccess API and get the project fileds.
        Task<Template> GetProjectMetadataAsync(string projectId);
    }
}
