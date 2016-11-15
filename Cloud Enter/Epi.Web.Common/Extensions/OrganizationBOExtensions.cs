using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Web.Enter.Common.Extensions
{
    public static class OrganizationBOExtensions
    {
        public static OrganizationDTO ToOrganizationDTO(this OrganizationBO organizationBO)
        {
            return new OrganizationDTO
            {
                //  AdminId = pBO.AdminId,
                IsEnabled = organizationBO.IsEnabled,
                Organization = organizationBO.Organization,
                OrganizationKey = Epi.Web.Enter.Common.Security.Cryptography.Decrypt(organizationBO.OrganizationKey),
                OrganizationId = organizationBO.OrganizationId
            };

        }
    }
}
