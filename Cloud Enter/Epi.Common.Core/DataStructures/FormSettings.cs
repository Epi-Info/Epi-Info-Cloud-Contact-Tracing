using System.Collections.Generic;

namespace Epi.Common.Core.DataStructures
{
    public class FormSettings
    {
        public string FormId { get; set; }
        public string FormName { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsShareable { get; set; }
        public int DataAccessRuleId { get; set; }
        public List<ResponseDisplaySettings> ResponseDisplaySettings { get; set; }
    }
}
