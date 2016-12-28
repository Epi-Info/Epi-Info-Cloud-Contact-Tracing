using System;
using System.Runtime.Serialization;

namespace Epi.Cloud.Common.Message
{
    public class SurveyRequest
    {
        public DateTime ClosingDate { get; set; }

        public bool IsSingleResponse { get; set; }

        public string SurveyName { get; set; }

        public string SurveyNumber { get; set; }

        public string OrganizationName { get; set; }

        public string DepartmentName { get; set; }

        public string IntroductionText { get; set; }
    }
}
