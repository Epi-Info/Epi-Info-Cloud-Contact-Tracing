using System.Collections.Generic;
using Epi.Cloud.Common.MessageBase;
using Epi.Cloud.Common.Criteria;
using Epi.Cloud.Common.DTO;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a SurveyInfo request message from client.
    /// </summary>
    public class SurveyAnswerRequest : RequestBase
    {
        public SurveyAnswerRequest()
        {
            this.Criteria = new SurveyAnswerCriteria();
            this.SurveyAnswerList = new List<SurveyAnswerDTO>();
        }

        /// <summary>
        /// Selection criteria and sort order
        /// </summary>
        public SurveyAnswerCriteria Criteria { get; set; }

        /// <summary>
        /// SurveyInfo object.
        /// </summary>
        public List<SurveyAnswerDTO> SurveyAnswerList { get; set; }

        public FormResponseDetail ResponseDetail { get; set; }
    }
}
