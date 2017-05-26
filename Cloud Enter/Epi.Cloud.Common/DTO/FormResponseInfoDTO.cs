using System;
using System.Runtime.Serialization;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;

namespace Epi.Cloud.Common.DTO
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class FormResponseInfoDTO : IResponseContext
    {
        public FormResponseInfoDTO()
        {
            ResponseContext = new ResponseContext();
        }

        public ResponseContext ResponseContext { get; set; }

        [DataMember]
        public string ResponseId { get { return ResponseContext.ResponseId; } set { ResponseContext.ResponseId = value; } }
        [DataMember]
        public string SurveyId { get { return ResponseContext.FormId; } set { ResponseContext.FormId = value; } }
        [DataMember]
        public DateTime DateUpdated { get; set; }
        [DataMember]
        public DateTime? DateCompleted { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public Guid UserPublishKey { get; set; }

        [DataMember]
        public bool IsDraftMode { get; set; }

        public string FormId { get { return ResponseContext.FormId; } set { ResponseContext.FormId = value; } }

        public string FormName { get { return ResponseContext.FormName; } set { ResponseContext.FormName = value; } }

        public string ParentResponseId { get { return ResponseContext.ParentResponseId; } set { ResponseContext.ParentResponseId = value; } }

        public string ParentFormId { get { return ResponseContext.ParentFormId; } set { ResponseContext.ParentFormId = value; } }

        public string ParentFormName { get { return ResponseContext.ParentFormName; } set { ResponseContext.ParentFormName = value; } }

        public string RootResponseId { get { return ResponseContext.RootResponseId; } set { ResponseContext.RootResponseId = value; } }

        public string RootFormId { get { return ResponseContext.RootFormId; } set { ResponseContext.RootFormId = value; } }

        public string RootFormName { get { return ResponseContext.RootFormName; } set { ResponseContext.RootFormName = value; } }

        public bool IsNewRecord { get { return ResponseContext.IsNewRecord; } set { ResponseContext.IsNewRecord = value; } }

        public int OrgId { get { return ResponseContext.OrgId; } set { ResponseContext.OrgId = value; } }

        public int UserId { get { return ResponseContext.UserId; } set { ResponseContext.UserId = value; } }

        public string UserName { get { return ResponseContext.UserName; } set { ResponseContext.UserName = value; } }

        public bool IsChildResponse{ get { return ResponseContext.IsChildResponse; } }
        public bool IsRootResponse { get { return ResponseContext.IsRootResponse; } }
    }
}
