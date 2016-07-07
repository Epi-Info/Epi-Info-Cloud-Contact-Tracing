using System;
using Epi.Cloud.Interfaces.DataInterface;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.DataEntryServices
{
    public class DataEntryService : IDataEntryService
    {
        public DataEntryService()
        {
        }

        public SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest pRequest)
        {
            return new SurveyAnswerResponse();
        }

        public SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest pRequest)
        {
            return new SurveyAnswerResponse();
        }
    }
}
