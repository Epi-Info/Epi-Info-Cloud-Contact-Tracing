using System;
using System.Collections.Generic;

namespace Epi.Cloud.Common.EntityObjects
{
    public partial class FormResponseDetail
    {
        public string GlobalRecordID { get; set; }
        public string FormId { get; set; }
        public string FormName { get; set; }
        public int RecStatus { get; set; }
        public string RelateParentId { get; set; }
        public string FirstSaveLogonName { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public DateTime LastSaveTime { get; set; }
        public int LastActiveUserId { get; set; }
        public bool IsRelatedView { get; set; }
        public bool IsDraftMode { get; set; }
        public List<int> PageIds { get; set; }

        public int LastPageVisited { get; set; }

        public string RequiredFieldsList { get; set; }
        public string HiddenFieldsList { get; set; }
        public string HighlightedFieldsList { get; set; }
        public string DisabledFieldsList { get; set; }
        public List<PageResponseDetail> PageResponseDetailList{ get; private set; }

        public List<FormResponseDetail> ChildFormResponseDetailList { get; private set; }
    }

}
