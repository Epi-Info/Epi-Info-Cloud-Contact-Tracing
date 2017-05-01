using Epi.Cloud.DataConsistencyServices.Proxy;
using Epi.Common.Core.Interfaces;
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
		public string GetResponseInfoData(IResponseContext responceContext)
		{

			SurveyResponseCRUD surveyResponseCRUD = new SurveyResponseCRUD();
			var formResponseProperties = surveyResponseCRUD.GetHierarchialResponseListByResponseId(responceContext, /*includeDeletedRecords=*/true, /*excludeInProcessRecords=*/true);
			var formResponseDetail = formResponseProperties != null ? formResponseProperties.ToHierarchialFormResponseDetail() : null;
			string response = JsonConvert.SerializeObject(formResponseDetail);
			return response;
		}
	}
}