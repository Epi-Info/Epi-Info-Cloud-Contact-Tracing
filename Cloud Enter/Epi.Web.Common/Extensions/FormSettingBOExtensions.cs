using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Web.Enter.Common.Extensions
{
    public static class FormSettingBOExtensions
    {
        public static FormSettingDTO ToFormSettingDTO(this FormSettingBO formSettingBO)
        {
            return new FormSettingDTO
            {
                ColumnNameList = formSettingBO.ColumnNameList,
                FormControlNameList = formSettingBO.FormControlNameList,
                AssignedUserList = formSettingBO.AssignedUserList,
                UserList = formSettingBO.UserList,
                IsShareable = formSettingBO.IsShareable,
                AvailableOrgList = formSettingBO.AvailableOrgList,
                SelectedOrgList = formSettingBO.SelectedOrgList,
                IsDisabled = formSettingBO.IsDisabled,
                DataAccessRuleIds = formSettingBO.DataAccessRuleIds,
                SelectedDataAccessRule = formSettingBO.SelectedDataAccessRule,
                DataAccessRuleDescription = formSettingBO.DataAccessRuleDescription,
                DeleteDraftData = formSettingBO.DeleteDraftData
            };
        }
    }
}
