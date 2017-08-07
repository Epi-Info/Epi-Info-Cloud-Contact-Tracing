using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Mvc;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.MVC.Constants;
using Epi.Cloud.MVC.Extensions;
using Epi.Cloud.MVC.Models;
using Epi.Cloud.MVC.Utility;

namespace Epi.Cloud.MVC.Controllers
{
    public class AdminOrganizationController : BaseSurveyController
    {
        private ISurveyFacade _surveyFacade;
        private ISecurityFacade _securityFacade;

        public AdminOrganizationController(ISurveyFacade isurveyFacade,
                                           ISecurityFacade isecurityFacade)
        {
            _surveyFacade = isurveyFacade;
            _securityFacade = isecurityFacade;
        }

        [HttpGet]
        public ActionResult OrgList()
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            int userHighestRole = GetIntSessionValue(UserSession.Key.UserHighestRole);
            OrganizationRequest request = new OrganizationRequest();
            request.UserId = userId;
            request.UserRole = userHighestRole;
            OrganizationResponse organizations = _securityFacade.GetUserOrganizations(request);

            List<OrganizationModel> model = organizations.OrganizationList.ToOrganizationModelList();
            OrgListModel orgListModel = new OrgListModel();
            orgListModel.OrganizationList = model;

            if (userHighestRole == 3)
            {
                return View(ViewActions.OrgList, orgListModel);
            }
            else
            {
                return RedirectToAction(ViewActions.UserList, ControllerNames.AdminUser);
            }
        }

        [HttpGet]
        public ActionResult OrgInfo(string orgKey, bool isEditMode)
        {
            OrgAdminInfoModel orgInfo = new OrgAdminInfoModel();
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            if (isEditMode)
            {
                OrganizationRequest request = new OrganizationRequest();
                request.Organization.OrganizationKey = orgKey;

                OrganizationResponse organizations = _securityFacade.GetOrganizationInfo(request);
                orgInfo = organizations.ToOrgAdminInfoModel();
                orgInfo.IsEditMode = isEditMode;
                return View(ViewActions.OrgInfo, orgInfo);
            }

            orgInfo.IsEditMode = isEditMode;
            orgInfo.IsOrgEnabled = true;
            return View(ViewActions.OrgInfo, orgInfo);
        }

        [HttpPost]
        public ActionResult OrgInfo(OrgAdminInfoModel orgAdminInfoModel)
        {
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            int userHighestRole = GetIntSessionValue(UserSession.Key.UserHighestRole);
            string url = "";
            if (this.Request.UrlReferrer == null)
            {
                url = this.Request.Url.ToString();
            }
            else
            {
                url = this.Request.UrlReferrer.ToString();
            }
            //Edit Organization
            if (orgAdminInfoModel.IsEditMode)
            {
                ModelState.Remove("AdminFirstName");
                ModelState.Remove("AdminLastName");
                ModelState.Remove("ConfirmAdminEmail");
                ModelState.Remove("AdminEmail");

                OrganizationRequest request = new OrganizationRequest();

				UserDTO adminInfo = new UserDTO();

                adminInfo.FirstName = "";
                adminInfo.LastName = "";
                adminInfo.EmailAddress = "";
                adminInfo.Role = 0;
                adminInfo.PhoneNumber = "";
                request.OrganizationAdminInfo = adminInfo;

                request.Organization.Organization = orgAdminInfoModel.OrgName;
                request.Organization.IsEnabled = orgAdminInfoModel.IsOrgEnabled;
                request.Organization.IsHostOrganization = orgAdminInfoModel.IsHostOrganization;

                request.Organization.OrganizationKey = GetOrgKey(url);
                request.UserId = userId;
                request.UserRole = userHighestRole;
                request.Action = RequestAction.Update;
                try
                {
                    OrganizationResponse result = _securityFacade.SetOrganization(request);
                    OrganizationResponse organizations = _securityFacade.GetUserOrganizations(request);
                    List<OrganizationModel> model = organizations.OrganizationList.ToOrganizationModelList();
                    OrgListModel orgListModel = new OrgListModel();
                    orgListModel.OrganizationList = model;
                    orgListModel.Message = "Organization " + orgAdminInfoModel.OrgName + " has been updated.";
                    if (result.Message.ToUpper() != "EXISTS" && result.Message.ToUpper() != "ERROR")
                    {

                        orgListModel.Message = "Organization " + orgAdminInfoModel.OrgName + " has been updated.";
                        return View(ViewActions.OrgList, orgListModel);
                    }
                    else if (result.Message.ToUpper() == "ERROR")
                    {
                        OrgAdminInfoModel orgInfo = new OrgAdminInfoModel();
                        request.Organization.OrganizationKey = GetOrgKey(url); ;

                        organizations = _securityFacade.GetOrganizationInfo(request);
                        orgInfo = organizations.ToOrgAdminInfoModel();
                        orgInfo.IsEditMode = true;
                        ModelState.AddModelError("IsOrgEnabled", "Organization for the super admin cannot be deactivated.");
                        return View(ViewActions.OrgInfo, orgInfo);
                    }
                    else
                    {
                        OrgAdminInfoModel orgInfo = new OrgAdminInfoModel();
                        request.Organization.OrganizationKey = GetOrgKey(url); ;

                        organizations = _securityFacade.GetOrganizationInfo(request);
                        orgInfo = organizations.ToOrgAdminInfoModel();
                        orgInfo.IsEditMode = true;
                        ModelState.AddModelError("OrgName", "The organization name provided already exists.");
                        return View(ViewActions.OrgInfo, orgInfo);
                    }
                }
                catch (Exception ex)
                {
                    return View(orgAdminInfoModel);
                }
            }
            else
            {
                // Add new Organization 

                if (ModelState.IsValid)
                {
                    OrganizationRequest request = new OrganizationRequest();
                    request.Organization.Organization = orgAdminInfoModel.OrgName;
                    request.Organization.IsEnabled = orgAdminInfoModel.IsOrgEnabled;
                    request.Organization.IsHostOrganization = orgAdminInfoModel.IsHostOrganization;
                    UserDTO adminInfo = new UserDTO();

                    adminInfo.FirstName = orgAdminInfoModel.AdminFirstName;
                    adminInfo.LastName = orgAdminInfoModel.AdminLastName;
                    adminInfo.EmailAddress = orgAdminInfoModel.AdminEmail;
                    adminInfo.Role = Roles.Administrator;
                    adminInfo.PhoneNumber = "123456789";
                    adminInfo.UGuid = Guid.NewGuid();
                    request.OrganizationAdminInfo = adminInfo;

                    request.UserRole = userHighestRole;
                    request.UserId = userId;
                    request.Action = RequestAction.Insert;
                    try
                    {
                        OrganizationResponse result = _securityFacade.SetOrganization(request);
                        OrgListModel orgListModel = new OrgListModel();
                        OrganizationResponse organizations = _securityFacade.GetUserOrganizations(request);
                        List<OrganizationModel> model = organizations.OrganizationList.ToOrganizationModelList();
                        orgListModel.OrganizationList = model;

                        if (result.Message.ToUpper() != "EXISTS")
                        {

                            orgListModel.Message = "Organization " + orgAdminInfoModel.OrgName + " has been created.";
                        }
                        else
                        {
                            // OrgListModel.Message = "The organization name provided already exists.";
                            OrgAdminInfoModel orgInfo = new OrgAdminInfoModel();
                            //Request.Organization.OrganizationKey = GetOrgKey(url); ;

                            //Organizations = _isurveyFacade.GetOrganizationInfo(Request);
                            orgInfo = organizations.ToOrgAdminInfoModel();
                            orgInfo.IsEditMode = false;
                            ModelState.AddModelError("OrgName", "The organization name provided already exists.");
                            return View(ViewActions.OrgInfo, orgInfo);
                        }
                        return View(ViewActions.OrgList, orgListModel);
                    }
                    catch (Exception ex)
                    {
                        return View(orgAdminInfoModel);
                    }
                }
                else
                {
                    return View(orgAdminInfoModel);
                }
            }
        }

        private string GetOrgKey(string url)
        {
            var array = url.Split('/');
            string orgkey = "";
            Regex guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");

            foreach (var item in array)
            {

                if (guidRegEx.IsMatch(item))
                {
                    orgkey = item;
                    break;
                }

            }
            return orgkey;
        }

        [HttpPost]
        public ActionResult AutoComplete(string term)
        {
            var result = new[] { "App", "Bbc", "Cool", "Div", "Enter", "False" };
            return Json(result, JsonRequestBehavior.AllowGet);
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
                var userAD = Utility.WindowsAuthentication.GetUserFromAd(email, domain);
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
