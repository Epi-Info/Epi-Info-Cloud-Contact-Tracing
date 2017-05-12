using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.ServiceBus
{
    [Serializable]
    public class MessagePayload
    {
        public IDictionary<string, object> Properties { get; set; }
        public string Body { get; set; }
    }
}
