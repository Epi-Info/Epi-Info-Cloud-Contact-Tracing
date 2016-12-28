using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Message
{
    public class UserRequest
    {
        public UserRequest()
        {
            this.Organization = new OrganizationDTO();
            this.User = new UserDTO();
        }
        public UserDTO User;

        public OrganizationDTO Organization;

        public string Action;

        public int CurrentUser;

        public int CurrentOrg;

        public bool IsAuthenticated;
    }
}
