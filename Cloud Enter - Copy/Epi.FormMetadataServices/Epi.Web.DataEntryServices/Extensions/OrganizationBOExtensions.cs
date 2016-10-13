using Epi.Web.EF;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class OrganizationBOExtensions
    {
        public static Organization ToOrganization(OrganizationBO organizationBO)
        {
            return new Organization
            {
                Organization1 = organizationBO.Organization,
                IsEnabled = organizationBO.IsEnabled,
                OrganizationKey = organizationBO.OrganizationKey
            };
        }
    }
}
