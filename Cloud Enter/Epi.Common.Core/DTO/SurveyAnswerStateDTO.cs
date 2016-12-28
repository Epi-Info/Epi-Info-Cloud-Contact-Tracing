using System;
using Epi.DataPersistence.Constants;

namespace Epi.Cloud.Common.DTO
{
	public class SurveyAnswerStateDTO
	{
		public string ResponseId { get; set; }
		public string SurveyId { get; set; }
		public DateTime DateUpdated { get; set; }
		public DateTime? DateCompleted { get; set; }
		public DateTime DateCreated { get; set; }
		public int Status { get; set; }
		public Guid UserPublishKey { get; set; }
		public bool IsDraftMode { get; set; }
		public bool IsLocked { get; set; }
		public string ParentRecordId { get; set; }
		public string UserEmail { get; set; }
		public int LastActiveUserId { get; set; }
		public string RelateParentId { get; set; }
		public int RecordSourceId { get; set; }
		public int ViewId { get; set; }
		public int FormOwnerId { get; set; }
		public int LoggedInUserId { get; set; }
		public bool RecoverLastRecordVersion { get; set; }
		public string RequestedViewId { get; set; }
		public int CurrentPageNumber { get; set; }
		public RecordStatusChangeReason ReasonForStatusChange { get; set; }
    }
}
