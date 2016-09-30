using Epi.Cloud.DataConsistencyServicesAPI.Proxy;
using Epi.Cloud.DataConsistencyServicesAPI.Services;
using Epi.Cloud.DBAccessService.Handlers;
using System.Collections.Generic;
using System.Web.Http;

namespace Epi.Cloud.DataConsistencyServicesAPI.Controllers
{
    public class FormController : ApiController
    {
        private IFormInfoProxyService _forminfoService;

        public FormController()
        {
            _forminfoService = new FormInfoService();
        }
        // GET: api/Form
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Project/5
        public IHttpActionResult Get(string id)
        {
            return new ServiceResult<string>(_forminfoService.GetFormInfoData(id), this);
        }

        // POST: api/Form
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Form/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Form/5
        public void Delete(int id)
        {
        }
    }
}
