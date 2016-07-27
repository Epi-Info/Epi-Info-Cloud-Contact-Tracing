using Epi.Cloud.Common.Metadata;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;

namespace Epi.Cloud.DataEntryServices.Model
{
    public class Survey
    {
        public SurveyProperties SurveyProperties { get; set; }
        public FormQuestionandAnswer FormQuestionandAnswer { get; set; }
        public FormParentProperties FormParentProperties { get; set; }
        public FormChildProperties FormChildProperties { get; set; }
        public bool IsChildForm { get; set; }
        public string SurveyName { get; set; }
        public int TotalNoofPages { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }

    }
    public class SurveyProperties : Resource
    {
        public string FormId { get; set; }
        public bool RecStatus { get; set; }
        public string GlobalRecordID { get; set; }
        public string FirstSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime LastSaveTime { get; set; }
        public string UserId { get; set; }
    }
    public class FormParentProperties : Resource
    {
        public string FormId { get; set; }
        public bool RecStatus { get; set; }
        public string GlobalRecordID { get; set; }
        public string FirstSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime LastSaveTime { get; set; }
        public string UserId { get; set; }
        public bool IsRelatedView { get; set; }
    }

    public class FormChildProperties : FormParentProperties
    {
        public string RelateParentId { get; set; }
    }
    public class FormQuestionandAnswer : Resource
    {
        public string GlobalRecordID { get; set; }
        public Dictionary<string, string> SurveyQAList { get; set; }
    }
}