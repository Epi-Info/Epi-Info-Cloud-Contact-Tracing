using System.Web.Http;
using Epi.MetadataAccessService.Proxy.Interfaces;
using Epi.MetadataAccessService.Handlers;
using Epi.MetadataAccessService.Services;
using Epi.FormMetadata.DataStructures;

namespace Epi.MetadataAccessService.Controllers
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
