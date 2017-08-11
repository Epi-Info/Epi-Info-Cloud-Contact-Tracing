using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Resources;
using Epi.Cloud.SurveyInfoServices.Extensions;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistenceServices.CosmosDB.DAO;

namespace Epi.DataPersistenceServices.CosmosDB.Facades
{
    public class CosmosDB_EF_FormSettingFacade : IFormSettingFacade
    {
        private readonly MetadataAccessor _metadataAccessor;
        private readonly CosmosDBCRUD _formResponseCRUD;
        private readonly IFormSettingDao_EF _formSettingDao_EF;
        private readonly IFormSettingDao _formSettingDao_CosmosDB;

        public CosmosDB_EF_FormSettingFacade(IFormSettingDao_EF formSettingDao_EF)
        {
            _metadataAccessor = new MetadataAccessor();
            _formResponseCRUD = new CosmosDBCRUD();
            _formSettingDao_EF = formSettingDao_EF;
            _formSettingDao_CosmosDB = new FormSettingDao(formSettingDao_EF);
        }

        public List<ResponseGridColumnSettings> GetResponseDisplaySettings(string formId)
        {
            return _formResponseCRUD.GetResponseGridColumns(formId);
        }

        public void UpdateResponseDisplaySettings(string formId, List<ResponseGridColumnSettings> responseDisplaySettings)
        {
            _formResponseCRUD.UpdateResponseGridColumnNames(formId, responseDisplaySettings);
        }

        public List<Epi.Common.Core.DataStructures.FormSettings> GetFormSettings(List<string> formIds)
        {
            var formSettingsPropertiesList = _formResponseCRUD.GetFormSettingsPropertiesList(formIds);
            var formSettingsList = formSettingsPropertiesList.ToFormSettingsList();
            return formSettingsList;
        }

        public Epi.Common.Core.DataStructures.FormSettings GetFormSettings(string formId)
        {
            var formSettingsProperties = _formResponseCRUD.GetFormSettingsProperties(formId);
            var formSettings = formSettingsProperties.ToFormSettings();
            return formSettings;
        }

        public void UpdateFormSettings(Epi.Common.Core.DataStructures.FormSettings formSettings)
        {
            _formResponseCRUD.UpdateFormSettings(formSettings);
        }
        public void UpdateFormSettings(IEnumerable<Epi.Common.Core.DataStructures.FormSettings> formSettingsList)
        {
            _formResponseCRUD.UpdateFormSettings(formSettingsList);
        }

        public List<FormSettingBO> GetFormSettingsList(List<string> formIds, int currentOrgId)
        {
            List<FormSettingBO> formSettingBOList = _formSettingDao_EF.GetFormSettingsList(formIds, currentOrgId, userAndOrgInfoOnly: true);

            foreach (var formSettingBO in formSettingBOList)
            {
                int i = 1;
                formSettingBO.FormControlNameList = GetAllColumnNames(formSettingBO.FormId).ToDictionary(k => i++, v => v);
            }

            DataAccessessRulesHelper.GetDataAccessRules(formSettingBOList);

            var formSettingsList = GetFormSettings(formIds);

            formSettingBOList = FormSettingsExtensions.MergeInfoFormSettingBOList(formSettingsList, formSettingBOList);
            return formSettingBOList;
        }

        public FormSettingBO GetFormSettings(string formId, int currentOrgId)
        {
            FormSettingBO formSettingBO = new FormSettingBO { FormId = formId };
            Dictionary<int, string> columnNameList = new Dictionary<int, string>();
            try
            {
                formSettingBO = _formSettingDao_EF.GetFormSettings(formId, currentOrgId, userAndOrgInfoOnly: true);
                int i = 1;
                formSettingBO.FormControlNameList = GetAllColumnNames(formId).ToDictionary(k => i++, v => v);
                formSettingBO = DataAccessessRulesHelper.GetDataAccessRules(formSettingBO);
                formSettingBO = GetFormSettings(formId).ToFormSettingBO(formSettingBO);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return formSettingBO;
        }

        public FormSettingBO GetFormSettings()
        {
            FormSettingBO formSettingBO = new FormSettingBO();

            try
            {
                formSettingBO = DataAccessessRulesHelper.GetDataAccessRules(formSettingBO);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return formSettingBO;
        }

        public List<string> GetAllColumnNames(string formId)
        {
            return _metadataAccessor.GetAllColumnNames(formId);
        }

        public void UpdateResponseGridColumnNames(FormSettingBO formSettingBO, string formId)
        {
            var responseGridColumnSettingsList = formSettingBO.ResponseGridColumnNameList
                .Select(n => new ResponseGridColumnSettings { ColumnName = n.Value, SortOrder = n.Key, FormId = formId })
                .ToList();

            UpdateResponseDisplaySettings(formId, responseGridColumnSettingsList);
        }

        public void UpdateFormMode(FormInfoBO formInfoBO, FormSettingBO formSettingBO = null)
        {
            _formSettingDao_CosmosDB.UpdateFormMode(formInfoBO, formSettingBO);
        }

        public void UpdateSettingsList(FormSettingBO formSettingBO, string formId)
        {
            _formSettingDao_EF.UpdateSettingsList(formSettingBO, formId);

        }

        public Dictionary<int, string> GetOrgAdmins(Dictionary<int, string> selectedOrgList)
        {
            return _formSettingDao_EF.GetOrgAdmins(selectedOrgList);
        }

        public List<UserBO> GetOrgAdminsByFormId(string formId)
        {
            return _formSettingDao_EF.GetOrgAdminsByFormId(formId);
        }

        public void SoftDeleteForm(string formId)
        {
            _formSettingDao_EF.SoftDeleteForm(formId);
        }

        public void DeleteDraftRecords(string formId)
        {
            throw new NotImplementedException("DeleteDraftRecords");
        }
    }
}
