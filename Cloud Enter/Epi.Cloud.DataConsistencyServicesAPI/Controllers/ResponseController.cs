using System;
using System.Web.Http;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.DataConsistencyServices.Proxy;
using Epi.Cloud.DataConsistencyServices.Services;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
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

        // GET: api/Response/c94b709b-2c7f-4f79-9042-a8f8bc37d5b5?formId=2e1d01d4-f50d-4f23-888b-cd4b7fc9884b
        public IHttpActionResult Get(string responseId, string formId, string rootResponseId = null)
        {
            IResponseContext responseContext;
            try
            {
                responseContext = new ResponseContext
                {
                    ResponseId = responseId,
                    FormId = formId,
                    RootResponseId = rootResponseId
                }.ResolveMetadataDependencies();
            }
            catch
            {
                responseContext = new ResponseContext { ResponseId = responseId };
            }
            return new ServiceResult<string>(_responseInfoService.GetResponseInfoData(responseContext), this);
        }

		// PUT: api/Response/
		public void Put([FromBody]string formResponseDetailJson)
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
