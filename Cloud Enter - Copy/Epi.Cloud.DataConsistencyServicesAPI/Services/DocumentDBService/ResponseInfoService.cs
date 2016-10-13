using Epi.Cloud.DataConsistencyServicesAPI.Proxy;
using Newtonsoft.Json;
using Epi.DataPersistenceServices.DocumentDB;
using Epi.DataPersistence.Extensions;

namespace Epi.Cloud.DataConsistencyServicesAPI.Services
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