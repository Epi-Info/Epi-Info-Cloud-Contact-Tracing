using System.Collections.Generic;
using Epi.FormMetadata.DataStructures;

namespace Epi.Web.Enter.Common.DTO
{
    public class FormSettingDTO
    {
        public FormSettingDTO()
        {
            SelectedDataAccessRule = 1;
        }

        public Dictionary<string, string> DataAccessRuleDescription { get; set; }

        public Dictionary<int, string> DataAccessRuleIds { get; set; }

        public int SelectedDataAccessRule { get; set; }

        public Dictionary<int, string> ColumnNameList { get; set; }

        public Dictionary<int, FieldDigest> ColumnDigestList { get; set; }

        public Dictionary<int, string> FormControlNameList { get; set; }

        public Dictionary<int, string> AssignedUserList { get; set; }

        public Dictionary<int, string> UserList { get; set; }

        public string FormId { get; set; }

        public bool IsShareable { get; set; }

        public Dictionary<int, string> AvailableOrgList { get; set; }

        public Dictionary<int, string> SelectedOrgList { get; set; }

        public bool IsDisabled { get; set; }

        public bool DeleteDraftData { get; set; }
    }
}
