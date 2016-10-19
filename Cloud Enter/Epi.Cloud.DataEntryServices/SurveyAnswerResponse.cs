using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Cloud.DataEntryServices
{
    /// <summary>
    /// Represents a SurveyInfo response message to client
    /// </summary>    
    public class SurveyAnswerResponse
    {
        /// <summary>
        /// Default Constructor for SurveyInfoResponse.
        /// </summary>
        public SurveyAnswerResponse() { this.SurveyResponseList = new List<SurveyAnswerDTO>(); }

        /// <summary>
        /// Overloaded Constructor for SurveyInfoResponse. Sets CorrelationId.
        /// </summary>
        /// <param name="correlationId"></param>
        public SurveyAnswerResponse(string correlationId)
		{
			this.SurveyResponseList = new List<SurveyAnswerDTO>();
		}

        /// <summary>
        /// Single SurveyInfo
        /// </summary>
        public List<SurveyAnswerDTO> SurveyResponseList;

        /// <summary>
        /// Total number of pages for query
        /// </summary>
        public int NumberOfPages { get; set; }

        /// <summary>
        /// Number of Records per page
        /// </summary>
        public int PageSize { get; set; }

        public int NumberOfResponses { get; set; }

        public FormInfoDTO FormInfo;
    }
}
