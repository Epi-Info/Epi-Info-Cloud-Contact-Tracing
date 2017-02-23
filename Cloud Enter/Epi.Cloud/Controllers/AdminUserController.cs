using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Mvc;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;

namespace Epi.Web.MVC.Controllers
{
    public class AdminUserController : Controller
    {
        private readonly ISurveyFacade _surveyFacade;
        private readonly ISecurityFacade _securityFacade;

        public AdminUserController(ISurveyFacade surveyFacade,
                                   ISecurityFacade securityFacade)
        {
            _surveyFacade = surveyFacade;
            _securityFacade = securityFacade;
        }

        [HttpGet]
        public ActionResult UserList()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            int orgId = -1;
            int.TryParse((Session[SessionKeys.CurrentOrgId] ?? string.Empty).ToString(),out orgId);           
            UserOrgModel userOrgModel = GetUserInfoList(orgId);
            userOrgModel.UserHighestRole = int.Parse(Session[SessionKeys.UserHighestRole].ToString());
            if (Session[SessionKeys.CurrentOrgId] == null)
            {
                Session[SessionKeys.CurrentOrgId] = userOrgModel.OrgList[0].OrganizationId;
            }
            
            return View(ViewActions.UserList, userOrgModel);
        }

        [HttpGet]
        public ActionResult UserInfo(int userId, bool isEditMode, int orgId)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            UserModel userModel = new UserModel();
            UserRequest request = new UserRequest();
            int.TryParse(Session[SessionKeys.CurrentOrgId].ToString(), out orgId);
            if (isEditMode)
            {
                request.Organization = new OrganizationDTO();
                request.Organization.OrganizationId = orgId;

                request.User = new UserDTO();
                request.User.UserId = userId;

                UserResponse response = _securityFacade.GetUserInfo(request);
                userModel = response.User[0].ToUserModelR();
                userModel.IsEditMode = true;
                return View(ViewActions.UserInfo, userModel);
            }

            userModel.IsActive = true;
            return View(ViewActions.UserInfo, userModel);
        }

        [HttpPost]
        public ActionResult UserInfo(UserModel userModel)
        {
            UserOrgModel userOrgModel = new UserOrgModel();
            UserResponse response = new UserResponse();
            UserRequest request = new UserRequest();
            string messageTemplate;
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            try
            {
                if (ModelState.IsValid)
                {
                    if (userModel.IsEditMode)
                    {
                        request.Action = RequestAction.Update;

                        request.User = userModel.ToUserDTO();

                         int.TryParse(Session[SessionKeys.CurrentOrgId].ToString(), out request.CurrentOrg);

                        request.CurrentUser = userId;
                        response = _securityFacade.SetUserInfo(request);
                        userOrgModel = GetUserInfoList(request.CurrentOrg);

                        // "User information for {0} {1} has been updated."
                        messageTemplate = ResourceProvider.GetResourceString(ResourceNamespaces.UserAdminMessages, UserAdminResourceKeys.UserInformationUpdated);
                        userOrgModel.Message = string.Format(messageTemplate, userModel.FirstName, userModel.LastName);
                    }
                    else
                    {
                        request.Action = RequestAction.None;
                        request.User = userModel.ToUserDTO();

                        int.TryParse(Session[SessionKeys.CurrentOrgId].ToString(), out request.CurrentOrg);

                        request.CurrentUser = userId;
                        response = _securityFacade.SetUserInfo(request);

                        if (response.Message.ToUpper() == "EXISTS" )
                        {
                            // "Error occurred. User already exists for this organization."
                            messageTemplate = ResourceProvider.GetResourceString(ResourceNamespaces.UserAdminMessages, UserAdminResourceKeys.UserAlreadyExists);
                            ModelState.AddModelError("Email", messageTemplate);
                            return View(ViewActions.UserInfo, userModel);
                        }

                        userOrgModel = GetUserInfoList(request.CurrentOrg);

                        // "User {0} {1} has been added."
                        messageTemplate = ResourceProvider.GetResourceString(ResourceNamespaces.UserAdminMessages, UserAdminResourceKeys.UserAdded);
                        userOrgModel.Message = string.Format(messageTemplate, userModel.FirstName, userModel.LastName);
                    }
                }
                else
                {
                    return View(ViewActions.UserInfo, userModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            userOrgModel.UserHighestRole = int.Parse(Session[SessionKeys.UserHighestRole].ToString());
            return View(ViewActions.UserList, userOrgModel);
        }

        [HttpGet]
        public ActionResult GetUserList(int orgid)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            OrganizationRequest request = new OrganizationRequest();
            request.Organization.OrganizationId = orgid;
            OrganizationResponse organizationUsers = _securityFacade.GetOrganizationUsers(request);
            List<UserModel> userModel = organizationUsers.OrganizationUsersList.ToUserModelList();
            ViewBag.SelectedOrg = orgid;
            Session[SessionKeys.CurrentOrgId] = orgid;

            return PartialView("PartialUserList", userModel);
        }

        private UserOrgModel GetUserInfoList(int orgId = -1)
        {
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            UserOrgModel userOrgModel = new UserOrgModel();
            try
            {
                OrganizationRequest request = new OrganizationRequest();
                request.UserId = userId;
                request.UserRole = Convert.ToInt16(Session[SessionKeys.UserHighestRole].ToString());
                OrganizationResponse organizations = _securityFacade.GetAdminOrganizations(request);
                List<OrganizationModel> orgListModel = organizations.OrganizationList.ToOrganizationModelList();
                userOrgModel.OrgList = orgListModel;
                if (orgId != -1)
                {
                    request.Organization.OrganizationId = orgId;
                    ViewBag.SelectedOrg = orgId;
                }
                else
                {
                    request.Organization.OrganizationId = organizations.OrganizationList[0].OrganizationId;
                    ViewBag.SelectedOrg = organizations.OrganizationList[0].OrganizationId;
                }
                OrganizationResponse organizationUsers = _securityFacade.GetOrganizationUsers(request);
                List<UserModel> userModel = organizationUsers.OrganizationUsersList.ToUserModelList();

                userOrgModel.UserList = userModel;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userOrgModel;
        }

        [HttpPost]
        public JsonResult GetUserInfoAD(string email)
        {
            UserModel user = new UserModel();
            var configuration = WebConfigurationManager.OpenWebConfiguration("/");
            var authenticationSection = (AuthenticationSection)configuration.GetSection("system.web/authentication");
            if (authenticationSection.Mode == AuthenticationMode.Windows)
            {
                var currentUserName = System.Web.HttpContext.Current.User.Identity.Name;
                var domain = currentUserName.Split('\\')[0].ToString();
                var userAD = Utility.WindowsAuthentication.GetUserFromAd(email,domain );
                if (userAD != null)
                {
                    user.LastName = userAD.Surname;
                    user.FirstName = userAD.GivenName;
                }
            }
            return Json(user);
        }
    }
}
