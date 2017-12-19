using System;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Constants;
using Epi.Common.Core.Interfaces;
using System.Collections.Generic;

namespace Epi.Cloud.Common.BusinessObjects
{
    public class SurveyResponseBO : IFormResponseDetail
    {
        [ThreadStatic]
        public FormResponseDetail ActiveChildResponseDetail;

        public SurveyResponseBO(FormResponseDetail responseDetail = null)
        {
            ResponseDetail = responseDetail ?? new FormResponseDetail();
            DateUpdated = DateTime.UtcNow;
            RecStatus = RecordStatus.InProcess;
        }

        public string ResponseId { get { return ResponseDetail.ResponseId; } set { ResponseDetail.ResponseId = value; } }
        public string FormId { get { return ResponseDetail.FormId; } set { ResponseDetail.FormId = value; } }
        public string FormName { get { return ResponseDetail.FormName; } set { ResponseDetail.FormName = value; } }

        public string ParentResponseId { get { return ResponseDetail.ParentResponseId; } set { ResponseDetail.ParentResponseId = value; } }
        public string ParentFormId { get { return ResponseDetail.ParentFormId; } set { ResponseDetail.ParentFormId = value; } }
        public string ParentFormName { get { return ResponseDetail.ParentFormName; } set { ResponseDetail.ParentFormName = value; } }

        public string RootResponseId { get { return ResponseDetail.RootResponseId; } set { ResponseDetail.RootResponseId = value; } }
        public string RootFormId { get { return ResponseDetail.RootFormId; } set { ResponseDetail.RootFormId = value; } }
        public string RootFormName { get { return ResponseDetail.RootFormName; } set { ResponseDetail.RootFormName = value; } }

        public bool IsChildResponse { get { return (ResponseDetail).IsChildResponse; } }
        public bool IsRootResponse { get { return (ResponseDetail).IsRootResponse; } }

        public int UserOrgId { get { return ResponseDetail.UserOrgId; }
            set
            {
                ResponseDetail.UserOrgId = value;
                if (ActiveChildResponseDetail != null) ActiveChildResponseDetail.UserOrgId = value;
            }
        }
        public int UserId { get { return ResponseDetail.UserId; }
            set
            {
                ResponseDetail.UserId = value;
                if (ActiveChildResponseDetail != null) ActiveChildResponseDetail.UserId = value;

            }
        }
        public int LastActiveOrgId { get { return ResponseDetail.UserOrgId; }
            set
            {
                ResponseDetail.UserOrgId = value;
                if (ActiveChildResponseDetail != null) ActiveChildResponseDetail.UserOrgId = value;

            }
        }
        public int LastActiveUserId
        {
            get { return ResponseDetail.UserId; }
            set
            {
                ResponseDetail.UserId = value;
                if (ActiveChildResponseDetail != null) ActiveChildResponseDetail.UserId = value;

            }
        }
        public string UserName { get { return ResponseDetail.UserName; } set { ResponseDetail.UserName = value; } }

        public DateTime DateUpdated
        {
            get { return ResponseDetail.LastSaveTime; }
            set
            {
                ResponseDetail.LastSaveTime = value;
                if (ActiveChildResponseDetail != null) ActiveChildResponseDetail.LastSaveTime = value;

            }
        }
        public DateTime DateCreated { get { return ResponseDetail.FirstSaveTime; } set { ResponseDetail.FirstSaveTime = value; } }
        public bool IsNewRecord { get { return ResponseDetail.IsNewRecord; } set { ResponseDetail.IsNewRecord = value; } }
        public bool IsDraftMode { get { return ResponseDetail.IsDraftMode; } set { ResponseDetail.IsDraftMode = value; } }
        public bool IsLocked { get { return ResponseDetail.IsLocked; } set { ResponseDetail.IsLocked = value; } }
        public FormResponseDetail ResponseDetail { get; set; }

        public int RecStatus { get { return ResponseDetail.RecStatus; }
            set
            {
                ResponseDetail.RecStatus = value;
                if (ActiveChildResponseDetail != null) ActiveChildResponseDetail.RecStatus = value;

            }
        }
        public string FirstSaveLogonName { get { return ResponseDetail.FirstSaveLogonName; } set { ResponseDetail.FirstSaveLogonName = value; } }

        public string LastSaveLogonName { get { return ResponseDetail.LastSaveLogonName; }
            set
            {
                ResponseDetail.LastSaveLogonName = value;
                if (ActiveChildResponseDetail != null) ActiveChildResponseDetail.LastSaveLogonName = value;

            }
        }

        public DateTime FirstSaveTime { get { return ResponseDetail.FirstSaveTime; } set { ResponseDetail.FirstSaveTime = value; } }

        public DateTime LastSaveTime { get { return ResponseDetail.LastSaveTime; }
            set
            {
                ResponseDetail.LastSaveTime = value;
                if (ActiveChildResponseDetail != null) ActiveChildResponseDetail.LastSaveTime = value;

            }
        }

        public bool IsRelatedView { get { return ResponseDetail.IsRelatedView; } set { ResponseDetail.IsRelatedView = value; } }

        public List<int> PageIds { get { return ResponseDetail.PageIds; } set { ResponseDetail.PageIds = value; } }

        public int LastPageVisited { get { return ResponseDetail.LastPageVisited; } set { ResponseDetail.LastPageVisited = value; } }

        public string RequiredFieldsList { get { return ResponseDetail.RequiredFieldsList; } set { ResponseDetail.RequiredFieldsList = value; } }

        public string HiddenFieldsList { get { return ResponseDetail.HiddenFieldsList; } set { ResponseDetail.HiddenFieldsList = value; } }

        public string HighlightedFieldsList { get { return ResponseDetail.HighlightedFieldsList; } set { ResponseDetail.HighlightedFieldsList = value; } }

        public string DisabledFieldsList { get { return ResponseDetail.DisabledFieldsList; } set { ResponseDetail.DisabledFieldsList = value; } }

        public List<PageResponseDetail> PageResponseDetailList { get { return ResponseDetail.PageResponseDetailList; } set { ResponseDetail.PageResponseDetailList = value; } }

        public List<FormResponseDetail> ChildFormResponseDetailList { get { return ResponseDetail.ChildFormResponseDetailList; } set { ResponseDetail.ChildFormResponseDetailList = value; } }

        public Guid UserPublishKey { get; set; }
        public DateTime? DateCompleted { get; set; }
        public RecordStatusChangeReason ReasonForStatusChange { get; set; }
        public string UserEmail { get; set; }
        public int ViewId { get; set; }
        public int RecordSourceId { get; set; }
        public int CurrentOrgId { get; set; }
    }
}
