using Epi.Cloud.DataConsistencyServicesAPI.Proxy;
using Epi.Cloud.DataConsistencyServicesAPI.Services;
using Epi.Cloud.DBAccessService.Handlers;
using Newtonsoft.Json;
using System.Web.Http;
using System;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.DataConsistencyServicesAPI.Controllers
{
    public class ResponseController : ApiController
    {
        private IResponseInfoProxyService _responseInfoService;

        public ResponseController()
        {
            _responseInfoService = new ResponseInfoService();
        }

        // GET: api/Project/5
        public IHttpActionResult Get(string id)
        {
            return new ServiceResult<string>(_responseInfoService.GetResponseInfoData(id), this);
        }

		// PUT: api/Response/formResponseDetailJson
		public void Put(int id, [FromBody]string formResponseDetailJson)
        {
			try
			{
				var formResponseDetail = JsonConvert.DeserializeObject<FormResponseDetail>(formResponseDetailJson);

				//TODO: Call Epi.Cloud.DBAccessServiceAPI  Response/Put      formResponseDetailJson
			}
			catch (Exception ex)
			{
			}
        }
    }
}
