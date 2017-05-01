using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Constants;
using Microsoft.Azure.Documents;

namespace Epi.PersistenceServices.DocumentDB
{
    public partial class DocumentResponseProperties
    {
        public DocumentResponseProperties()
        {
        }

        public string ResponseId { get; set; }
        public FormResponseProperties FormResponseProperties { get; set; }
        public bool IsChildForm { get; set; }
        public string FormName { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public partial class FormResponseResource : Resource
    {
        public class ResponseDirectory
        {
            public ResponseDirectory()
            {
            }

            public ResponseDirectory(FormResponseProperties formResponseProperties)
            {
                FormId = formResponseProperties.FormId;
                FormName = formResponseProperties.FormName;
                ParentFormId = formResponseProperties.ParentFormId;
                ParentFormName = formResponseProperties.ParentFormName;
                ParentResponseId = formResponseProperties.ParentResponseId;
            }
            public string FormId { get; set; }
            public string FormName { get; set; }
            public string ParentFormId { get; set; }
            public string ParentFormName { get; set; }
            public string ParentResponseId { get; set; }
        }
        public FormResponseResource()
        {
            ChildResponses = new Dictionary<string/*ParentResponseId*/, Dictionary<string/*ChildFormName*/, List<FormResponseProperties>>>();
            ChildResponseIndex = new Dictionary<string/*ResponseId*/, ResponseDirectory>();
        }

        public FormResponseProperties FormResponseProperties { get; set; }

        public Dictionary<string/*ParentResponseId*/, Dictionary<string/*ChildFormName*/, List<FormResponseProperties>>> ChildResponses { get; set; }
        public Dictionary<string/*ResponseId*/, ResponseDirectory> ChildResponseIndex { get; set; }

        public interface IFormResponseProperties
        {
            string ResponseId { get; set; }
            string FormId { get; set; }
            string FormName { get; set; }
            bool IsNewRecord { get; set; }
            int RecStatus { get; set; }
            string RelateParentResponseId { get; set; }
            string UserName { get; set; }
            string FirstSaveLogonName { get; set; }
            string LastSaveLogonName { get; set; }
            DateTime FirstSaveTime { get; set; }
            DateTime LastSaveTime { get; set; }
            int UserId { get; set; }
            bool IsDraftMode { get; set; }
            List<int> PageIds { get; set; }
            string RequiredFieldsList { get; set; }
            string HiddenFieldsList { get; set; }
            string HighlightedFieldsList { get; set; }
            string DisabledFieldsList { get; set; }

            Dictionary<string, string> ResponseQA { get; set; }

            bool IsRootForm { get; }
            bool IsRelatedView { get; }
        }
    }

    public partial class FormResponseProperties : IResponseContext
    {
        public FormResponseProperties()
        {
            IsNewRecord = true;
            RecStatus = 0;
            ResponseQA = new Dictionary<string, string>();
        }
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
        public int RecStatus { get; set; }
        public int LastPageVisited { get; set; }
        public string FirstSaveLogonName { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public DateTime LastSaveTime { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsLocked { get; set; }
        public string RequiredFieldsList { get; set; }
        public string HiddenFieldsList { get; set; }
        public string HighlightedFieldsList { get; set; }
        public string DisabledFieldsList { get; set; }

        public Dictionary<string, string> ResponseQA { get; set; }

        public bool IsRootForm { get { return ParentResponseId == null; } }
        public bool IsRelatedView { get { return ParentResponseId != null; } }

        public bool IsChildResponse { get { return ResponseId != RootResponseId; } }
        public bool IsRootResponse { get { return ResponseId == RootResponseId; } }

    }
}

