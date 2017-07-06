using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Extensions
{
    public static class OrganizationBOExtensions
    {
        public static OrganizationDTO ToOrganizationDTO(this OrganizationBO organizationBO)
        {
            return new OrganizationDTO
            {
                IsEnabled = organizationBO.IsEnabled,
                IsHostOrganization = organizationBO.IsHostOrganization,
                Organization = organizationBO.Organization,
                OrganizationKey = Epi.Common.Security.Cryptography.Decrypt(organizationBO.OrganizationKey),
                OrganizationId = organizationBO.OrganizationId
            };
        }

        public static List<OrganizationDTO> ToOrganizationDTOList(this List<OrganizationBO> organizationBOList)
        {
            return organizationBOList.Select(o => o.ToOrganizationDTO()).ToList();
        }
    }
}
