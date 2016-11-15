using System.Collections.Generic;
using Epi.DataPersistence.DataStructures;

namespace Epi.Web.Enter.Common.DTO
{
    public class SurveyAnswerDTO : SurveyAnswerStateDTO
    {
        public SurveyAnswerDTO()
        {
            ResponseDetail = new FormResponseDetail();
        }

        public List<SurveyAnswerDTO> ResponseHierarchyIds { get; set; }

        public Dictionary<string, string> SqlData { get; set; }

        public FormResponseDetail ResponseDetail { get; set; }
    }
}
