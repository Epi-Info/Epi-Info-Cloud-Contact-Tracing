using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Epi.Web.Enter.Common.BusinessObject
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class OrganizationBO
    {
        private string _Organization;
        private string _OrganizationKey;
        private bool _IsEnabled;
        private int _OrganizationId;
        private List<UserBO> _OrganizationUserList;
        [DataMember]
        public string Organization { get; set; }

        [DataMember]
        public string OrganizationKey { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool IsHostOrganization { get; set; }

        [DataMember]
        public int OrganizationId { get; set; }
    }
}
