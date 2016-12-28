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
            int OrgId = -1;
           int.TryParse((Session[SessionKeys.CurrentOrgId]?? string.Empty).ToString(),out OrgId);           
            UserOrgModel UserOrgModel = GetUserInfoList(OrgId);
            UserOrgModel.UserHighestRole = int.Parse(Session[SessionKeys.UserHighestRole].ToString());
            if (Session[SessionKeys.CurrentOrgId] == null)
            {
                Session[SessionKeys.CurrentOrgId] = UserOrgModel.OrgList[0].OrganizationId;
            }
            
            return View("UserList", UserOrgModel);
        }

        [HttpGet]
        public ActionResult UserInfo(int userid, bool iseditmode, int orgid)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            UserModel UserModel = new UserModel();
            UserRequest Request = new UserRequest();
            int.TryParse(Session[SessionKeys.CurrentOrgId].ToString(),out orgid);
            if (iseditmode)
            {


                Request.Organization = new OrganizationDTO();
                Request.Organization.OrganizationId = orgid;

                Request.User = new UserDTO();
                Request.User.UserId = userid;

                UserResponse Response = _securityFacade.GetUserInfo(Request);
                UserModel = Response.User[0].ToUserModelR();
                UserModel.IsEditMode = true;
                return View("UserInfo", UserModel);
            }

            UserModel.IsActive = true;
            return View("UserInfo", UserModel);
        }
        [HttpPost]
        public ActionResult UserInfo(UserModel UserModel)
        {
            UserOrgModel UserOrgModel = new UserOrgModel();
            UserResponse Response = new UserResponse();
            UserRequest Request = new UserRequest();
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            try
            {
                if (ModelState.IsValid)
                {
                    if (UserModel.IsEditMode)
                    {
                        Request.Action = "UpDate";

                        Request.User = UserModel.ToUserDTO();

                         int.TryParse(Session[SessionKeys.CurrentOrgId].ToString(), out Request.CurrentOrg);

                        Request.CurrentUser = UserId;
                        Response = _securityFacade.SetUserInfo(Request);
                        UserOrgModel = GetUserInfoList(Request.CurrentOrg);
                        UserOrgModel.Message = "User information for " + UserModel.FirstName + " " + UserModel.LastName + " has been updated. ";
                    }
                    else
                    {
                        Request.Action = "";
                        Request.User = UserModel.ToUserDTO();

                        int.TryParse(Session[SessionKeys.CurrentOrgId].ToString(), out Request.CurrentOrg);


                        Request.CurrentUser = UserId;
                        Response = _securityFacade.SetUserInfo(Request);

                        if (Response.Message.ToUpper() == "EXISTS" )
                        {
                            ModelState.AddModelError("Email", "Error occurred. User already exists for this organization.");
                            return View("UserInfo", UserModel);
                        }

                        UserOrgModel = GetUserInfoList(Request.CurrentOrg);
                        UserOrgModel.Message = "User " + UserModel.FirstName + " " + UserModel.LastName + " has been added. ";
                    }
                }
                else
                {
                    return View("UserInfo", UserModel);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            UserOrgModel.UserHighestRole = int.Parse(Session[SessionKeys.UserHighestRole].ToString());
            return View("UserList", UserOrgModel);
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
