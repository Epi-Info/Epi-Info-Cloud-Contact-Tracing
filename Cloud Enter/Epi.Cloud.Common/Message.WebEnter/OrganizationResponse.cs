using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    public class OrganizationResponse : ResponseBase
    {
        /// <summary>
        /// Default Constructor for OrganizationResponse.
        /// </summary>
        public OrganizationResponse()
        {
            OrganizationList = new List<OrganizationDTO>();
            OrganizationUsersList = new List<UserDTO>();
        }

        /// <summary>
        /// OrganizationInfo object.
        /// </summary>
        public List<OrganizationDTO> OrganizationList;

        public List<UserDTO> OrganizationUsersList;
    }
}
