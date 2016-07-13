using System.Runtime.Serialization;
using System.Collections.Generic;
using Epi.Web.Enter.Common.MessageBase;
using Epi.Web.Enter.Common.Criteria;
using Epi.Web.Enter.Common.DTO;
using Epi.Cloud.Common.EntityObjects;

namespace Epi.Web.Enter.Common.Message
{
    /// <summary>
    /// Represents a SurveyInfo request message from client.
    /// </summary>
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class SurveyAnswerRequest : RequestBase
    {
        public SurveyAnswerRequest()
        {
            this.Criteria = new SurveyAnswerCriteria();
            this.SurveyAnswerList = new List<SurveyAnswerDTO>();
            this.ResponseDetail = new FormResponseDetail();
        }

        /// <summary>
        /// Selection criteria and sort order
        /// </summary>
        [DataMember]
        public SurveyAnswerCriteria Criteria;

        /// <summary>
        /// SurveyInfo object.
        /// </summary>
        [DataMember]
        public List<SurveyAnswerDTO> SurveyAnswerList;

        [DataMember]
        public FormResponseDetail ResponseDetail;
    }
}
