using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Reflection;
using Epi.Common.Diagnostics;
using Epi.Cloud.Common.Message;
using Epi.Web.MVC.Models;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Common.DTO;

namespace Epi.Web.MVC.Controllers
{

    public class LoginController : Controller
    {
        private readonly ILogger _logger;

        private ISecurityFacade _securityFacade;
        private ISurveyFacade _surveyFacade;

		/// <summary>
		/// Inject ILogger, IsurveyFacade, ISecurityFacade
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="surveyFacade"></param>
		/// <param name="securityFacade"></param>
        public LoginController(ILogger logger, ISurveyFacade surveyFacade, ISecurityFacade securityFacade)
        {
            _logger = logger;
            _surveyFacade = surveyFacade;
            _securityFacade = securityFacade;
        }

        // GET: /Login/

        [HttpGet]
        public ActionResult Index(string responseId, string ReturnUrl)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            UserLoginModel UserLoginModel = new Models.UserLoginModel();
            ViewBag.Version = version;

            var isDemoMode = AppSettings.GetBoolValue(AppSettings.Key.IsDemoMode);

            Session[SessionKeys.IsDemoMode] = isDemoMode.ToString().ToUpper();

            if (isDemoMode)
            {
                string UserId = Epi.Common.Security.Cryptography.Encrypt("1");
                FormsAuthentication.SetAuthCookie("Guest@cdc.gov", false);

                Session[SessionKeys.UserId] = UserId;

                Session[SessionKeys.UserHighestRole] = 3;
                Session[SessionKeys.UserFirstName] = "John";
                Session[SessionKeys.UserLastName]= "Doe";
                Session[SessionKeys.UserEmailAddress] = "Guest@cdc.gov";
                return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = "" });
            }
            var configuration = WebConfigurationManager.OpenWebConfiguration("/");
            var authenticationSection = (AuthenticationSection)configuration.GetSection("system.web/authentication");
            if (authenticationSection.Mode == AuthenticationMode.Forms)
            {
                _logger.Information("Using Forms Authentication Mode");
                return View("Index", UserLoginModel);
            }
            else
            {
                _logger.Information("Using Active Directory Authentication Mode");
                var CurrentUserName = System.Web.HttpContext.Current.User.Identity.Name;
                try
                {
                    var UserAD = Utility.WindowsAuthentication.GetCurrentUserFromAd(CurrentUserName);
                    // validate user in EWE system
                    UserRequest User = new UserRequest();
                    User.IsAuthenticated = true;
                    User.User.EmailAddress = UserAD.EmailAddress;

                    UserResponse result = _securityFacade.GetUserInfo(User);
                    if (result != null && result.User.Count() > 0)
                    {
                        FormsAuthentication.SetAuthCookie(CurrentUserName.Split('\\')[0].ToString(), false);
                        string UserId = Epi.Common.Security.Cryptography.Encrypt(result.User[0].UserId.ToString());
                        Session[SessionKeys.UserId] = UserId;
                        //Session[SessionKeys.UsertRole] = result.User.Role;
                        Session[SessionKeys.UserHighestRole] = result.User[0].UserHighestRole;

                        Session[SessionKeys.UserEmailAddress] = result.User[0].EmailAddress;
                        Session[SessionKeys.UserFirstName] = result.User[0].FirstName;
                        Session[SessionKeys.UserLastName] = result.User[0].LastName;
                        Session[SessionKeys.UGuid] = result.User[0].UGuid;
                        return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = "" });
                    }
                    else
                    {

                        return View("Index", UserLoginModel);
                    }
                }
                catch (Exception ex)
                {
                    return View("Index", UserLoginModel);
                }
            }

        }


        [HttpPost]

        public ActionResult Index(UserLoginModel Model, string Action, string ReturnUrl)
        {

            return ValidateUser(Model, ReturnUrl);

            //if (ReturnUrl == null || !ReturnUrl.Contains("/"))
            //{
            //    ReturnUrl = "/Home/Index";
            //}


            //Epi.Cloud.Common.Message.UserAuthenticationResponse result = _isurveyFacade.ValidateUser(Model.UserName, Model.Password);

            //if (result.UserIsValid)
            //{
            //    if (result.User.ResetPassword)
            //    {
            //        return ResetPassword(Model.UserName);
            //    }
            //    else
            //    {

            //        FormsAuthentication.SetAuthCookie(Model.UserName, false);
            //        string UserId = Epi.Common.Security.Cryptography.Encrypt(result.User.UserId.ToString());
            //        Session[SessionKeys.UserId] = UserId;
            //        return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = "" });
            //    }
            //}
            //else
            //{
            //    ModelState.AddModelError("", "The email or password you entered is incorrect.");
            //    return View();
            //}
        }
        /// <summary>
        /// parse and return the responseId from response Url 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private string GetResponseId(string returnUrl)
        {
            string responseId = string.Empty;
            string[] expressions = returnUrl.Split('/');

            foreach (var expression in expressions)
            {
                if (Epi.Web.MVC.Utility.SurveyHelper.IsGuid(expression))
                {

                    responseId = expression;
                    break;
                }

            }
            return responseId;
        }


        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        //[HttpGet]
        public ActionResult ResetPassword(UserResetPasswordModel model)
        {
            return View("ResetPassword", model);
        }

        [HttpPost]
        public ActionResult ForgotPassword(UserForgotPasswordModel Model, string Action, string ReturnUrl)
        {
            switch (Action.ToUpper())
            {
                case "CANCEL":
                    return RedirectToAction(ViewActions.Index, ControllerNames.Login);
                default:
                    break;
            }

            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                List<string> errorMessages = new List<string>();

                string msg = ModelState.First().Value.Errors.First().ErrorMessage.ToString();

                ModelState.AddModelError("", msg);


                return View("ForgotPassword", Model);
            }

            bool success = _securityFacade.UpdateUser(new UserDTO() { UserName = Model.UserName, Operation = OperationMode.UpdatePassword });
            if (success)
            {
                return RedirectToAction(ViewActions.Index, ControllerNames.Login);
            }
            else
            {
                UserResetPasswordModel model = new UserResetPasswordModel();
                model.UserName = Model.UserName;
                 
                ReadPasswordPolicy(model);
                ModelState.AddModelError("UserName", "The email you provided does not exist.");
                return View("ForgotPassword", model);
            }

        }

        [HttpPost]
        public ActionResult ResetPassword(UserResetPasswordModel Model, string Action, string ReturnUrl)
        {

            switch (Action.ToUpper())
            {
                case "CANCEL":
                    return RedirectToAction(ViewActions.Index, ControllerNames.Login);
                default:
                    break;
            }

            if (!ModelState.IsValid)
            {
                UserResetPasswordModel model = new UserResetPasswordModel();
                model.UserName = Model.UserName;
                ReadPasswordPolicy(model);
               // ModelState.AddModelError("", "Passwords do not match. Please try again.");
                return View("ResetPassword", model);
            }

            if (!ValidatePassword(Model))
            {

                ModelState.AddModelError("", "Password is not strong enough. Please try again.");
                return View("ResetPassword", Model);
            }

            _securityFacade.UpdateUser(new UserDTO() { UserName = Model.UserName, PasswordHash = Model.Password, Operation = OperationMode.UpdatePassword, ResetPassword = true });
            UserLoginModel UserLoginModel = new UserLoginModel();
            UserLoginModel.Password = Model.Password;
            UserLoginModel.UserName = Model.UserName;
            return ValidateUser(UserLoginModel, ReturnUrl);

        }

        private ActionResult ValidateUser(UserLoginModel Model, string ReturnUrl)
        {
            string formId = "", pageNumber;
            
            if (ReturnUrl == null || !ReturnUrl.Contains("/"))
            {
                ReturnUrl = "/Home/Index";
            }
            else
            {
                formId = ReturnUrl.Substring(0, ReturnUrl.IndexOf('/'));
                pageNumber = ReturnUrl.Substring(ReturnUrl.LastIndexOf('/') + 1);
            }

            try
            {
                Epi.Cloud.Common.Message.UserAuthenticationResponse result = _securityFacade.ValidateUser(Model.UserName, Model.Password);
                if (result.UserIsValid)
                {
                    if (result.User.ResetPassword)
                    {
                        UserResetPasswordModel model = new UserResetPasswordModel();
                        model.UserName = Model.UserName;
                        model.FirstName = result.User.FirstName;
                        model.LastName = result.User.LastName;
                        ReadPasswordPolicy(model);
                        return ResetPassword(model);
                    }
                    else
                    {
                        string UserId = Epi.Common.Security.Cryptography.Encrypt(result.User.UserId.ToString());
                        OrganizationRequest request = new OrganizationRequest();
                        request.UserId = result.User.UserId;
                        request.UserRole = result.User.UserHighestRole;
                        OrganizationResponse organizations = _securityFacade.GetAdminOrganizations(request);


                        FormsAuthentication.SetAuthCookie(Model.UserName, false);
                        Session[SessionKeys.UserId] = UserId;
                        //Session[SessionKeys.UsertRole] = result.User.Role;
                        Session[SessionKeys.UserHighestRole] = result.User.UserHighestRole;
                        Session[SessionKeys.UserEmailAddress] = result.User.EmailAddress;
                        Session[SessionKeys.UserFirstName] = result.User.FirstName;
                        Session[SessionKeys.UserLastName] = result.User.LastName;
                        Session[SessionKeys.UserName] = result.User.UserName;
                        Session[SessionKeys.UGuid] = result.User.UGuid;
                        Session[SessionKeys.CurrentOrgId] = organizations.OrganizationList[0].OrganizationId;
                        return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = formId });
                        //return Redirect(ReturnUrl);
                    }
                }
                //else
                {
                    ModelState.AddModelError("", "The email or password you entered is incorrect.");
                  Model.ViewValidationSummary = true;
                 return View(Model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "The email or password you entered is incorrect.");
                Model.ViewValidationSummary = true;
                return View(Model);
                throw;
            }
        }

        private bool ValidatePassword(UserResetPasswordModel Model)
        {
            //int minLength = Convert.ToInt16(ConfigurationManager.AppSettings["PasswordMinimumLength"]);
            //int maxLength = Convert.ToInt16(ConfigurationManager.AppSettings["PasswordMaximumLength"]);
            //bool useSymbols = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSymbols"]); //= false;
            //bool useNumeric = Convert.ToBoolean(ConfigurationManager.AppSettings["UseNumbers"]); //= false;
            //bool useLowerCase = Convert.ToBoolean(ConfigurationManager.AppSettings["UseLowerCase"]);
            //bool useUpperCase = Convert.ToBoolean(ConfigurationManager.AppSettings["UseUpperCase"]);
            //bool useUserIdInPassword = Convert.ToBoolean(ConfigurationManager.AppSettings["UseUserIdInPassword"]);
            //bool useUserNameInPassword = Convert.ToBoolean(ConfigurationManager.AppSettings["UseUserNameInPassword"]);
            //int numberOfTypesRequiredInPassword = Convert.ToInt16(ConfigurationManager.AppSettings["NumberOfTypesRequiredInPassword"]);

            ReadPasswordPolicy(Model);

            int successCounter = 0;

            if (Model.UseSymbols && HasSymbol(Model.Password))
            {
                successCounter++;
            }

            if (Model.UseUpperCase && HasUpperCase(Model.Password))
            {
                successCounter++;
            }
            if (Model.UseLowerCase && HasLowerCase(Model.Password))
            {
                successCounter++;
            }
            if (Model.UseNumeric && HasNumber(Model.Password))
            {
                successCounter++;
            }

            if (Model.UseUserIdInPassword)
            {
                if (Model.Password.ToString().Contains(Model.UserName.Split('@')[0].ToString()))
                {
                    successCounter = 0;
                }

            }

            if (Model.UseUserNameInPassword)
            {
                if (Model.Password.ToString().Contains(Model.FirstName) || Model.Password.ToString().Contains(Model.LastName))
                {
                    successCounter = 0;
                }
            }

            if (Model.Password.Length < Model.MinimumLength || Model.Password.Length > Model.MaximumLength)
            {
                return false;
            }

            if (Model.NumberOfTypesRequiredInPassword <= successCounter && successCounter != 0)
            {
                return true;
            }

            return false;
        }

        private bool HasNumber(string password)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(password, @"\d");
        }

        private bool HasUpperCase(string password)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]");
        }

        private bool HasLowerCase(string password)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]");
        }

        private bool HasSymbol(string password)
        {
            bool result = false;

            result = System.Text.RegularExpressions.Regex.IsMatch(password, @"[" + ConfigurationManager.AppSettings["Symbols"].Replace(" ", "") + "]");

            if (result)//Validates if password has only allowed characters.
            {
                foreach (char character in password.ToCharArray())
                {
                    if (Char.IsPunctuation(character))
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(character.ToString(), @"[" + ConfigurationManager.AppSettings["Symbols"].Replace(" ", "") + "]"))
                        {
                            return false;
                        }
                    }
                }
            }

            return result;

        }

        private void ReadPasswordPolicy(UserResetPasswordModel Model)
        {
            Model.MinimumLength = Convert.ToInt16(ConfigurationManager.AppSettings["PasswordMinimumLength"]);
            Model.MaximumLength = Convert.ToInt16(ConfigurationManager.AppSettings["PasswordMaximumLength"]);
            Model.UseSymbols = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSymbols"]); //= false;
            Model.UseNumeric = Convert.ToBoolean(ConfigurationManager.AppSettings["UseNumbers"]); //= false;
            Model.UseLowerCase = Convert.ToBoolean(ConfigurationManager.AppSettings["UseLowerCase"]);
            Model.UseUpperCase = Convert.ToBoolean(ConfigurationManager.AppSettings["UseUpperCase"]);
            Model.UseUserIdInPassword = Convert.ToBoolean(ConfigurationManager.AppSettings["UseUserIdInPassword"]);
            Model.UseUserNameInPassword = Convert.ToBoolean(ConfigurationManager.AppSettings["UseUserNameInPassword"]);
            Model.NumberOfTypesRequiredInPassword = Convert.ToInt16(ConfigurationManager.AppSettings["NumberOfTypesRequiredInPassword"]);
            Model.Symbols = ConfigurationManager.AppSettings["Symbols"];
        }
    }
}
