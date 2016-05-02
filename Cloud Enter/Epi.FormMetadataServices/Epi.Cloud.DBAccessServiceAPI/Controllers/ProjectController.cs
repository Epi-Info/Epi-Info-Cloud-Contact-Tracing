
using System.Web.Http;
using System.Net;
using Epi.Cloud.DBAccessService.Proxy.Interfaces;
using Epi.Cloud.DBAccessService.Handlers;
using Epi.Cloud.DBAccessService.Services;
using System.Collections.Generic;
using Epi.Cloud.MetadataServices.DataTypes;

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
            return new ServiceResult<List<MetadataFieldAttributes>>(_projectService.GetProjectMetaData(null), this);
        }

        // GET: api/Project/5
        public IHttpActionResult Get(string ID)
        {

            return new ServiceResult<List<MetadataFieldAttributes>>(_projectService.GetProjectMetaData(ID), this);             
        }
        
    }
}
