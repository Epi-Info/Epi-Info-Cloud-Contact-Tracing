using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
    public interface IFormSettingDao
    {
        FormSettingBO GetFormSettings(string FormId, int CurrentOrgId);
        FormSettingBO GetFormSettings();
        void UpDateColumnNames(FormSettingBO FormSettingBO, string FormId);
        void UpDateFormMode(FormInfoBO FormInfoBO);
        void UpDateSettingsList(FormSettingBO FormSettingBO, string FormId);
        List<string> GetAllColumnNames(string FormId);
        Dictionary<int, string> GetOrgAdmins(Dictionary<int, string> SelectedOrgList);
        List<UserBO> GetOrgAdminsByFormId(string FormId);
        void SoftDeleteForm(string FormId);
        void DeleteDraftRecords(string FormId);
    }
}
