using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;

namespace Epi.Cloud.DataEntryServices.Model
{
    public class Survey
    {
        public SurveyProperties SurveyProperties { get; set; }
        public SurveyQuestionandAnswer SurveyQuestionandAnswer { get; set; }
        public string SurveyName { get; set; }
        public int TotalNoofPages { get; set; }

    }
    public class SurveyProperties:Resource
    {
        public string SurveyID { get; set; }
        public string GlobalRecordID { get; set; }
        public int RecStatus { get; set; }
        public string PageId { get; set; }
        public string PagePosition { get; set; }
        public DateTime DateOfInterview { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UserId { get; set; }
    }
    public class SurveyQuestionandAnswer:Resource
    {
        public string GlobalRecordID { get; set; }
        public string PageId { get; set; }
        public Dictionary<string, string> SurveyQAList { get; set; }
    }
}