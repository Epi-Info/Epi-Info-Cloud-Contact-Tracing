using System.Collections.Generic;

namespace Epi.Common.Core.DataStructures
{
    public class FormSettings
    {
        public string FormId { get; set; }
        public string FormName { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsShareable { get; set; }
        public int DataAccessRuleId { get; set; }
        public List<ResponseGridColumnSettings> ResponseDisplaySettings { get; set; }

        public Dictionary<int, string> AssignedUserList { get; set; }
        public Dictionary<int, string> SelectedOrgList { get; set; }

        public bool DeleteDraftData { get; set; }
    }
}
