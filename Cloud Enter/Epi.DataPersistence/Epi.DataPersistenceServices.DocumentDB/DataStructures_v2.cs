using System;
using System.Collections.Generic;
using Epi.DataPersistence.Constants;
using Microsoft.Azure.Documents;

namespace Epi.PersistenceServices.DocumentDB
{
    public partial class DataStructures
    {
        public class HierarchicalDocumentResponseProperties
        {
#if DocDbV2
            public HierarchicalDocumentResponseProperties()
            {
                ChildResponseList = new List<HierarchicalDocumentResponseProperties>();
                PageResponsePropertiesList = new List<PageResponseProperties>();
            }

            public List<HierarchicalDocumentResponseProperties> ChildResponseList { get; set; }
            public FormResponseProperties FormResponseProperties { get; set; }
            public List<PageResponseProperties> PageResponsePropertiesList { get; set; }
        }

        public class DocumentResponseProperties
        {
            public DocumentResponseProperties()
            {
            }

            public string GlobalRecordID { get; set; }
            public FormResponseProperties FormResponseProperties { get; set; }
            public bool IsChildForm { get; set; }
            public string FormName { get; set; }

            public int UserId { get; set; }
            public string UserName { get; set; }
        }

        public class FormResponseResource : Resource
        {
            public FormResponseProperties FormResponse { get; set; }
        }

        public interface IFormResponseProperties
        {
            bool IsRootForm { get; set; }
            string GlobalRecordID { get; set; }
            string FormId { get; set; }
            string FormName { get; set; }
            bool IsNewRecord { get; set; }
            int RecStatus { get; set; }
            string ParentResponseId { get; set; }
            string UserName { get; set; }
            string FirstSaveLogonName { get; set; }
            string LastSaveLogonName { get; set; }
            DateTime FirstSaveTime { get; set; }
            DateTime LastSaveTime { get; set; }
            int UserId { get; set; }
            bool IsRelatedView { get; set; }
            bool IsDraftMode { get; set; }
            List<int> PageIds { get; set; }
            string RequiredFieldsList { get; set; }
            string HiddenFieldsList { get; set; }
            string HighlightedFieldsList { get; set; }
            string DisabledFieldsList { get; set; }

            Dictionary<string, string> ResponseQA { get; set; }
            Dictionary<string, FormResponseProperties> ChildFormResponseProperties { get; set; }
        }

        public partial class FormResponseProperties : IFormResponseProperties
        {
            public FormResponseProperties()
            {
                IsNewRecord = true;
                RecStatus = RecordStatus.InProcess;
                PageIds = new List<int>();
                ResponseQA = new Dictionary<string, string>();
                ChildFormResponseProperties = new Dictionary<string, FormResponseProperties>();
            }
            public bool IsRootForm { get; set; }
            public string GlobalRecordID { get; set; }
            public string FormId { get; set; }
            public string FormName { get; set; }
            public bool IsNewRecord { get; set; }
            public int RecStatus { get; set; }
            public string ParentResponseId { get; set; }
            public string UserName { get; set; }
            public string FirstSaveLogonName { get; set; }
            public string LastSaveLogonName { get; set; }
            public DateTime FirstSaveTime { get; set; }
            public DateTime LastSaveTime { get; set; }
            public int UserId { get; set; }
            public bool IsRelatedView { get; set; }
            public bool IsDraftMode { get; set; }
            public List<int> PageIds { get; set; }
            public string RequiredFieldsList { get; set; }
            public string HiddenFieldsList { get; set; }
            public string HighlightedFieldsList { get; set; }
            public string DisabledFieldsList { get; set; }
            public Dictionary<string, string> ResponseQA { get; set; }
            public Dictionary<string, FormResponseProperties> ChildFormResponseProperties { get; set; }
#endif
        }
    }
}
