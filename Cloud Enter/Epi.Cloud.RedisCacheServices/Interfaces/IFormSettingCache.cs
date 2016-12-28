using System;
using Epi.Cloud.Common.BusinessObjects;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public interface IFormSettingCache
    {
        bool FormFormSettingExists(Guid projectId, Guid formId);

        FormSettingBO GetFormFormSetting(Guid projectId, Guid formId);

        bool SetFormSetting(Guid projectId, Guid formId, FormSettingBO formSetting);

        void ClearFormSetting(Guid projectId, Guid formId);
    }
}
