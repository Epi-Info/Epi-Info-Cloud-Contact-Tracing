using System;
using System.Collections.Generic;
using Epi.FormMetadata.DataStructures;
using Epi.DataPersistence.Constants;

namespace Epi.Cloud.Common.Criteria
{
    /// <summary>
    /// Holds criteria for SurveyResponse queries.
    /// </summary>
    public class SurveyAnswerCriteria : Criteria
    {


        public SurveyAnswerCriteria()
        {
            this.SurveyAnswerIdList = new List<string>();
            this.StatusId = -1;
            this.DateCompleted = DateTime.MinValue;
        }

        /// <summary>
        /// Which page to retrieve
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of Records per page
        /// </summary>
        public int GridPageSize { get; set; }

        /// <summary>
        /// Number of Records per page
        /// </summary>
        public bool ReturnSizeInfoOnly { get; set; }

        /// <summary>
        /// Unique SurveyResponse identifier.
        /// </summary>
        public List<string> SurveyAnswerIdList { get; set; }

        /// <summary>
        /// SurveyInfo identifier.
        /// </summary>
        public string SurveyId { get; set; }

        /// <summary>
        /// Complete / Inprogress indicator
        /// </summary>
        public int StatusId { get; set; }


        /// <summary>
        /// IsCompleted date.
        /// </summary>
        public DateTime DateCompleted { get; set; }

        /// <summary>
        /// Flag as to whether to include order statistics.
        /// </summary>
        public bool IncludeOrderStatistics { get; set; }

        public Guid UserPublishKey { get; set; }

        public Guid OrganizationKey { get; set; }

        public bool IsMobile { get; set; }
        public int UserId { get; set; }

        public string UserName { get; set; }
        public bool IsEditMode { get; set; }

        public string SortOrder { get; set; }

        public string Sortfield { get; set; }

        public bool GetAllColumns { get; set; }

        public string SearchCriteria { get; set; }

        public string QuerySetToken { get; set; }

        public bool IsSqlProject { get; set; }
        public bool IsDeleteMode { get; set; }
        public bool IsDraftMode { get; set; }

        public bool IsShareable { get; set; }
        public int UserOrganizationId { get; set; }
        public bool IsHostOrganizationUser { get; set; }
        public int DataAccessRuleId { get; set; }

        public int? FormResponseCount { get; set; }

        public IDictionary<string, string> SurveyQAList { get; set; }
        public IDictionary<int, FieldDigest> FieldDigestList { get; set; }
        public IDictionary<int, KeyValuePair<FieldDigest, string>> SearchDigestList { get; set; }
        public RecordStatusChangeReason StatusChangeReason { get; set; }


        // Helpers
        public IDictionary<int, FieldDigest> GridFields { get { return FieldDigestList ?? new Dictionary<int, FieldDigest>(); } }
        public IDictionary<int, KeyValuePair<FieldDigest, string>> SearchFields { get { return SearchDigestList ?? new Dictionary<int, KeyValuePair<FieldDigest, string>>(); } }

        public bool SortOrderIsAscending { get { return string.IsNullOrWhiteSpace(SortOrder) || SortOrder.Trim().ToLower().StartsWith("asc"); } }
    }
}
