using System;
using System.Runtime.Serialization;

namespace Epi.Cloud.Common.Criteria
{
    public class FormInfoCriteria : Criteria
    {

        [DataMember]
        public Guid OrganizationKey { get; set; }

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public DateTime DateCreatedMin { get; set; }
        [DataMember]
        public DateTime DateCreatedMax { get; set; }

        [DataMember]
        public string FormName { get; set; }

        [DataMember]
        public int CurrentOrgId { get; set; }
    }
}
