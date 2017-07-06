using System;
using System.Collections.Generic;
using Epi.Common.Core.Interfaces;

namespace Epi.DataPersistence.DataStructures
{
    [Serializable]
    public partial class FormResponseDetail : IResponseContext
    {
        public FormResponseDetail()
        {
            RequiredFieldsList = string.Empty;
            HiddenFieldsList = string.Empty;
            HighlightedFieldsList = string.Empty;
            DisabledFieldsList = string.Empty;
            PageResponseDetailList = new List<PageResponseDetail>();
            ChildFormResponseDetailList = new List<FormResponseDetail>();
            PageIds = new List<int>();
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

        public int RecStatus { get; set; }
        public string FirstSaveLogonName { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public DateTime LastSaveTime { get; set; }
        public int UserOrgId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsRelatedView { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsLocked { get; set; }
        public bool IsNewRecord { get; set; }
        public List<int> PageIds { get; set; }

        public int LastPageVisited { get; set; }

        public string RequiredFieldsList { get; set; }
        public string HiddenFieldsList { get; set; }
        public string HighlightedFieldsList { get; set; }
        public string DisabledFieldsList { get; set; }

        public List<PageResponseDetail> PageResponseDetailList { get; private set; }

        public List<FormResponseDetail> ChildFormResponseDetailList { get; private set; }

        public bool IsChildResponse { get { return IsRelatedView; } }

        public bool IsRootResponse { get { return !IsRelatedView; } }
    }
}
