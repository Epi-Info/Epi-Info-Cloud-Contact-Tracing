using System.Collections.Generic;
using Epi.Cloud.Common.Criteria;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a SurveyInfo request message from client.
    /// </summary>
    public class SurveyInfoRequest : RequestBase
    {
        public SurveyInfoRequest()
        {
            this.Criteria = new SurveyInfoCriteria();
            this.SurveyInfoList = new List<SurveyInfoDTO>();
        }

        /// <summary>
        /// Selection criteria and sort order
        /// </summary>
        public SurveyInfoCriteria Criteria { get; set; }


        /// <summary>
        /// SurveyInfo List.
        /// </summary>
        public List<SurveyInfoDTO> SurveyInfoList { get; set; }
    }
}
