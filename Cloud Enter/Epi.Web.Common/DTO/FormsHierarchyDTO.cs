using System.Collections.Generic;

namespace Epi.Web.Enter.Common.DTO
{
    public class FormsHierarchyDTO
    {
        public string RootFormId { get; set; }

        public string FormId { get; set; }

        public List<SurveyAnswerDTO> ResponseIds { get; set; }

        public bool IsRoot { get; set; }

        public int ViewId { get; set; }

        public bool IsSqlProject { get; set; }

		public SurveyInfoDTO SurveyInfo { get; set; }
	}
}
