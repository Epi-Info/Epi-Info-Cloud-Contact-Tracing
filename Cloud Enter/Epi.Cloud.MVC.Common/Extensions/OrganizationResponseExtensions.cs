using Epi.Cloud.Common.Message;
using Epi.Cloud.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    public static class OrganizationResponseExtensions
    {
        public static OrgAdminInfoModel ToOrgAdminInfoModel(this OrganizationResponse Organizations)
        {
            OrgAdminInfoModel OrgAdminInfoModel = new OrgAdminInfoModel();
            OrgAdminInfoModel.OrgName = Organizations.OrganizationList[0].Organization;
            OrgAdminInfoModel.IsOrgEnabled = Organizations.OrganizationList[0].IsEnabled;
            OrgAdminInfoModel.IsHostOrganization = Organizations.OrganizationList[0].IsHostOrganization;

            return OrgAdminInfoModel;
        }
    }
}
