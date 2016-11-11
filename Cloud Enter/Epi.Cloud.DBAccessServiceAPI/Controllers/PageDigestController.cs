using System.Web.Http;
using Epi.Cloud.DBAccessService.Proxy.Interfaces;
using Epi.Cloud.DBAccessService.Handlers;
using Epi.Cloud.DBAccessService.Services;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.DBAccessServiceAPI.Controllers
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