using Epi.Cloud.Common.Metadata;
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
    public class SurveyProperties : Resource
    {
        public string SurveyID { get; set; }
        public int RecStatus { get; set; }
        public string GlobalRecordID { get; set; }
        public string FirstSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime LastSaveTime { get; set; }
        public string UserId { get; set; }
        public string PageId { get; set; }
    }
    public class SurveyQuestionandAnswer : Resource
    {
        public int RecStatus { get; set; }
        public string SurveyID { get; set; }
        public string GlobalRecordID { get; set; }
        public int PageId { get; set; }
        public List<Digest> Digest { get; set; }
        public Dictionary<string, string> SurveyQAList { get; set; }
        public ProjectDigest[] ProjectDigest { get; set; }
    }

    public class Digest : ProjectDigest
    {
        public Dictionary<string, string> Fields { get; set; }
    }
}