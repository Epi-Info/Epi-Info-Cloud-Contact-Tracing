using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.Facades.Interfaces
{
    public interface ISecurityFacade
    {
        UserAuthenticationResponse ValidateUser(string userName, string password);
        void UpdatePassCode(string responseId, string passcode);
        UserAuthenticationResponse GetAuthenticationResponse(string responseId);
        UserAuthenticationResponse GetUserInfo(int UserId);
        bool UpdateUser(UserDTO User);
		UserResponse GetUserInfo(UserRequest Request);
		UserResponse SetUserInfo(UserRequest Request);
		OrganizationResponse GetOrganizationsByUserId(OrganizationRequest organizationRequest);
		OrganizationResponse GetUserOrganizations(OrganizationRequest organizationRequest);
		OrganizationResponse GetAdminOrganizations(OrganizationRequest organizationRequest);
		OrganizationResponse GetOrganizationInfo(OrganizationRequest organizationRequest);
		OrganizationResponse SetOrganization(OrganizationRequest organizationRequest);
		OrganizationResponse GetOrganizationUsers(OrganizationRequest organizationRequest);

	}
}
