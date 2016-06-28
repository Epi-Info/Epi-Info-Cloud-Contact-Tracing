using System.Runtime.Serialization;

namespace Epi.Cloud.Common.BusinessObjects
{
    [DataContract]
    public class ResponseXmlBO
    {
        [DataMember]
        public string ResponseId { get; set; }

        [DataMember]
        public string Json { get; set; }

        [DataMember]
        public int User { get; set; }

        [DataMember]
        public bool IsNewRecord { get; set; }
    }
}
