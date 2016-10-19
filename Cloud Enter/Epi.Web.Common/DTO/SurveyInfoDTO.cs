using System;
using System.Runtime.Serialization;
using Epi.Cloud.Common.Metadata;

namespace Epi.Web.Enter.Common.DTO
{
    public class SurveyInfoDTO : MetadataAccessor
    {
        public string ParentId { get; set; }

        public int DataAccessRuleId { get; set; }

        public string SurveyId { get { return _formId; } set { _formId = value; } }

        public string SurveyNumber { get; set; }

        public string SurveyName { get; set; }

        public int SurveyType { get; set; }

        public string OrganizationName { get; set; }

        public string DepartmentName { get; set; }

        public string IntroductionText { get; set; }

        public string ExitText { get; set; }

        public string XML { get; set; }

        public bool IsSuccess { get; set; }

        public DateTime ClosingDate { get; set; }

        public Guid UserPublishKey { get; set; }

        public Guid OrganizationKey { get; set; }

        public bool IsDraftMode { get; set; }

        public DateTime StartDate { get; set; }

        public int ViewId { get; set; }

        public int OwnerId { get; set; }

        public bool IsSqlProject { get; set; }

        public string DBConnectionString { get; set; }

        public bool IsShareable { get; set; }

        public bool IsShared { get; set; }

        public bool HasDraftModeData { get; set; }

        public bool EwavLiteToggleSwitch { get; set; }
    }
}
