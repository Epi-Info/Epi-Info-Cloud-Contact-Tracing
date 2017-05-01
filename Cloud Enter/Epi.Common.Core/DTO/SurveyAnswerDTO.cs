using System;
using System.Collections.Generic;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.Common.DTO
{
    public class SurveyAnswerDTO : IResponseContext
    {
        public SurveyAnswerDTO()
        {
            ResponseDetail = new FormResponseDetail();
        }
        public FormResponseDetail ResponseDetail { get; set; }


        #region Proxy to ResponseContext in ResponseDetail

        public string ResponseId
        {
            get { return ((IResponseContext)ResponseDetail).ResponseId; }
            set { ((IResponseContext)ResponseDetail).ResponseId = value; }
        }
        public string ParentResponseId
        {
            get { return ((IResponseContext)ResponseDetail).ParentResponseId; }
            set { ((IResponseContext)ResponseDetail).ParentResponseId = value; }
        }

        public string RootResponseId
        {
            get { return ((IResponseContext)ResponseDetail).RootResponseId; }
            set { ((IResponseContext)ResponseDetail).RootResponseId = value; }
        }

        public string FormId
        {
            get { return ((IResponseContext)ResponseDetail).FormId; }
            set { ((IResponseContext)ResponseDetail).FormId = value; }
        }

        public string ParentFormId
        {
            get { return ((IResponseContext)ResponseDetail).ParentFormId; }
            set { ((IResponseContext)ResponseDetail).ParentFormId = value; }
        }

        public string RootFormId
        {
            get { return ((IResponseContext)ResponseDetail).RootFormId; }
            set { ((IResponseContext)ResponseDetail).RootFormId = value; }
        }

        public string FormName
        {
            get { return ((IResponseContext)ResponseDetail).FormName; }
            set { ((IResponseContext)ResponseDetail).FormName = value; }
        }

        public string ParentFormName
        {
            get { return ((IResponseContext)ResponseDetail).ParentFormName; }
            set { ((IResponseContext)ResponseDetail).ParentFormName = value; }
        }

        public string RootFormName
        {
            get { return ((IResponseContext)ResponseDetail).RootFormName; }
            set { ((IResponseContext)ResponseDetail).RootFormName = value; }
        }

        public bool IsNewRecord
        {
            get { return ((IResponseContext)ResponseDetail).IsNewRecord; }
            set { ((IResponseContext)ResponseDetail).IsNewRecord = value; }
        }

        public int UserId
        {
            get { return ((IResponseContext)ResponseDetail).UserId; }
            set { ((IResponseContext)ResponseDetail).UserId = value; }
        }

        public string UserName
        {
            get { return ((IResponseContext)ResponseDetail).UserName; }
            set { ((IResponseContext)ResponseDetail).UserName = value; }
        }

        public bool IsChildResponse { get { return ((IResponseContext)ResponseDetail).IsChildResponse; } }

        public bool IsRootResponse { get { return ((IResponseContext)ResponseDetail).IsRootResponse; } }

        #endregion // Proxy to ResponseContext in ResponseDetail

        public string SurveyId
        {
            get { return ((IResponseContext)ResponseDetail).FormId; }
            set { ((IResponseContext)ResponseDetail).FormId = value; }
        }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public DateTime DateCreated { get; set; }
        public int Status { get; set; }
        public RecordStatusChangeReason ReasonForStatusChange { get; set; }
        public Guid UserPublishKey { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsLocked { get; set; }
        public string UserEmail { get; set; }
        public int LastActiveUserId { get; set; }
        public int RecordSourceId { get; set; }
        public int ViewId { get; set; }
        public int FormOwnerId { get; set; }
        public int LoggedInUserId { get; set; }
        public bool RecoverLastRecordVersion { get; set; }
        public string RequestedViewId { get; set; }
        public int CurrentPageNumber { get; set; }

        public Dictionary<string, string> SqlData { get; set; }

    }
}
