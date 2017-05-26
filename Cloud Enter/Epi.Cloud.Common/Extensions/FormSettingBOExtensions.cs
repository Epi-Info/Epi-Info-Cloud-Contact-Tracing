using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Common.Core.DataStructures;

namespace Epi.Cloud.Common.Extensions
{
    public static class FormSettingBOExtensions
    {
        public static FormSettingDTO ToFormSettingDTO(this FormSettingBO formSettingBO)
        {
            return new FormSettingDTO
            {
                UserList = formSettingBO.UserList,
                AssignedUserList = formSettingBO.AssignedUserList,
                AvailableOrgList = formSettingBO.AvailableOrgList,
                SelectedOrgList = formSettingBO.SelectedOrgList,

                ColumnNameList = formSettingBO.ColumnNameList,
                FormControlNameList = formSettingBO.FormControlNameList,

                SelectedDataAccessRule = formSettingBO.SelectedDataAccessRule,
                DataAccessRuleIds = formSettingBO.DataAccessRuleIds,
                DataAccessRuleDescription = formSettingBO.DataAccessRuleDescription,
                IsShareable = formSettingBO.IsShareable,
                IsDisabled = formSettingBO.IsDisabled,

                DeleteDraftData = formSettingBO.DeleteDraftData
            };
        }
    }
}
