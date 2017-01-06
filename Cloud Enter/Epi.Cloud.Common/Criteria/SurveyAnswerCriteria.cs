﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Epi.FormMetadata.DataStructures;
using Epi.DataPersistence.Constants;

namespace Epi.Cloud.Common.Criteria
{
    /// <summary>
    /// Holds criteria for SurveyResponse queries.
    /// </summary>
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
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
        [DataMember]
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of Records per page
        /// </summary>
        [DataMember]
        public int GridPageSize { get; set; }

        /// <summary>
        /// Number of Records per page
        /// </summary>
        [DataMember]
        public bool ReturnSizeInfoOnly { get; set; }


        /// <summary>
        /// Unique SurveyResponse identifier.
        /// </summary>
        [DataMember]
        public List<string> SurveyAnswerIdList { get; set; }

        /// <summary>
        /// SurveyInfo identifier.
        /// </summary>
        [DataMember]
        public string SurveyId { get; set; }

        /// <summary>
        /// Complete / Inprogress indicator
        /// </summary>
        [DataMember]
        public int StatusId { get; set; }


        /// <summary>
        /// IsCompleted date.
        /// </summary>
        [DataMember]
        public DateTime DateCompleted { get; set; }

        /// <summary>
        /// Flag as to whether to include order statistics.
        /// </summary>
        [DataMember]
        public bool IncludeOrderStatistics { get; set; }


        [DataMember]
        public Guid UserPublishKey { get; set; }

        [DataMember]
        public Guid OrganizationKey { get; set; }

        [DataMember]
        public bool IsMobile { get; set; }
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public bool IsEditMode { get; set; }

        [DataMember]
        public string SortOrder { get; set; }

        [DataMember]
        public string Sortfield { get; set; }

        [DataMember]
        public bool GetAllColumns { get; set; }

        [DataMember]
        public string SearchCriteria { get; set; }

        [DataMember]
        public bool IsSqlProject { get; set; }
        [DataMember]
        public bool IsDeleteMode { get; set; }
        [DataMember]
        public bool IsDraftMode { get; set; }

        [DataMember]
        public bool IsShareable { get; set; }
        [DataMember]
        public int UserOrganizationId { get; set; }

        [DataMember]
        public int DataAccessRuleId
        {
            get;
            set;
        }

        public int? FormResponseCount { get; set; }

        public IDictionary<string, string> SurveyQAList { get; set; }
        public IDictionary<int, FieldDigest> FieldDigestList { get; set; }
		public RecordStatusChangeReason StatusChangeReason { get; set; }
	}
}