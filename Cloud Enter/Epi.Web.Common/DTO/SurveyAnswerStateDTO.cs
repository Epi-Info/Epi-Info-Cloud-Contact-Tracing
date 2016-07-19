using System;
using System.Runtime.Serialization;

namespace Epi.Web.Enter.Common.DTO
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class SurveyAnswerStateDTO
    {
        [DataMember]
        public string ResponseId { get; set; }
        [DataMember]
        public string SurveyId { get; set; }
        [DataMember]
        public DateTime DateUpdated { get; set; }
        [DataMember]
        public DateTime? DateCompleted { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public Guid UserPublishKey { get; set; }

        [DataMember]
        public bool IsDraftMode { get; set; }

        [DataMember]
        public bool IsLocked { get; set; }

        [DataMember]
        public string ParentRecordId { get; set; }

        [DataMember]
        public string UserEmail { get; set; }

        [DataMember]
        public int LastActiveUserId { get; set; }

        [DataMember]
        public string RelateParentId { get; set; }

        [DataMember]
        public int RecordSourceId { get; set; }

        [DataMember]
        public int ViewId { get; set; }

        [DataMember]
        public int FormOwnerId { get; set; }

        [DataMember]
        public int LoggedInUserId { get; set; }

        [DataMember]
        public bool RecoverLastRecordVersion { get; set; }

        public string RequestedViewId { get; set; }

        public int CurrentPageNumber { get; set; }
    }
}
