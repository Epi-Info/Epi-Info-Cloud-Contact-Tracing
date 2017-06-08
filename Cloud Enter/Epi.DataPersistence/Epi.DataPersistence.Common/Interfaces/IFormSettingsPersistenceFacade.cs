using System.Collections.Generic;
using Epi.Common.Core.DataStructures;

namespace Epi.DataPersistence.Common.Interfaces
{
    public interface IFormSettingsPersistenceFacade
    {
        FormSettings GetFormSettings(string formId);
        List<FormSettings> GetFormSettings(IEnumerable<string> formIds);

        void UpdateFormSettings(FormSettings formSettings);
        void UpdateFormSettings(IEnumerable<FormSettings> formSettingsList);

        List<ResponseGridColumnSettings> GetResponseDisplaySettings(string formId);
        void UpdateResponseDisplaySettings(string formId, List<ResponseGridColumnSettings> responseDisplaySettings);
    }
}
