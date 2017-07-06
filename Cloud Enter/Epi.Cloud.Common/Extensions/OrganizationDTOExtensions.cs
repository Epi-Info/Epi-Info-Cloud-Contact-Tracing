using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Extensions
{
    public static class OrganizationDTOExtensions
    {
        public static OrganizationBO ToOrganizationBO(this OrganizationDTO pDTO)
        {
            return new OrganizationBO
            {
                IsEnabled = pDTO.IsEnabled,
                IsHostOrganization = pDTO.IsHostOrganization,
                Organization = pDTO.Organization,
                OrganizationKey = pDTO.OrganizationKey,
                OrganizationId = pDTO.OrganizationId
            };
        }
    }
}
