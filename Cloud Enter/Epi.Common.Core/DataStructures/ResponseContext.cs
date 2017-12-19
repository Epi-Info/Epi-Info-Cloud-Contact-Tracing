using System;
using System.Collections.Generic;
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

        public bool IsChildResponse
        {
            get { return !string.IsNullOrEmpty(FormId) && !string.IsNullOrEmpty(RootFormId) && FormId != RootFormId ||
                !string.IsNullOrEmpty(ResponseId) && !string.IsNullOrEmpty(RootResponseId) && ResponseId != RootResponseId; }
        }

        public bool IsRootResponse
        {
            get { return !IsChildResponse; }
        }

        public Stack<ChildContext> ChildStack = null;

        public class ChildContext
        {
            public string ParentResponseId { get; set; }
            public string ChildFormName { get; set; }
            public string ChildResponseId { get; set; }
        }
    }
}
