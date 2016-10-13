using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using Epi.Cloud.Common.Constants;

namespace Epi.Cloud.DataEntryServices.Model
{
    public class FormDocumentDBEntity
    { 
        public string GlobalRecordID { get; set; }
        public FormPropertiesResource FormProperties { get; set; }
        public PageResponseDetailResource PageResponseDetail { get; set; }
        public bool IsChildForm { get; set; }
        public string SurveyName { get; set; }
        public int TotalNumberOfPages { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
   
    public class FormPropertiesResource : Resource
    {
        public FormPropertiesResource()
        {
            RecStatus = RecordStatus.InProcess;
        }
        public string GlobalRecordID { get; set; }
        public string FormId { get; set; }
        public string FormName { get; set; }
        public int RecStatus { get; set; }
        public string RelateParentId { get; set; }
        public string FirstSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime LastSaveTime { get; set; }
        public string UserId { get; set; }
    }

    public class PageResponseDetailResource : Resource
    {
        public string GlobalRecordID { get; set; }
        public int PageId { get; set; }
        public Dictionary<string, string> ResponseQA { get; set; }
    }
}