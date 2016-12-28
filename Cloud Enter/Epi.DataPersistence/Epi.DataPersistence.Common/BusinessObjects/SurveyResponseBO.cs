using System;
using System.Collections.Generic;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Constants;

namespace Epi.Cloud.Common.BusinessObjects
{
    public class SurveyResponseBO : ICloneable
    {

        public SurveyResponseBO()
        {
            this.DateUpdated = DateTime.Now;
            this.Status = RecordStatus.InProcess;
        }

        public string ResponseId { get; set; }
        public Guid UserPublishKey { get; set; }
        public string SurveyId { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public bool IsNewRecord { get; set; }
        public int Status { get; set; }
		public RecordStatusChangeReason ReasonForStatusChange { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsLocked { get; set; }
        public string ParentRecordId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string ParentId { get; set; }
        public string RelateParentId { get; set; }
        public List<SurveyResponseBO> ResponseHierarchyIds { get; set; }
        public int ViewId { get; set; }
        public int LastActiveUserId { get; set; }
        public Dictionary<string, string> SqlData { get; set; }
        public int RecordSourceId { get; set; }
        public int CurrentOrgId { get; set; }

        public FormResponseDetail ResponseDetail { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
