using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Web.Enter.Common.Extensions
{
    public static class OrganizationDTOExtensions
    {
        public static OrganizationBO ToOrganizationBO(this OrganizationDTO pDTO)
        {
            return new OrganizationBO
            {
                IsEnabled = pDTO.IsEnabled,
                Organization = pDTO.Organization,
                OrganizationKey = pDTO.OrganizationKey,
                OrganizationId = pDTO.OrganizationId
            };
        }
    }
}
