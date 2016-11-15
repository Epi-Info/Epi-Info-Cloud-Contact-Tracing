using System;
using Epi.Cloud.Common.Metadata;

namespace Epi.Web.Enter.Common.Model
{
    /// <summary>
    /// The Survey Model that will be pumped to view
    /// </summary>
    public class SurveyInfoModel : MetadataAccessor
    {
        public string SurveyId { get { return _formId; } set { _formId = value; } }

        public string SurveyNumber { get; set; }

        public string SurveyName { get; set; }

        public string OrganizationName { get; set; }

        public string DepartmentName { get; set; }

        public string IntroductionText { get; set; }

        public string ExitText { get; set; }

        public bool IsSuccess { get; set; }

        public bool IsSqlProject { get; set; }

        public DateTime ClosingDate { get; set; }

        public int SurveyType { get; set; }

        public Guid UserPublishKey { get; set; }

        public DateTime StartDate { get; set; }

        public bool IsDraftMode { get; set; }

        public string IsDraftModeStyleClass { get; set; }

        public int FormOwnerId { get; set; }
    }
}