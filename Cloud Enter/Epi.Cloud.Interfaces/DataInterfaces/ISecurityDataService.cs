using Epi.Cloud.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
    public interface ISecurityDataService
	{
		UserAuthenticationResponse PassCodeLogin(UserAuthenticationRequest request);

		UserAuthenticationResponse SetPassCode(UserAuthenticationRequest request);

		UserAuthenticationResponse GetAuthenticationResponse(UserAuthenticationRequest request);


		UserAuthenticationResponse ValidateUser(UserAuthenticationRequest request);

		UserAuthenticationResponse GetUser(UserAuthenticationRequest request);

		bool UpdateUser(UserAuthenticationRequest request);

		FormSettingResponse SaveSettings(FormSettingRequest formSettingRequest);

		OrganizationResponse GetOrganizationsByUserId(OrganizationRequest orgRequest);

		OrganizationResponse GetUserOrganizations(OrganizationRequest orgRequest);

		OrganizationResponse GetAdminOrganizations(OrganizationRequest orgRequest);

		OrganizationResponse GetOrganizationInfo(OrganizationRequest orgRequest);

		OrganizationResponse SetOrganization(OrganizationRequest request);

		OrganizationResponse GetOrganizationUsers(OrganizationRequest request);

		UserResponse GetUserInfo(UserRequest request);

		UserResponse SetUserInfo(UserRequest request);
	}
}
