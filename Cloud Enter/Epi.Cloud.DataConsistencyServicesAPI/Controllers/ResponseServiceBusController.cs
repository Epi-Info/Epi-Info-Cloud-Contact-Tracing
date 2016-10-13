using Epi.Cloud.DataConsistencyServicesAPI.Proxy;
using Epi.Cloud.DataConsistencyServicesAPI.Services.ServiceBusService;
using Epi.Cloud.DBAccessService.Handlers;
using System.Web.Http;

namespace Epi.Cloud.DataConsistencyServicesAPI.Controllers
{
    public class ResponseServiceBusController : ApiController
    {
        public IResponseInfoServiceBus _responseInfoServiceBus;

        public ResponseServiceBusController()
        {
            _responseInfoServiceBus = new ResponseInfoServiceBus();
        }

        // GET: api/ResponseServiceBus
        public IHttpActionResult Get()
        {
            return new ServiceResult<string>(_responseInfoServiceBus.GetResponseInfoMessageFromServiceBus(), this);
        }

		//// GET: api/FormServiceBus/{responseId}
		//public string Get(string id)
		//{
		//	var responseInfoService = new ResponseInfoService();
		//	var formResponseDetailJson = responseInfoService.GetResponseInfoData(id);
		//	return formResponseDetailJson;
		//}

		//// POST: api/FormServiceBus
		//public void Post([FromBody]string value)
		//{
		//}

		//// PUT: api/FormServiceBus/5
		//public void Put(int id, [FromBody]string value)
		//{
		//}

		//// DELETE: api/FormServiceBus/5
		//public void Delete(int id)
		//{
		//}
	}
}
