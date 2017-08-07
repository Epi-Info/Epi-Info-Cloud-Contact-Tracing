using System.Collections.Generic;

namespace Epi.Cloud.MVC.Models
{
    public class UserOrgModel
    {
        public List<UserModel> UserList { get; set; }
        public List<OrganizationModel> OrgList { get; set; }
        public string Message { get; set; }
        public int UserHighestRole { get; set; }

    }
}