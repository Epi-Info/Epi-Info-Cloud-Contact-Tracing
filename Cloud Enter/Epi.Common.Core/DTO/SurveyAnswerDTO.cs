using System.Collections.Generic;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.Common.DTO
{
    public class SurveyAnswerDTO : SurveyAnswerStateDTO
    {
        public SurveyAnswerDTO()
        {
            ResponseDetail = new FormResponseDetail();
        }


        public Dictionary<string, string> SqlData { get; set; }

        public FormResponseDetail ResponseDetail { get; set; }
    }
}
