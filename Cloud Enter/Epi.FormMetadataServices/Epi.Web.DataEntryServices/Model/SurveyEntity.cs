using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;

namespace Epi.Cloud.DataEntryServices.Model
{
    public class Survey : Resource
    {
        public SurveyProperties SurveyProperties { get; set; }

        public List<SurveyQuestionandAnswer> SurveyQuestionandAnswer { get; set; }

    }
    public class SurveyProperties
    {
        public string RecStatus { get; set; }
        public string GlobalRecordID { get; set; }

        public string CaseID { get; set; }
    }
    public class SurveyQuestionandAnswer
    {
        public string GlobalRecordID { get; set; }
        public List<KeyValuePair<string, string>> SurveyQAList { get; set; }
        public DateTime DateOfInterview { get; set; }
        public int PageNumber { get; set; }
        public string SurveyName { get; set; }
    }
}