using System.Collections.Generic;
using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a SurveyInfo response message to client
    /// </summary>    
    public class FormsInfoResponse
    {
        /// <summary>
        /// Default Constructor for SurveyInfoResponse.
        /// </summary>
        public FormsInfoResponse()
        {
            this.FormInfoList = new List<FormInfoDTO>();
        }
        /// <summary>
        /// Single SurveyInfo
        /// </summary>
        public List<FormInfoDTO> FormInfoList;
    }
}
