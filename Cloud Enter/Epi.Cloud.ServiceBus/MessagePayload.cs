using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.ServiceBus
{
    public struct MessagePropertyKeys
    {
        public const string ResponseId = "ResponseId";
        public const string FormId = "FormId";
        public const string FormName = "FormName";
        public const string UserOrgId = "UserOrgId";
        public const string IsDeleted = "IsDeleted";
    }

    [Serializable]
    public class MessagePayload
    {
        public IDictionary<string, object> Properties { get; set; }
        public string Body { get; set; }
    }
}
