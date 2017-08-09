using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Resources.Constants;
using System.Globalization;
using System.Resources;
using System.Collections;

namespace Epi.Cloud.Resources
{
    public static class DataAccessessRulesHelper
    {
        public static void GetDataAccessRules(out Dictionary<int, string> dataAccessRuleIds, out Dictionary<string, string> dataAccessRuleDescriptions)
        {
            dataAccessRuleIds = new Dictionary<int, string>();
            dataAccessRuleDescriptions = new Dictionary<string, string>();

            var ruleId = 0;
            string ruleName;
            string ruleDescription;
            var resourceManager = ResourceProvider.GetResourceManager(ResourceNamespaces.DataAccessRules);
            ResourceSet resourceSet= resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);            
            foreach (DictionaryEntry entry in resourceSet)
            {
                ruleId++;
                dataAccessRuleIds.Add(ruleId, entry.Key.ToString());
                dataAccessRuleDescriptions.Add(entry.Key.ToString(), entry.Value.ToString());               
            }           
        }

        public static void GetDataAccessRules(List<FormSettingBO> formSettingBOList)
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


        public static FormSettingBO GetDataAccessRules(FormSettingBO formSettingBO)
        {
            Dictionary<int, string> dataAccessRuleIds = new Dictionary<int, string>();
            Dictionary<string, string> dataAccessRuleDescriptions = new Dictionary<string, string>();
            GetDataAccessRules(out dataAccessRuleIds, out dataAccessRuleDescriptions);
            formSettingBO.DataAccessRuleDescription = dataAccessRuleDescriptions;
            formSettingBO.DataAccessRuleIds = dataAccessRuleIds;
            return formSettingBO;
        }
    }
}
