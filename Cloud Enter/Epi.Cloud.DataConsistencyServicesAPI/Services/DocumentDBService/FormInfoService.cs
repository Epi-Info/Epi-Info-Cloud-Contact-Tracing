using Epi.Cloud.DataConsistencyServicesAPI.Proxy;
using Epi.Cloud.DataEntryServices.Facade;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Epi.Cloud.DataConsistencyServicesAPI.Services
{
    public class FormInfoService : IFormInfoProxyService
    {
        public FormInfoService()
        {

        }
        public string GetFormInfoData(string id)
        {
            SurveyDocumentDBFacade _surveyDocumentFacade = new SurveyDocumentDBFacade();
            var DataEntryServiceResponse = _surveyDocumentFacade.ReadFormInfo(id);
            string response = JsonConvert.SerializeObject(DataEntryServiceResponse);
            return response;
        }
    }
}