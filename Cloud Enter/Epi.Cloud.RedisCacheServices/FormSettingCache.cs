using System;
using Epi.Cloud.Common.BusinessObjects;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache
    {
        private const string FormSettingKey = "FormSettings";

        private string ComposeFormSettingKey(Guid formId)
        {
            return FormSettingKey + FormSubKey + formId.ToString("N");
        }

        public bool FormFormSettingExists(Guid projectId, Guid formId)
        {
            return KeyExists(projectId, ComposeFormSettingKey(formId), NoTimeout).Result;
        }

        public FormSettingBO GetFormFormSetting(Guid projectId, Guid formId)
        {
            FormSettingBO formSettings = null;
            var json = Get(projectId, ComposeFormSettingKey(formId), NoTimeout).Result;
            if (json != null)
            {
                formSettings = JsonConvert.DeserializeObject<FormSettingBO>(json);
            }
            return formSettings;
        }

        public bool SetFormSetting(Guid projectId, Guid formId, FormSettingBO formSetting)
        {
            var json = JsonConvert.SerializeObject(formSetting, DontSerializeNulls);
            return Set(projectId, ComposeFormSettingKey(formId), json).Result;
        }

        public void ClearFormSetting(Guid projectId, Guid formId)
        {
            Delete(projectId, ComposeFormSettingKey(formId));
        }
    }
}
