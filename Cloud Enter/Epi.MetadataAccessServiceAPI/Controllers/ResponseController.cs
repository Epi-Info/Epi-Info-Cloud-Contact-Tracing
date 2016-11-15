using System.Web.Http;
using Newtonsoft.Json;
using Epi.DataPersistence.DataStructures;
using System;

namespace Epi.MetadataAccessServiceAPI.Controllers
{
    [Authorize]
    public class ResponseController : ApiController
    {
		// GET api/response/5
		//public string Get(string id)
		//{
		//    return "value";
		//}

		// PUT api/response/formResponseDetailJson
		public void Put([FromBody]string formResponseDetailJson)
        {
			try
			{
				var formResponseDetail = JsonConvert.DeserializeObject<FormResponseDetail>(formResponseDetailJson);
			}
			catch (Exception ex)
			{
			}
			// TODO: Persist to SQL Database
		}
	}
}
