using System.Collections.Generic;

namespace Epi.Cloud.Common.Message
{
    public class PreFilledAnswerResponse
    {
        public PreFilledAnswerResponse()
        {
        }

        public Dictionary<string, string> ErrorMessageList;
        public string SurveyResponseUrl;
        public string Status;
        public string SurveyResponseID;
        public string SurveyResponsePassCode;

    }
}
