using System;
using System.Runtime.Serialization;
using Epi.Cloud.Common.Metadata;

namespace Epi.Web.Enter.Common.DTO
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class SurveyInfoDTO : MetadataAccessor
    {
        [DataMember]
        public string ParentId { get; set; }

        [DataMember]
        public int DataAccessRuleId { get; set; }

        [DataMember]
        public string SurveyId { get { return _formId; } set { _formId = value; } }

        [DataMember]
        public string SurveyNumber { get; set; }

        [DataMember]
        public string SurveyName { get; set; }

        [DataMember]
        public int SurveyType { get; set; }

        [DataMember]
        public string OrganizationName { get; set; }

        [DataMember]
        public string DepartmentName { get; set; }

        [DataMember]
        public string IntroductionText { get; set; }

        [DataMember]
        public string ExitText { get; set; }

        [DataMember]
        public string XML { get; set; }

        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public DateTime ClosingDate { get; set; }

        [DataMember]
        public Guid UserPublishKey { get; set; }

        [DataMember]
        public Guid OrganizationKey { get; set; }


        [DataMember]
        public bool IsDraftMode { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public int ViewId { get; set; }

        [DataMember]
        public int OwnerId { get; set; }

        [DataMember]
        public bool IsSqlProject { get; set; }

        [DataMember]
        public string DBConnectionString { get; set; }

        [DataMember]
        public bool IsShareable { get; set; }

        [DataMember]
        public bool IsShared { get; set; }

        [DataMember]
        public bool HasDraftModeData { get; set; }

        [DataMember]
        public bool EwavLiteToggleSwitch { get; set; }
    }
}
