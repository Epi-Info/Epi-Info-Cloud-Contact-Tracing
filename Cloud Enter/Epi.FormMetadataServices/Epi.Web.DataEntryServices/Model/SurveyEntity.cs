using System;
using System.Collections.Generic;
using Epi.Cloud.Common.Constants;
using Microsoft.Azure.Documents;

namespace Epi.Cloud.DataEntryServices.Model
{
    public class HierarchicalDocumentResponseProperties
    {
        public HierarchicalDocumentResponseProperties()
        {
            ChildResponseList = new List<HierarchicalDocumentResponseProperties>();
            PageResponsePropertiesList = new List<PageResponseProperties>();
        }

        public HierarchicalDocumentResponseProperties ParentResponse { get; set; }
        public List<HierarchicalDocumentResponseProperties> ChildResponseList { get; set; }
        public FormResponseProperties FormResponseProperties { get; set; }
        public List<PageResponseProperties> PageResponsePropertiesList { get; set; }
    }

    public class DocumentResponseProperties
    {
        public DocumentResponseProperties()
        {
            PageResponsePropertiesList = new List<Model.PageResponseProperties>();
        }

        public string GlobalRecordID { get; set; }
        public FormResponseProperties FormResponseProperties { get; set; }
        public List<PageResponseProperties> PageResponsePropertiesList { get; set; }
        public bool IsChildForm { get; set; }
        public string FormName { get; set; }
        public string CollectionName { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
    }
   
    public class FormResponseProperties : Resource
    {
        public FormResponseProperties()
        {
            RecStatus = RecordStatus.InProcess;
            PageIds = new List<int>();
        }
        public string GlobalRecordID { get; set; }
        public string FormId { get; set; }
        public string FormName { get; set; }
        public int RecStatus { get; set; }
        public string RelateParentId { get; set; }
        public string FirstSaveLogonName { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public DateTime LastSaveTime { get; set; }
        public int UserId { get; set; }
        public bool IsRelatedView { get; set; }
        public bool IsDraftMode { get; set; }
        public List<int> PageIds { get; set; }

        public string id { get; set; }
        public string _self { get; set; }
        public Int64 _ts { get; set; }
    }

    public class PageResponseProperties : Resource
    {
        public string GlobalRecordID { get; set; }
        public int PageId { get; set; }
        public Dictionary<string, string> ResponseQA { get; set; }
    }
}