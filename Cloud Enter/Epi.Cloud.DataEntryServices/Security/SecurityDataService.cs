using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Common.Exception;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.DataEntryServices
{
    public class SecurityDataService : ISecurityDataService
    {

        // Session state variables 
        private string _accessToken;
        private string _userName;

		private readonly ISurveyInfoDao _surveyInfoDao;
		private readonly ISurveyResponseDao _surveyResponseDao;
		private readonly IFormInfoDao _formInfoDao;
		private readonly IFormSettingDao _formSettingDao;
		private readonly IUserDao _userDao;
		private readonly IOrganizationDao _organizationDao;

		public SecurityDataService(ISurveyInfoDao surveyInfoDao,
						   ISurveyResponseDao surveyResponseDao,
						   IFormInfoDao formInfoDao,
						   IFormSettingDao formSettingDao,
						   IUserDao userDao,
						   IOrganizationDao organizationDao)
		{
			_surveyInfoDao = surveyInfoDao;
			_surveyResponseDao = surveyResponseDao;
			_formInfoDao = formInfoDao;
			_formSettingDao = formSettingDao;
			_userDao = userDao;
			_organizationDao = organizationDao;
		}

		/// <summary>
		/// Validation options enum. Used in validation of messages.
		/// </summary>
		[Flags]
		private enum Validate
		{
			ClientTag = 0x0001,
			AccessToken = 0x0002,
			UserCredentials = 0x0004,
			All = ClientTag | AccessToken | UserCredentials
		}

		public UserAuthenticationResponse PassCodeLogin(UserAuthenticationRequest request)
		{
			var response = new UserAuthenticationResponse();

			SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

			UserAuthenticationRequestBO passCodeBO = request.ToPassCodeBO();
			bool result = surveyResponseImplementation.ValidateUser(passCodeBO);

			if (result)
			{
				response.Message = "Invalid Pass Code.";
				response.UserIsValid = true;
			}
			else
			{
				response.UserIsValid = false;
			}

			return response;
		}

		public UserAuthenticationResponse ValidateUser(UserAuthenticationRequest userAuthenticationRequest)
		{
			var response = new UserAuthenticationResponse();

			Epi.Web.BLL.User userImplementation = new Epi.Web.BLL.User(_userDao);

			UserBO UserBO = userAuthenticationRequest.User.ToUserBO();

			UserBO result = userImplementation.GetUser(UserBO);

			if (result != null)
			{
				response.User = result.ToUserDTO();
				response.UserIsValid = true;
			}
			else
			{
				response.UserIsValid = false;
			}

			return response;
		}


		public bool UpdateUser(UserAuthenticationRequest request)
		{
			Epi.Web.BLL.User userImplementation = new Epi.Web.BLL.User(_userDao);

			UserBO UserBO = request.User.ToUserBO();
			OrganizationBO OrgBO = new OrganizationBO();
			return userImplementation.UpdateUser(UserBO, OrgBO);
		}

		public UserAuthenticationResponse GetAuthenticationResponse(UserAuthenticationRequest request)
		{
			UserAuthenticationResponse response = new UserAuthenticationResponse();

			SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

			Epi.Cloud.Common.BusinessObjects.UserAuthenticationRequestBO PassCodeBO = request.ToPassCodeBO();

            var userAuthenticationResponseBO = surveyResponseImplementation.GetAuthenticationResponse(PassCodeBO);
            response = userAuthenticationResponseBO.ToUserAuthenticationResponse();

			return response;
		}

		public UserAuthenticationResponse SetPassCode(UserAuthenticationRequest request)
		{
			var response = new UserAuthenticationResponse();

			SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

			Epi.Cloud.Common.BusinessObjects.UserAuthenticationRequestBO PassCodeBO = request.ToPassCodeBO();
			surveyResponseImplementation.SavePassCode(PassCodeBO);

			return response;
		}

		/// <summary>
		/// Validate 3 security levels for a request: ClientTag, AccessToken, and User Credentials
		/// </summary>
		/// <param name="request">The request message.</param>
		/// <param name="response">The response message.</param>
		/// <param name="validate">The validation that needs to take place.</param>
		/// <returns></returns>
		private bool ValidRequest(RequestBase request, ResponseBase response, Validate validate)
		{
			bool result = true;

			// Validate user credentials
			if ((Validate.UserCredentials & validate) == Validate.UserCredentials)
			{
				if (_userName == null)
				{
					response.Message = "Please login and provide user credentials before accessing these methods.";
					//return false;
				}
			}


			return result;
		}



		public UserAuthenticationResponse GetUser(UserAuthenticationRequest request)
        {
            var response = new UserAuthenticationResponse();
            Epi.Web.BLL.User userImplementation = new Epi.Web.BLL.User(_userDao);

            UserBO UserBO = request.User.ToUserBO();

            UserBO result = userImplementation.GetUserByUserId(UserBO);

            if (result != null)
            {
                response.User = result.ToUserDTO();
            }

            return response;
        }
        public FormSettingResponse SaveSettings(FormSettingRequest FormSettingReq)
        {
            FormSettingResponse Response = new FormSettingResponse();
            try
            {
                Epi.Web.BLL.FormSetting formSettingsImplementation = new Epi.Web.BLL.FormSetting(_formSettingDao, _userDao);
                if (FormSettingReq.FormSetting.Count() > 0)
                {
                    foreach (var item in FormSettingReq.FormSetting)
                    {
                        formSettingsImplementation.UpdateFormSettings(FormSettingReq.FormInfo.IsDraftMode, item);

                    }
                    string Message = formSettingsImplementation.SaveSettings(FormSettingReq.FormInfo.IsDraftMode, FormSettingReq.FormSetting[0]);
                }

                return Response;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public OrganizationResponse GetOrganizationsByUserId(OrganizationRequest request)
        {
            try
            {
                Epi.Web.BLL.Organization organizationImplementation = new Epi.Web.BLL.Organization(_organizationDao);

                var response = new OrganizationResponse();

                if (!ValidRequest(request, response, Validate.All))
                    return response;

                List<OrganizationBO> organizationBOList = organizationImplementation.GetOrganizationsByUserId(request.UserId);
                response.OrganizationList = organizationBOList.ToOrganizationDTOList();
                return response;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public OrganizationResponse GetUserOrganizations(OrganizationRequest request)
        {
            try
            {
                Epi.Web.BLL.Organization organizationImplementation = new Epi.Web.BLL.Organization(_organizationDao);
                var response = new OrganizationResponse();

                if (!ValidRequest(request, response, Validate.All))
                    return response;

                List<OrganizationBO> listOrganizationBO = organizationImplementation.GetOrganizationInfoByUserId(request.UserId, request.UserRole);
                response.OrganizationList = new List<OrganizationDTO>();
                foreach (OrganizationBO Item in listOrganizationBO)
                {
                    (response.OrganizationList).Add(Item.ToOrganizationDTO());
                }
                return response;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public OrganizationResponse GetAdminOrganizations(OrganizationRequest request)
        {
            try
            {
                Epi.Web.BLL.Organization organizationImplementation = new Epi.Web.BLL.Organization(_organizationDao);
                
                var response = new OrganizationResponse();


                if (!ValidRequest(request, response, Validate.All))
                    return response;

                List<OrganizationBO> organizationBOList = organizationImplementation.GetOrganizationInfoForAdmin(request.UserId, request.UserRole);
                response.OrganizationList = organizationBOList.ToOrganizationDTOList();
                return response;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public OrganizationResponse GetOrganizationInfo(OrganizationRequest request)
        {
            try
            {
                Epi.Web.BLL.Organization organizationImplementation = new Epi.Web.BLL.Organization(_organizationDao);
                
                OrganizationResponse response = new OrganizationResponse();

                if (!ValidRequest(request, response, Validate.All))
                    return response;
                OrganizationBO organizationBO = organizationImplementation.GetOrganizationByKey(request.Organization.OrganizationKey);

                response.OrganizationList = new List<OrganizationDTO>();

                response.OrganizationList.Add(organizationBO.ToOrganizationDTO());

                return response;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public OrganizationResponse SetOrganization(OrganizationRequest request)
        {
            try
            {
                Epi.Web.BLL.Organization organizationImplementation = new Epi.Web.BLL.Organization(_organizationDao);

                // Transform SurveyInfo data transfer object to SurveyInfo business object
                var organization = request.Organization.ToOrganizationBO();
                var user = request.OrganizationAdminInfo.ToUserBO();
                var response = new OrganizationResponse();

                if (request.Action.ToUpper() == "UPDATE")
                {
                    if (!ValidRequest(request, response, Validate.All))
                    {
                        response.Message = "Error";
                        return response;
                    }

                    if (organizationImplementation.OrganizationNameExists(organization.Organization, organization.OrganizationKey, "Update"))
                    {
                        response.Message = "Exists";
                    }
                    else
                    {
                        var success = organizationImplementation.UpdateOrganizationInfo(organization);
                        if (success)
                        {
                            response.Message = "Successfully added organization Key";
                        }
                        else
                        {
                            response.Message = "Error";
                            return response;
                        }
                    }
                }
                else if (request.Action.ToUpper() == "INSERT")
                {
                    Guid organizationKey = Guid.NewGuid();
                    organization.OrganizationKey = organizationKey.ToString();
                    if (!ValidRequest(request, response, Validate.All))
                        return response;
                    if (organizationImplementation.OrganizationNameExists(organization.Organization, organization.OrganizationKey, "Create"))
                    {
                        response.Message = "Exists";
                    }
                    else
                    {
                        organizationImplementation.InsertOrganizationInfo(organization, user);

                        response.Message = "Success";
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public OrganizationResponse GetOrganizationUsers(OrganizationRequest request)
        {
            try
            {
                Epi.Web.BLL.User userImplementation = new Epi.Web.BLL.User(_userDao);
                // Transform SurveyInfo data transfer object to SurveyInfo business object
                OrganizationBO organization = request.Organization.ToOrganizationBO();
                var response = new OrganizationResponse();

                if (!ValidRequest(request, response, Validate.All))
                    return response;

                List<UserBO> userBOList = userImplementation.GetUsersByOrgId(request.Organization.OrganizationId);
                response.OrganizationUsersList = userBOList.ToUserDTOList();
                return response;
            }
            catch (Exception ex)
            {

                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public UserResponse GetUserInfo(UserRequest request)
        {

            UserResponse response = new UserResponse();

			Epi.Web.BLL.User userImplementation = new Epi.Web.BLL.User(_userDao);

            UserBO userBO = request.User.ToUserBO();
            OrganizationBO orgBO = request.Organization.ToOrganizationBO();
            UserBO result = new UserBO();
            if (!request.IsAuthenticated)
            {
                result = userImplementation.GetUserByUserIdAndOrgId(userBO, orgBO);
            }
            else
            {
                result = userImplementation.GetUserByEmail(userBO);
            }

            if (result != null)
            {
                response.User = new List<UserDTO>();
                response.User.Add(result.ToUserDTO());
            }

            return response;
        }


        public UserResponse SetUserInfo(UserRequest request)
        {
            try
            {
                UserResponse response = new UserResponse();

				Epi.Web.BLL.Organization organizationImplementation = new Epi.Web.BLL.Organization(_organizationDao);

                Epi.Web.BLL.User userImplementation = new Epi.Web.BLL.User(_userDao);
                UserBO userBO = request.User.ToUserBO();
                OrganizationBO orgBo = request.Organization.ToOrganizationBO();
                if (request.Action.ToUpper() == "UPDATE")
                {
                    OrganizationBO organizationBO = organizationImplementation.GetOrganizationByOrgId(request.CurrentOrg);
                    userBO.Operation = OperationMode.UpdateUserInfo;
                    userImplementation.UpdateUser(userBO, organizationBO);
                }
                else
                {
                    UserBO existingUser = userImplementation.GetUserByEmail(userBO);//Validate if user is in the system. 
                    bool UserExists = false;
                    if (existingUser != null)
                    {
                        OrganizationBO OrganizationBO = organizationImplementation.GetOrganizationByOrgId(request.CurrentOrg);
                        existingUser.Role = userBO.Role;
                        existingUser.IsActive = userBO.IsActive;
                        userBO = existingUser;
                        UserExists = userImplementation.IsUserExistsInOrganizaion(userBO, OrganizationBO); //validate if user is part of the organization already. 
                    }

                    if (!UserExists)
                    {
                        //OrgBo.OrganizationId = request.CurrentOrg; // User is added to the current organization
                        OrganizationBO organizationBO = organizationImplementation.GetOrganizationByOrgId(request.CurrentOrg);
                        userImplementation.SetUserInfo(userBO, organizationBO);
                        response.Message = "Success";
                    }
                    else
                    {
                        response.Message = "Exists";
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }
	}
}
