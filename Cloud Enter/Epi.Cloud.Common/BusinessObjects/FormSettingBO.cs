using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Epi.Cloud.Common.BusinessObjects
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class FormSettingBO
    {
        public Dictionary<string, string> DataAccessRuleDescription { get; set; }

        public Dictionary<int, string> DataAccessRuleIds { get; set; }

        public int SelectedDataAccessRule { get; set; }

        public Dictionary<int, string> ColumnNameList { get; set; }

        public Dictionary<int, string> FormControlNameList { get; set; }

        public bool IsShareable { get; set; }

        public bool IsDisabled { get; set; }


        public Dictionary<int, string> AssignedUserList { get; set; }

        public Dictionary<int, string> UserList { get; set; }


        public Dictionary<int, string> AvailableOrgList { get; set; }

        public Dictionary<int, string> SelectedOrgList { get; set; }


        public bool DeleteDraftData { get; set; }
    }
}
