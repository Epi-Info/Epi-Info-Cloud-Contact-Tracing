using System;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    public class OrganizationRequest : RequestBase
    {
        public OrganizationRequest()
        {
            this.Organization = new OrganizationDTO();
        }

        /// <summary>
        /// Organization object.
        /// </summary>
        public OrganizationDTO Organization;

        public Guid AdminSecurityKey;

        public int UserId;

        public int UserRole;

        public UserDTO OrganizationAdminInfo;
    }
}
