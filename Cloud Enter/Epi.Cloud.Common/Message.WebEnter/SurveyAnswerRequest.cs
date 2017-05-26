using System.Collections.Generic;
using Epi.Cloud.Common.MessageBase;
using Epi.Cloud.Common.Criteria;
using Epi.Cloud.Common.DTO;
using Epi.Common.Core.Interfaces;
using Epi.Common.Core.DataStructures;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a SurveyInfo request message from client.
    /// </summary>
    public class SurveyAnswerRequest : RequestBase, IResponseContext
    {
        public SurveyAnswerRequest()
        {
            this.Criteria = new SurveyAnswerCriteria();
            this.SurveyAnswerList = new List<SurveyAnswerDTO>();
            ResponseContext = new ResponseContext();
        }

        public IResponseContext ResponseContext { get; set; }

        /// <summary>
        /// Selection criteria and sort order
        /// </summary>
        public SurveyAnswerCriteria Criteria { get; set; }

        /// <summary>
        /// SurveyInfo object.
        /// </summary>
        public List<SurveyAnswerDTO> SurveyAnswerList { get; set; }

        public string ResponseId
        {
            get { return ResponseContext.ResponseId; }
            set { ResponseContext.ResponseId = value; }
        }

        public string FormId
        {
            get { return ResponseContext.FormId; }
            set { ResponseContext.FormId = value; }
        }

        public string FormName
        {
            get { return ResponseContext.FormName; }
            set { ResponseContext.FormName = value; }
        }

        public string ParentResponseId
        {
            get { return ResponseContext.ParentResponseId; }
            set { ResponseContext.ParentResponseId = value; }
        }

        public string ParentFormId
        {
            get { return ResponseContext.ParentFormId; }
            set { ResponseContext.ParentFormId = value; }
        }

        public string ParentFormName
        {
            get { return ResponseContext.ParentFormName; }
            set { ResponseContext.ParentFormName = value; }
        }

        public string RootResponseId
        {
            get { return ResponseContext.RootResponseId; }
            set { ResponseContext.RootResponseId = value; }
        }

        public string RootFormId
        {
            get { return ResponseContext.RootFormId; }
            set { ResponseContext.RootFormId = value; }
        }

        public string RootFormName
        {
            get { return ResponseContext.RootFormName; }
            set { ResponseContext.RootFormName = value; }
        }

        public bool IsNewRecord
        {
            get { return ResponseContext.IsNewRecord; }
            set { ResponseContext.IsNewRecord = value; }
        }
        public int OrgId
        {
            get { return ResponseContext.OrgId; }
            set { ResponseContext.OrgId = value; }
        }

        public int UserId
        {
            get { return ResponseContext.UserId; }
            set { ResponseContext.UserId = value; }
        }

        public string UserName
        {
            get { return ResponseContext.UserName; }
            set { ResponseContext.UserName = value; }
        }

        public bool IsChildResponse { get { return ResponseId != RootResponseId; } }
        public bool IsRootResponse { get { return ResponseId == RootResponseId; } }
    }
}
