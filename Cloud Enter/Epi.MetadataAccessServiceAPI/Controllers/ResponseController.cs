using System;
using System.Web.Http;
using Epi.DataPersistence.DataStructures;
using Newtonsoft.Json;

namespace Epi.MetadataAccessServiceAPI.Controllers
{
    [Authorize]
    public class ResponseController : ApiController
    {
		// PUT api/response/formResponseDetailJson
		public void Put([FromBody]string formResponseDetailJson)
        {
			try
			{
				var formResponseDetail = JsonConvert.DeserializeObject<FormResponseDetail>(formResponseDetailJson);
			    // TODO: Persist to SQL Database
			}
			catch (Exception ex)
			{
			}
		}
	}
}
