using System;
using System.Runtime.Serialization;

namespace Epi.Web.Enter.Common.BusinessObject
{
    public class SurveyInfoBO
    {
        public int DataAccessRuleId { get; set; }

        public string StatusText { get; set; }

		public string SurveyId { get; set; }

		public string SurveyNumber { get; set; }

		public int SurveyType { get; set; }

		public string SurveyName { get; set; }

		public string OrganizationName { get; set; }

		public string DepartmentName { get; set; }

		public string IntroductionText { get; set; }

		public string ExitText { get; set; }

		public DateTime ClosingDate { get; set; }

		public Guid UserPublishKey { get; set; }

		public Guid OrganizationKey { get; set; }

		public DateTime DateCreated { get; set; }

		[DataMember]
        public bool IsDraftMode { get; set; }

		[DataMember]
        public bool IsShareable { get; set; }

		[DataMember]
        public DateTime StartDate { get; set; }

		[DataMember]
        public string ParentId { get; set; }

		[DataMember]
        public int ViewId { get; set; }

		[DataMember]
        public int OwnerId { get; set; }

		[DataMember]
        public bool IsSqlProject { get; set; }

		[DataMember]
        public string DBConnectionString { get; set; }

		[DataMember]
        public bool IsShared { get; set; }

		[DataMember]
        public bool HasDraftModeData { get; set; }

		[DataMember]
        public bool EwavLiteToggleSwitch { get; set; }
	}
}
