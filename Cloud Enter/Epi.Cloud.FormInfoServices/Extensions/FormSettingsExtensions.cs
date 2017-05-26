using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Common.Core.DataStructures;

namespace Epi.Cloud.SurveyInfoServices.Extensions
{
    public static class FormSettingsExtensions
    {
        public static FormSettingBO ToFormSettingBO(this FormSettings formSettings, FormSettingBO formSettingBO = null)
        {
            if (formSettingBO == null) formSettingBO = new FormSettingBO();
            formSettingBO.SelectedDataAccessRule = formSettings.DataAccessRuleId;
            formSettingBO.IsDisabled = formSettings.IsDisabled;
            formSettingBO.IsShareable = formSettings.IsShareable;
            formSettingBO.ColumnNameList = formSettings.ResponseDisplaySettings.ToDictionary(k => k.SortOrder, v => v.ColumnName);
            return formSettingBO;
        }
    }
}
