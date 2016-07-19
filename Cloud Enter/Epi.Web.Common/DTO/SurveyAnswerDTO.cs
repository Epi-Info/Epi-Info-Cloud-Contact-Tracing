using System.Collections.Generic;
using System.Runtime.Serialization;
using Epi.Cloud.Common.EntityObjects;

namespace Epi.Web.Enter.Common.DTO
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class SurveyAnswerDTO : SurveyAnswerStateDTO
    {
        public SurveyAnswerDTO()
        {
            ResponseDetail = new FormResponseDetail();
        }

        [DataMember]
        public List<SurveyAnswerDTO> ResponseHierarchyIds { get; set; }

        [DataMember]
        public Dictionary<string, string> SqlData { get; set; }

        [DataMember]
        public FormResponseDetail ResponseDetail { get; set; }

        [DataMember]
        public string XML { get; set; }
    }
}
