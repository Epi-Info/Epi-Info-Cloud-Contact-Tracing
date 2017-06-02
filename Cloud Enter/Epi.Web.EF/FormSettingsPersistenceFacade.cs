using System;
using System.Collections.Generic;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.Common.Interfaces;

namespace Epi.Web.EF
{
    public class EF_FormSettingsPersistenceFacade : IFormSettingsPersistenceFacade
    {
        public List<FormSettings> GetFormSettings(IEnumerable<string> formIds)
        {
            throw new NotImplementedException();
        }

        public FormSettings GetFormSettings(string formId)
        {
            throw new NotImplementedException();
        }

        public List<ResponseDisplaySettings> GetResponseDisplaySettings(string formId)
        {
            throw new NotImplementedException();
        }

        public void UpdateFormSettings(IEnumerable<FormSettings> formSettingsList)
        {
            throw new NotImplementedException();
        }

        public void UpdateFormSettings(FormSettings formSettings)
        {
            throw new NotImplementedException();
        }

        public void UpdateResponseDisplaySettings(string formId, List<ResponseDisplaySettings> responseDisplaySettings)
        {
            throw new NotImplementedException();
        }
    }
}
