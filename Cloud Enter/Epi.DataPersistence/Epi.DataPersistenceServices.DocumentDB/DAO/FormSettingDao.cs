using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Cloud.SurveyInfoServices.Extensions;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.Common.Interfaces;

namespace Epi.DataPersistenceServices.DocumentDB.DAO
{
    public class FormSettingDao : IFormSettingDao
    {
        private readonly IFormSettingDao_EF _formSettingDao_EF;
        private readonly MetadataAccessor _metadataAccessor;
        private readonly DocumentDbCRUD _formResponseCRUD;

        public FormSettingDao(IFormSettingDao_EF formSettingDao_EF)
        {
            _metadataAccessor = new MetadataAccessor();
            _formResponseCRUD = new DocumentDbCRUD();
            _formSettingDao_EF = formSettingDao_EF;
        }

        public List<FormSettingBO> GetFormSettingsList(List<string> formIds, int currentOrgId, bool userAndOrgInfoOnly = true)
        {
            List<FormSettingBO> formSettingBOList = _formSettingDao_EF.GetFormSettingsList(formIds, currentOrgId, userAndOrgInfoOnly: true);

            GetAllColumnNames(formSettingBOList);

            GetDataAccessRules(formSettingBOList);

            return formSettingBOList;
        }

        private void GetAllColumnNames(List<FormSettingBO> formSettingBOList)
        {
            foreach (var formSettingBO in formSettingBOList)
            {
                int i = 1;
                formSettingBO.FormControlNameList = _metadataAccessor.GetAllColumnNames(formSettingBO.FormId).ToDictionary(k => i++, v => v);
            }
        }

        public FormSettingBO GetFormSettings(string formId, int currentOrgId, bool userAndOrgInfoOnly)
        {
            FormSettingBO formSettingBO = new FormSettingBO { FormId = formId };
            Dictionary<int, string> columnNameList = new Dictionary<int, string>();
            try
            {
                formSettingBO = _formSettingDao_EF.GetFormSettings(formId, currentOrgId, userAndOrgInfoOnly);
                int i = 1;
                formSettingBO.FormControlNameList = _metadataAccessor.GetAllColumnNames(formId).ToDictionary(k => i++, v => v);
                formSettingBO = GetDataAccessRules(formSettingBO);
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
                formSettingBO = GetDataAccessRules(formSettingBO);
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

            _formResponseCRUD.UpdateResponseGridColumnNames(formId, responseGridColumnSettingsList);
        }

        private static void GetDataAccessRules(out Dictionary<int, string> dataAccessRuleIds, out Dictionary<string, string> dataAccessRuleDescriptions)
        {
            dataAccessRuleIds = new Dictionary<int, string>();
            dataAccessRuleDescriptions = new Dictionary<string, string>();

            var ruleId = 0;
            string ruleName;
            string ruleDescription;
            var resourceManager = ResourceProvider.GetResourceManager(ResourceNamespaces.DataAccessRules);
            while ((ruleDescription = resourceManager.GetString((ruleName = "Rule" + ++ruleId))) != null)
            {
                dataAccessRuleIds.Add(ruleId, ruleName);
                dataAccessRuleDescriptions.Add(ruleName, ruleDescription);
            }
        }

        private static void GetDataAccessRules(List<FormSettingBO> formSettingBOList)
        {
            Dictionary<int, string> dataAccessRuleIds = new Dictionary<int, string>();
            Dictionary<string, string> dataAccessRuleDescriptions = new Dictionary<string, string>();
            GetDataAccessRules(out dataAccessRuleIds, out dataAccessRuleDescriptions);
            foreach (var formSettingBO in formSettingBOList)
            {
                formSettingBO.DataAccessRuleDescription = dataAccessRuleDescriptions;
                formSettingBO.DataAccessRuleIds = dataAccessRuleIds;
            }
        }


        private static FormSettingBO GetDataAccessRules(FormSettingBO formSettingBO)
        {
            Dictionary<int, string> dataAccessRuleIds = new Dictionary<int, string>();
            Dictionary<string, string> dataAccessRuleDescriptions = new Dictionary<string, string>();
            GetDataAccessRules(out dataAccessRuleIds, out dataAccessRuleDescriptions);
            formSettingBO.DataAccessRuleDescription = dataAccessRuleDescriptions;
            formSettingBO.DataAccessRuleIds = dataAccessRuleIds;
            return formSettingBO;
        }

        public void UpdateFormMode(FormInfoBO formInfoBO, FormSettingBO formSettingBO = null)
        {
            var formId = formInfoBO.FormId;
            if (string.IsNullOrEmpty(formInfoBO.FormName)) formInfoBO.FormName = _metadataAccessor.GetFormDigest(formId).FormName;
            var formSettings = formInfoBO.ToFormSettings();
            if (formSettingBO != null)
            {
                formSettings.ResponseDisplaySettings = formSettingBO.ResponseGridColumnNameList.OrderBy(k => k.Key)
                    .Select(kvp => new ResponseGridColumnSettings { FormId = formId, ColumnName = kvp.Value, SortOrder = kvp.Key })
                    .ToList();
            }

            _formResponseCRUD.UpdateFormSettings(formSettings);

            // Temporarily update WebEnter tables too
            _formSettingDao_EF.UpdateFormMode(formInfoBO);

#if false
            try
            {
                Guid id = new Guid(formInfoBO.FormId);

                //Update Form Mode
                using (var context = DataObjectFactory.CreateContext())
                {
                    var query = from response in context.SurveyMetaDatas
                                where response.SurveyId == id
                                select response;

                    var dataRow = query.Single();
                    dataRow.IsDraftMode = formInfoBO.IsDraftMode;
                    dataRow.IsShareable = formInfoBO.IsShareable;
                    dataRow.DataAccessRuleId = formInfoBO.DataAccesRuleId;

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
#endif
        }


        // vvvvvvvvvvvvvvvvvvvvvvvvvvv Implemented in Epi.Web.EF/EntityFormSettingDao vvvvvvvvvvvvvvvvvvvvvvvvvvv //
        public void UpdateSettingsList(FormSettingBO formSettingBO, string formId)
        {
            _formSettingDao_EF.UpdateSettingsList(formSettingBO, formId);
        }

        public Dictionary<int, string> GetOrgAdmins(Dictionary<int, string> selectedOrgList)
        {
            return _formSettingDao_EF.GetOrgAdmins(selectedOrgList);
        }

        public List<UserBO> GetOrgAdminsByFormId(string FormId)
        {
            return _formSettingDao_EF.GetOrgAdminsByFormId(FormId);
        }

        public void SoftDeleteForm(string formId)
        {
            _formSettingDao_EF.SoftDeleteForm(formId);
        }

        public void DeleteDraftRecords(string FormId)
        {
            throw new NotImplementedException("DeleteDraftRecords");
        }
    }
}
