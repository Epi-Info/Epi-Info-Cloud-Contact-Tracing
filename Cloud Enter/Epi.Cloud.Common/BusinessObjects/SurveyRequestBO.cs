using System;

namespace Epi.Cloud.Common.BusinessObjects
{

    public class SurveyRequestBO
    {
        public DateTime ClosingDate { get; set; }

        public bool IsSingleResponse { get; set; }

		public string SurveyName { get; set; }

		public string SurveyNumber { get; set; }

		public string OrganizationName { get; set; }

		public string DepartmentName { get; set; }

		public string IntroductionText { get; set; }

		public int SurveyType { get; set; }
	}
}
