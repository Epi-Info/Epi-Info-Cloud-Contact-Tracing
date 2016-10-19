using System.Collections.Generic;
using System.Runtime.Serialization;
using Epi.Cloud.Common.DTO;

namespace Epi.Web.Enter.Common.DTO
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class FormsHierarchyDTO
    {
        [DataMember]
        public string RootFormId { get; set; }

		[DataMember]
        public string FormId { get; set; }

		[DataMember]
        public List<SurveyAnswerDTO> ResponseIds { get; set; }

		[DataMember]
        public bool IsRoot { get; set; }

		[DataMember]
        public int ViewId { get; set; }

		[DataMember]
        public bool IsSqlProject { get; set; }

		[DataMember]
		public SurveyInfoDTO SurveyInfo { get; set; }
	}
}
