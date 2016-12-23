using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	public interface ISecurityDataService
	{
		UserAuthenticationResponse PassCodeLogin(UserAuthenticationRequest pRequest);

		UserAuthenticationResponse SetPassCode(UserAuthenticationRequest pRequest);

		UserAuthenticationResponse GetAuthenticationResponse(UserAuthenticationRequest pRequest);


		UserAuthenticationResponse ValidateUser(UserAuthenticationRequest pRequest);

		UserAuthenticationResponse GetUser(UserAuthenticationRequest request);

		bool UpdateUser(UserAuthenticationRequest request);

		FormSettingResponse SaveSettings(FormSettingRequest FormSettingReq);

		OrganizationResponse GetOrganizationsByUserId(OrganizationRequest OrgRequest);

		OrganizationResponse GetUserOrganizations(OrganizationRequest OrgRequest);

		OrganizationResponse GetAdminOrganizations(OrganizationRequest OrgRequest);

		OrganizationResponse GetOrganizationInfo(OrganizationRequest OrgRequest);

		OrganizationResponse SetOrganization(OrganizationRequest request);

		OrganizationResponse GetOrganizationUsers(OrganizationRequest request);

		UserResponse GetUserInfo(UserRequest request);

		UserResponse SetUserInfo(UserRequest request);
	}
}
