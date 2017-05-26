using System.Collections.Generic;
using Microsoft.Azure.Documents;

namespace Epi.DataPersistenceServices.DocumentDB.FormSettings
{
    public class FormSettingsResource : Resource
    {
        public FormSettingsProperties FormSettingsProperties { get; set; }
    }

    public class FormSettingsProperties
    {
        public string FormId { get; set; }
        public string FormName { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsShareable { get; set; }
        public int DataAccessRuleId { get; set; }
        public List<string> ResponseGridColumnNames { get; set; }
    }
}
