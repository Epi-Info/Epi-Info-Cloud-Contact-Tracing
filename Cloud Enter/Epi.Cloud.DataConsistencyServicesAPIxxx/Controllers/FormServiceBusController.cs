using Epi.Cloud.DataConsistencyServicesAPI.Proxy;
using Epi.Cloud.DataConsistencyServicesAPI.Services.ServiceBusService;
using Epi.Cloud.DBAccessService.Handlers;
using System.Web.Http;

namespace Epi.Cloud.DataConsistencyServicesAPI.Controllers
{
    public class FormServiceBusController : ApiController
    {
        public IFormInfoServiceBus _fromInfoServiceBus;

        public FormServiceBusController()
        {
            _fromInfoServiceBus = new FormInfoServiceBus();
        }
        // GET: api/FormServiceBus
        public IHttpActionResult Get()
        {
            return new ServiceResult<string>(_fromInfoServiceBus.GetFormInfoFromServiceBus(), this);
        }

        // GET: api/FormServiceBus/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/FormServiceBus
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/FormServiceBus/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/FormServiceBus/5
        public void Delete(int id)
        {
        }
    }
}
