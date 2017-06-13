using System;
using System.Collections.Generic;
using Epi.Common.Core.DataStructures;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.Facades
{
    public class EF_FormSettingFacade : IFormSettingFacade
    {
        private readonly MetadataAccessor _metadataAccessor;
        private readonly IFormSettingDao_EF _formSettingDao;

        public EF_FormSettingFacade(IFormSettingDao_EF formSettingDao)
        {
            _metadataAccessor = new MetadataAccessor();
            _formSettingDao = formSettingDao;
        }

        public List<string> GetAllColumnNames(string formId)
        {
            return _metadataAccessor.GetAllColumnNames(formId);
        }

        public FormSettingBO GetFormSettings()
        {
            return _formSettingDao.GetFormSettings();
        }

        public FormSettingBO GetFormSettings(string formId, int currentOrgId)
        {
            return _formSettingDao.GetFormSettings(formId, currentOrgId, userAndOrgInfoOnly: false);
        }

        public List<FormSettingBO> GetFormSettingsList(List<string> formIds, int currentOrgId)
        {
            return _formSettingDao.GetFormSettingsList(formIds, currentOrgId, userAndOrgInfoOnly: false);
        }

        public Dictionary<int, string> GetOrgAdmins(Dictionary<int, string> selectedOrgList)
        {
            return _formSettingDao.GetOrgAdmins(selectedOrgList);
        }

        public List<UserBO> GetOrgAdminsByFormId(string formId)
        {
            return _formSettingDao.GetOrgAdminsByFormId(formId);
        }

        public void SoftDeleteForm(string formId)
        {
            _formSettingDao.SoftDeleteForm(formId);
        }

        public void UpdateResponseGridColumnNames(FormSettingBO formSettingBO, string formId)
        {
            _formSettingDao.UpdateResponseGridColumnNames(formSettingBO, formId);
        }

        public void UpdateFormMode(FormInfoBO formInfoBO, FormSettingBO formSettingBO = null)
        {
            _formSettingDao.UpdateFormMode(formInfoBO, formSettingBO);
        }

        public void UpdateFormSettings(IEnumerable<FormSettings> formSettingsList)
        {
            throw new NotImplementedException();
        }

        public void UpdateFormSettings(FormSettings formSettings)
        {
            throw new NotImplementedException();
        }

        public void UpdateResponseDisplaySettings(string formId, List<ResponseGridColumnSettings> responseDisplaySettings)
        {
            throw new NotImplementedException();
        }

        public void UpdateSettingsList(FormSettingBO formSettingBO, string formId)
        {
            _formSettingDao.UpdateSettingsList(formSettingBO, formId);
        }

        public void DeleteDraftRecords(string formId)
        {
            throw new NotImplementedException();
        }
    }
}
