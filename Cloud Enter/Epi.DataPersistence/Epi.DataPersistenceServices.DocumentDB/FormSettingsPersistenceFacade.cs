using System;
using System.Collections.Generic;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.Common.Interfaces;
using Epi.DataPersistence.Extensions;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public class DocumentDBFormSettingsPersistenceFacade : IFormSettingsPersistenceFacade
    {
        public DocumentDBFormSettingsPersistenceFacade()
        {
        }

        DocumentDbCRUD _formResponseCRUD = new DocumentDbCRUD();

        public List<ResponseDisplaySettings> GetResponseDisplaySettings(string formId)
        {
            return _formResponseCRUD.GetResponseGridColumns(formId);
        }

        public void UpdateResponseDisplaySettings(string formId, List<ResponseDisplaySettings> responseDisplaySettings)
        {
            _formResponseCRUD.SaveResponseGridColumnNames(formId, responseDisplaySettings);
        }

        public Epi.Common.Core.DataStructures.FormSettings GetFormSettings(string formId)
        {
            var formSettingsProperties = _formResponseCRUD.GetFormSettingsProperties(formId);
            var formSettings = formSettingsProperties.ToFormSettings();
            return formSettings;
        }

        public void UpdateFormSettings(string formId, Epi.Common.Core.DataStructures.FormSettings formSettings)
        {
            _formResponseCRUD.UpdateFormSettings(formSettings);
        }
    }
}
