﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Epi.Web.Enter.Common.Message
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class PreFilledAnswerResponse
    {
        public PreFilledAnswerResponse()
        {
        }

        [DataMember]
        public Dictionary<string, string> ErrorMessageList;
        [DataMember]
        public string SurveyResponseUrl;
        [DataMember]
        public string Status;
        [DataMember]
        public string SurveyResponseID;
        [DataMember]
        public string SurveyResponsePassCode;

    }
}
