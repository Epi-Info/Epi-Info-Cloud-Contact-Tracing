using System;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Constants;
using Epi.Common.Core.Interfaces;

namespace Epi.Cloud.Common.BusinessObjects
{
    public class SurveyResponseBO : ICloneable, IResponseContext
    {
        public SurveyResponseBO()
        {
            ResponseDetail = new FormResponseDetail();
            DateUpdated = DateTime.Now;
            Status = RecordStatus.InProcess;
        }

        public string SurveyId { get { return ResponseDetail.FormId; } set { ResponseDetail.FormId = value; } }
        public string ResponseId { get { return ResponseDetail.ResponseId; } set { ResponseDetail.ResponseId = value; } }
        public string ParentResponseId { get { return ResponseDetail.ParentResponseId; } set { ResponseDetail.ParentResponseId = value; } }
        public string RootResponseId { get { return ResponseDetail.RootResponseId; } set { ResponseDetail.RootResponseId = value; } }
        public string FormId { get { return ResponseDetail.FormId; } set { ResponseDetail.FormId = value; } }
        public string FormName { get { return ResponseDetail.FormName; } set { ResponseDetail.FormName = value; } }
        public string ParentFormId { get { return ResponseDetail.ParentFormId; } set { ResponseDetail.ParentFormId = value; } }
        public string ParentFormName { get { return ResponseDetail.ParentFormName; } set { ResponseDetail.ParentFormName = value; } }
        public string RootFormId { get { return ResponseDetail.RootFormId; } set { ResponseDetail.RootFormId = value; } }
        public string RootFormName { get { return ResponseDetail.RootFormName; } set { ResponseDetail.RootFormName = value; } }

        public bool IsChildResponse { get { return ((IResponseContext)ResponseDetail).IsChildResponse; } }
        public bool IsRootResponse { get { return ((IResponseContext)ResponseDetail).IsRootResponse; } }

        public Guid UserPublishKey { get; set; }
        public DateTime DateUpdated { get { return ResponseDetail.LastSaveTime; } set { ResponseDetail.LastSaveTime = value; } }
        public DateTime DateCreated { get { return ResponseDetail.FirstSaveTime; } set { ResponseDetail.FirstSaveTime = value; } }
        public DateTime? DateCompleted { get; set; }
        public bool IsNewRecord { get; set; }
        public int Status { get; set; }
        public RecordStatusChangeReason ReasonForStatusChange { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsLocked { get; set; }
        public int UserId { get { return ResponseDetail.UserId; } set { ResponseDetail.UserId = value; } }
        public string UserName { get { return ResponseDetail.UserName; } set { ResponseDetail.UserName = value; } }
        public string UserEmail { get; set; }
        public int ViewId { get; set; }
        public int LastActiveUserId { get { return ResponseDetail.UserId; } set { ResponseDetail.UserId = value; } }
        public int RecordSourceId { get; set; }
        public int CurrentOrgId { get; set; }

        public FormResponseDetail ResponseDetail { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
