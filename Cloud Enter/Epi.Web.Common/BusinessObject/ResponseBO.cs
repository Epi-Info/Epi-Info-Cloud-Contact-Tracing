using System.Runtime.Serialization;
using Epi.Cloud.Common.EntityObjects;

namespace Epi.Web.Enter.Common.BusinessObject
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class ResponseBO
    {
        [DataMember]
        public string ResponseId { get; set; }

        [DataMember]
        public int User { get; set; }

        [DataMember]
        public bool IsNewRecord { get; set; }

        [DataMember]
        public FormResponseDetail ResponseDetail { get; set; }

        [DataMember]
        public string Xml { get; set; }
    }
}
