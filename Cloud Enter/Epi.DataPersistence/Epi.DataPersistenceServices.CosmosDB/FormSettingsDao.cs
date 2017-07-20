using System.Collections.Generic;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.Common.Interfaces;
using Epi.DataPersistence.Extensions;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public class DocDB_FormSettingsPersistenceFacade : IFormSettingsPersistenceFacade
    {
        private readonly DocumentDbCRUD _formResponseCRUD;

        public DocDB_FormSettingsPersistenceFacade()
        {
            _formResponseCRUD = new DocumentDbCRUD();
        }

        public List<ResponseDisplaySettings> GetResponseDisplaySettings(string formId)
        {
            return _formResponseCRUD.GetResponseGridColumns(formId);
        }

        public void UpdateResponseDisplaySettings(string formId, List<ResponseDisplaySettings> responseDisplaySettings)
        {
            _formResponseCRUD.SaveResponseGridColumnNames(formId, responseDisplaySettings);
        }

        public List<Epi.Common.Core.DataStructures.FormSettings> GetFormSettings(IEnumerable<string> formIds)
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
    }
}
