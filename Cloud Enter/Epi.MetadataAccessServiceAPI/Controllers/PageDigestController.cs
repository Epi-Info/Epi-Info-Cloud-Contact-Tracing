using System.Web.Http;
using Epi.MetadataAccessService.Proxy.Interfaces;
using Epi.MetadataAccessService.Handlers;
using Epi.MetadataAccessService.Services;
using Epi.FormMetadata.DataStructures;

namespace Epi.MetadataAccessService.Controllers
{
    public class PageDigestController : ApiController
    {
        private IProjectProxyService _projectService;
        public PageDigestController()
        {
            _projectService = new ProjectService();
        }

        // GET: PageDigest
        // GET: api/Project/5
        public IHttpActionResult Get()
        {
            return new ServiceResult<PageDigest[][]>(_projectService.GetPageDigestMetaData(), this);
        }
    }
}