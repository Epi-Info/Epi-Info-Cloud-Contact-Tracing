using System;
using Epi.Common.Core.Interfaces;

namespace Epi.Common.Core.DataStructures
{
    [Serializable]
    public class ResponseContext : IResponseContext
    {
        public string ResponseId { get; set; }
        public string FormId { get; set; }
        public string FormName { get; set; }

        public string ParentResponseId { get; set; }
        public string ParentFormId { get; set; }
        public string ParentFormName { get; set; }

        public string RootResponseId { get; set; }
        public string RootFormId { get; set; }
        public string RootFormName { get; set; }

        public bool IsNewRecord { get; set; }

        public int UserOrgId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }

        public bool IsRootForm
        {
            get
            {
                return (!string.IsNullOrEmpty(FormId) && FormId == RootFormId)
                    || (string.IsNullOrEmpty(FormId) && string.IsNullOrEmpty(ParentFormId));
            }
        }
        public bool IsRootResponse { get { return ResponseId == RootResponseId || IsRootForm; } }
        public bool IsChildResponse { get { return !IsRootResponse; } }
    }
}
