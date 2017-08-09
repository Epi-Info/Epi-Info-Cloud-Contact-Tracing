using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Cloud.MVC.Utility;

namespace Epi.Cloud.Facades
{
    public class SecurityFacade : ISecurityFacade
    {
        private readonly ISecurityDataService _securityDataService;
		private readonly IDataEntryService _dataEntryService;

		public SecurityFacade(ISecurityDataService securityDataService,
						      IDataEntryService dataEntryService)
        {
			_securityDataService = securityDataService;
			_dataEntryService = dataEntryService;
        }

        public UserAuthenticationResponse ValidateUser(string userName, string password)
        {
            UserDTO User = new UserDTO { UserName = userName, PasswordHash = password };
			var surveyAuthenticationRequest = new UserAuthenticationRequest { User = User };

            UserAuthenticationResponse authenticationResponse = _securityDataService.ValidateUser(surveyAuthenticationRequest);

			return authenticationResponse;
        }
        public void UpdatePassCode(string responseId, string passcode)
        {
			// convert DTO to  UserAuthenticationRquest
			var passCodeDTO = new PassCodeDTO { ResponseId = responseId, PassCode = passcode };
            UserAuthenticationRequest authenticationRequest = passCodeDTO.ToUserAuthenticationObj();
            SurveyHelper.UpdatePassCode(authenticationRequest, _securityDataService);
        }
        public UserAuthenticationResponse GetAuthenticationResponse(string responseId)
        {
			var surveyAuthenticationRequest = new UserAuthenticationRequestBO { ResponseId = responseId };
			var authenticationResponseBO = _dataEntryService.GetAuthenticationResponse(surveyAuthenticationRequest);
			var authenticationResponse = new UserAuthenticationResponse { PassCode = authenticationResponseBO.PassCode };
			return authenticationResponse;
        }
        public UserAuthenticationResponse GetUserInfo(int UserId)
        {
			UserDTO User = new UserDTO();
            User.UserId = UserId;
            var userAuthenticationRequest = new UserAuthenticationRequest { User = User };

            UserAuthenticationResponse AuthenticationResponse = _securityDataService.GetAuthenticationResponse(userAuthenticationRequest);
            return AuthenticationResponse;

        }

        public bool UpdateUser(UserDTO User)
        {
            UserAuthenticationRequest request = new UserAuthenticationRequest();
            request.User = User;
            return _securityDataService.UpdateUser(request);
        }

		public UserResponse GetUserInfo(UserRequest Request)
		{
			UserResponse UserResponse = new UserResponse();
			UserResponse = _securityDataService.GetUserInfo(Request);
			return UserResponse;
		}

		public UserResponse SetUserInfo(UserRequest Request)
		{
			UserResponse UserResponse = new UserResponse();
			UserResponse = _securityDataService.SetUserInfo(Request);
			return UserResponse;
		}

		public OrganizationResponse GetOrganizationsByUserId(OrganizationRequest organizationRequest)
		{
			OrganizationResponse organizationResponse = new OrganizationResponse();
			organizationResponse = _securityDataService.GetOrganizationsByUserId(organizationRequest);
			return organizationResponse;
		}

		public OrganizationResponse GetUserOrganizations(OrganizationRequest organizationRequest)
		{
			OrganizationResponse organizationResponse = new OrganizationResponse();
			organizationResponse = _securityDataService.GetUserOrganizations(organizationRequest);
			return organizationResponse;
		}

		public OrganizationResponse GetAdminOrganizations(OrganizationRequest organizationRequest)
		{
			OrganizationResponse organizationResponse = new OrganizationResponse();
			organizationResponse = _securityDataService.GetAdminOrganizations(organizationRequest);
			return organizationResponse;
		}

		public OrganizationResponse GetOrganizationInfo(OrganizationRequest organizationRequest)
		{
			OrganizationResponse organizationResponse = new OrganizationResponse();
			organizationResponse = _securityDataService.GetOrganizationInfo(organizationRequest);
			return organizationResponse;
		}

		public OrganizationResponse SetOrganization(OrganizationRequest organizationRequest)
		{
			OrganizationResponse organizationResponse = new OrganizationResponse();
			organizationResponse = _securityDataService.SetOrganization(organizationRequest);
			return organizationResponse;
		}

		public OrganizationResponse GetOrganizationUsers(OrganizationRequest organizationRequest)
		{
			OrganizationResponse organizationResponse = new OrganizationResponse();
			organizationResponse = _securityDataService.GetOrganizationUsers(organizationRequest);
			return organizationResponse;
		}
	}
}