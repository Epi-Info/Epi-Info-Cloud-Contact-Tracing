using System.Collections.Generic;
using Epi.Common.Core.DataStructures;

namespace Epi.DataPersistence.Common.Interfaces
{
    public interface IFormSettingsPersistenceFacade
    {
        FormSettings GetFormSettings(string formId);
        void UpdateFormSettings(string formId, FormSettings formSettings);
   
        List<ResponseDisplaySettings> GetResponseDisplaySettings(string formId);
        void UpdateResponseDisplaySettings(string formId, List<ResponseDisplaySettings> responseDisplaySettings);
    }
}
