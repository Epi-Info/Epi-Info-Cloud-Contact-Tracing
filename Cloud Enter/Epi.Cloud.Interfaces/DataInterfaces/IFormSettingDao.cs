using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
    public interface IFormSettingDao
    {
        List<FormSettingBO> GetFormSettingsList(List<string> formIds, int currentOrgId);
        FormSettingBO GetFormSettings(string formId, int currentOrgId);
        FormSettingBO GetFormSettings();
        void UpdateColumnNames(FormSettingBO formSettingBO, string formId);
        void UpdateFormMode(FormInfoBO formInfoBO, FormSettingBO formSettingBO = null);
        void UpdateSettingsList(FormSettingBO formSettingBO, string formId);
        List<string> GetAllColumnNames(string formId);
        Dictionary<int, string> GetOrgAdmins(Dictionary<int, string> selectedOrgList);
        List<UserBO> GetOrgAdminsByFormId(string formId);
        void SoftDeleteForm(string formId);
        void DeleteDraftRecords(string formId);
    }
}
