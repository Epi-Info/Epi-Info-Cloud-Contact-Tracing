using System.Web.Http;
using Epi.Cloud.DBAccessService.Proxy.Interfaces;
using Epi.Cloud.DBAccessService.Handlers;
using Epi.Cloud.DBAccessService.Services;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.DBAccessService.Controllers
{
    public class ProjectController : ApiController
    {
        private IProjectProxyService _projectService;

        public ProjectController()
        {
            _projectService = new ProjectService();
        }

        // GET: api/Project/5
        public IHttpActionResult Get()
        {
            return new ServiceResult<Template>(_projectService.GetProjectMetaData(null), this);
        }

        // GET: api/Project/5
        public IHttpActionResult Get(string ID)
        {

            return new ServiceResult<Template>(_projectService.GetProjectMetaData(ID), this);
        }

    }
}
