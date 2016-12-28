using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a SurveyInfo response message to client
    /// </summary>    
    public class SurveyInfoResponse: ResponseBase
    {
        /// <summary>
        /// Default Constructor for SurveyInfoResponse.
        /// </summary>
        public SurveyInfoResponse()
        {
            this.SurveyInfoList = new List<SurveyInfoDTO>();
        }

        /// <summary>
        /// Overloaded Constructor for SurveyInfoResponse. Sets CorrelationId.
        /// </summary>
        /// <param name="correlationId"></param>
        public SurveyInfoResponse(string correlationId) : base(correlationId) { this.SurveyInfoList = new List<SurveyInfoDTO>(); }

        /// <summary>
        /// Total number of pages for query
        /// </summary>
        public int NumberOfPages { get; set; }

        /// <summary>
        /// Number of Records per page
        /// </summary>
        public int PageSize { get; set; }


        /// <summary>
        /// Single SurveyInfo
        /// </summary>
        public List<SurveyInfoDTO> SurveyInfoList { get; set; }
    }
}
