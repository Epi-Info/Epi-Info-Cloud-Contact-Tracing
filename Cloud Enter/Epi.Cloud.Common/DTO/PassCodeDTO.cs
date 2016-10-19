using System.Runtime.Serialization;


namespace Epi.Cloud.Common.DTO
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class PassCodeDTO
    {
        [DataMember]
        public string ResponseId { get; set; }
        [DataMember]
        public string PassCode { get; set; }
    }
}
