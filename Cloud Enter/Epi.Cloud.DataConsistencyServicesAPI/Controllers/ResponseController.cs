using System;
using System.Web.Http;
using Epi.Cloud.DataConsistencyServices.Proxy;
using Epi.Cloud.DataConsistencyServices.Services;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.DataStructures;
using Epi.MetadataAccessService.Handlers;
using Newtonsoft.Json;

namespace Epi.Cloud.DataConsistencyServices.Controllers
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
            ResponseContext responseContext;
            try
            {
                responseContext = JsonConvert.DeserializeObject<ResponseContext>(id);
            }
            catch
            {
                responseContext = new ResponseContext { ResponseId = id };
            }
            return new ServiceResult<string>(_responseInfoService.GetResponseInfoData(responseContext), this);
        }

		// PUT: api/Response/formResponseDetailJson
		public void Put(int id, [FromBody]string formResponseDetailJson)
        {
			try
			{
				var formResponseDetail = JsonConvert.DeserializeObject<FormResponseDetail>(formResponseDetailJson);

				//TODO: Call Epi.MetadataAccessServiceAPI  Response/Put      formResponseDetailJson
			}
			catch (Exception ex)
			{
			}
        }
    }
}
