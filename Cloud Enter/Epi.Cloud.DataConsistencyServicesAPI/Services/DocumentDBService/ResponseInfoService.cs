using Epi.Cloud.DataConsistencyServices.Proxy;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistenceServices.DocumentDB;
using Newtonsoft.Json;

namespace Epi.Cloud.DataConsistencyServices.Services
{
	public class ResponseInfoService : IResponseInfoProxyService
	{
		public ResponseInfoService()
		{

		}
		public string GetResponseInfoData(string id)
		{

			SurveyResponseCRUD surveyResponseCRUD = new SurveyResponseCRUD();
			var formResponseProperties = surveyResponseCRUD.GetHierarchialResponsesByResponseId(id, /*includeDeletedRecords=*/true, /*excludeInProcessRecords=*/true);
			var formResponseDetail = formResponseProperties != null ? formResponseProperties.ToFormResponseDetail() : null;
			string response = JsonConvert.SerializeObject(formResponseDetail);
			return response;
		}
	}
}