using System;

namespace Epi.Cloud.Common.BusinessObjects
{
    public class SurveyInfoBO
    {
        public int DataAccessRuleId { get; set; }

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

        public bool IsDraftMode { get; set; }

        public bool IsShareable { get; set; }

        public DateTime StartDate { get; set; }

        public string ParentFormId { get; set; }

        public int ViewId { get; set; }

        public int OwnerId { get; set; }

        public bool IsSqlProject { get; set; }

        public string DBConnectionString { get; set; }

        public bool IsShared { get; set; }

        public bool HasDraftModeData { get; set; }
	}
}
