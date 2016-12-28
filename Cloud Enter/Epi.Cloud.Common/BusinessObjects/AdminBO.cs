using System.Runtime.Serialization;

namespace Epi.Cloud.Common.BusinessObjects
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class AdminBO
    {
        private string _AdminEmail;
        private string _OrganizationId;
        private bool _IsActive;

        [DataMember]
        public string AdminEmail
        {
            get { return _AdminEmail; }
            set { _AdminEmail = value; }
        }

        [DataMember]
        public string OrganizationId
        {
            get { return _OrganizationId; }
            set { _OrganizationId = value; }
        }

        [DataMember]
        public bool IsActive
        {
            get { return _IsActive; }
            set { _IsActive = value; }
        }


    }
}
