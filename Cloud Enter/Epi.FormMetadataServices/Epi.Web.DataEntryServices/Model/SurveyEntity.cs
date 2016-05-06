using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Epi.Web.DataEntryServices.Model
{
    public class SurveyEntity : TableEntity
    {
        public SurveyEntity() { }
        public SurveyEntity(string responseId, string surveyId)
        {
            this.PartitionKey = responseId;
            this.RowKey = surveyId;
            this.Timestamp = DateTime.Now;
        }

        public List<SurveyTableEntity> SurveyTableEntity { get; set; }
        public int PageNumber { get; set; }
        public string SurveyData { get; set; }
        public int StatusId { get; set; }
        public string SurveyName { get; set; }
        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }
        public string UserEmail { get; set; }
        public bool IsLocked { get; set; }

    }
}