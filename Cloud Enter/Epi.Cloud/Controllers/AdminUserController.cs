using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Mvc;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;
using Epi.Cloud.Common.DTO;

namespace Epi.Web.MVC.Controllers
{
    public class AdminUserController : Controller
    {
        //
        // GET: /Organization/
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
           int.TryParse((Session[SessionKeys.CurrentOrgId]?? string.Empty).ToString(),out orgId);           
            UserOrgModel userOrgModel = GetUserInfoList(orgId);
            userOrgModel.UserHighestRole = int.Parse(Session[SessionKeys.UserHighestRole].ToString());
            if (Session[SessionKeys.CurrentOrgId] == null)
            {
                Session[SessionKeys.CurrentOrgId] = userOrgModel.OrgList[0].OrganizationId;
            }
            
            return View("UserList", userOrgModel);
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
                return View("UserInfo", userModel);
            }

            userModel.IsActive = true;
            return View("UserInfo", userModel);
        }

        [HttpPost]
        public ActionResult UserInfo(UserModel userModel)
        {
            UserOrgModel userOrgModel = new UserOrgModel();
            UserResponse response = new UserResponse();
            UserRequest request = new UserRequest();
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            try
            {
                if (ModelState.IsValid)
                {
                    if (userModel.IsEditMode)
                    {
                        request.Action = "Update";

                        request.User = userModel.ToUserDTO();

                         int.TryParse(Session[SessionKeys.CurrentOrgId].ToString(), out request.CurrentOrg);

                        request.CurrentUser = UserId;
                        response = _securityFacade.SetUserInfo(request);
                        userOrgModel = GetUserInfoList(request.CurrentOrg);
                        userOrgModel.Message = "User information for " + userModel.FirstName + " " + userModel.LastName + " has been updated. ";
                    }
                    else
                    {
                        request.Action = "";
                        request.User = userModel.ToUserDTO();

                        int.TryParse(Session[SessionKeys.CurrentOrgId].ToString(), out request.CurrentOrg);


                        request.CurrentUser = UserId;
                        response = _securityFacade.SetUserInfo(request);

                        if (response.Message.ToUpper() == "EXISTS" )
                        {
                            ModelState.AddModelError("Email", "Error occurred. User already exists for this organization.");
                            return View("UserInfo", userModel);
                        }

                        userOrgModel = GetUserInfoList(request.CurrentOrg);
                        userOrgModel.Message = "User " + userModel.FirstName + " " + userModel.LastName + " has been added. ";
                    }
                }
                else
                {
                    return View("UserInfo", userModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            userOrgModel.UserHighestRole = int.Parse(Session[SessionKeys.UserHighestRole].ToString());
            return View("UserList", userOrgModel);
        }

        [HttpGet]
        public ActionResult GetUserList(int orgid)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            OrganizationRequest Request = new OrganizationRequest();
            Request.Organization.OrganizationId = orgid;
            OrganizationResponse OrganizationUsers = _securityFacade.GetOrganizationUsers(Request);
            List<UserModel> UserModel = OrganizationUsers.OrganizationUsersList.ToUserModelList();
            ViewBag.SelectedOrg = orgid;
            Session[SessionKeys.CurrentOrgId] = orgid;

            return PartialView("PartialUserList", UserModel);
        }

        private UserOrgModel GetUserInfoList(int OrgId = -1)
        {
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            UserOrgModel UserOrgModel = new UserOrgModel();
            try
            {
                OrganizationRequest Request = new OrganizationRequest();
                Request.UserId = UserId;
                Request.UserRole = Convert.ToInt16(Session[SessionKeys.UserHighestRole].ToString());
                OrganizationResponse Organizations = _securityFacade.GetAdminOrganizations(Request);
                List<OrganizationModel> OrgListModel = Organizations.OrganizationList.ToOrganizationModelList();
                UserOrgModel.OrgList = OrgListModel;
                if (OrgId != -1)
                {
                    Request.Organization.OrganizationId = OrgId;
                    ViewBag.SelectedOrg = OrgId;
                }
                else
                {
                    Request.Organization.OrganizationId = Organizations.OrganizationList[0].OrganizationId;
                    ViewBag.SelectedOrg = Organizations.OrganizationList[0].OrganizationId;
                }
                OrganizationResponse OrganizationUsers = _securityFacade.GetOrganizationUsers(Request);
                List<UserModel> UserModel = OrganizationUsers.OrganizationUsersList.ToUserModelList();

                UserOrgModel.UserList = UserModel;

            }
            catch (Exception Ex)
            {
                throw Ex;

            }
            return UserOrgModel;
        }
        [HttpPost]
        public JsonResult GetUserInfoAD(string email)
        {

            UserModel User = new UserModel();
            var configuration = WebConfigurationManager.OpenWebConfiguration("/");
            var authenticationSection = (AuthenticationSection)configuration.GetSection("system.web/authentication");
            if (authenticationSection.Mode == AuthenticationMode.Windows)
            {
                var CurrentUserName = System.Web.HttpContext.Current.User.Identity.Name;
                var Domain = CurrentUserName.Split('\\')[0].ToString();
                var UserAD = Utility.WindowsAuthentication.GetUserFromAd(email,Domain );
                if (UserAD != null)
                {
                User.LastName = UserAD.Surname;
                User.FirstName = UserAD.GivenName;
                }
            }
            return Json(User);


        }
    }
}
