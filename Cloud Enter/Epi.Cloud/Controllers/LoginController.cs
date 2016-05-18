using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Reflection;
using Epi.Web.Enter.Common.Diagnostics;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Constants;

namespace Epi.Web.MVC.Controllers
{

    public class LoginController : Controller
    {
        private readonly ILogger _logger;

        private Epi.Web.MVC.Facade.ISecurityFacade _isecurityFacade;
        //declare SurveyTransactionObject object
        private Epi.Web.MVC.Facade.ISurveyFacade _isurveyFacade;
        /// <summary>
        /// Injectinting SurveyTransactionObject through Constructor
        /// </summary>
        /// <param name="iSurveyInfoRepository"></param>

        public LoginController(ILogger logger, Epi.Web.MVC.Facade.ISurveyFacade isurveyFacade, Epi.Web.MVC.Facade.ISecurityFacade isecurityFacade)
        {
            _logger = logger;
            _isurveyFacade = isurveyFacade;
            _isecurityFacade = isecurityFacade;
        }

        // GET: /Login/

        [HttpGet]
        public ActionResult Index(string responseId, string ReturnUrl)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            if (ConfigurationManager.AppSettings["IsDemoMode"] != null)
                Session[SessionKeys.IsDemoMode] = ConfigurationManager.AppSettings["IsDemoMode"].ToUpper();
            else
                Session[SessionKeys.IsDemoMode] = "null";
            if (System.Configuration.ConfigurationManager.AppSettings["IsDemoMode"] != null)
            {
                var IsDemoMode = System.Configuration.ConfigurationManager.AppSettings["IsDemoMode"];
                string UserId = Epi.Web.Enter.Common.Security.Cryptography.Encrypt("1");
                if (!string.IsNullOrEmpty(IsDemoMode) && IsDemoMode.ToUpper() == "TRUE")
                {
                    FormsAuthentication.SetAuthCookie("Guest@cdc.gov", false);

                    Session[SessionKeys.UserId] = UserId;

                    Session[SessionKeys.UserHighestRole] = 3;
                    return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Home", new { surveyid = "" });
                }
            }
            var configuration = WebConfigurationManager.OpenWebConfiguration("/");
            var authenticationSection = (AuthenticationSection)configuration.GetSection("system.web/authentication");
            if (authenticationSection.Mode == AuthenticationMode.Forms)
            {
                _logger.Information("Using Forms Authentication Mode");
                return View("Index");
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

                    UserResponse result = _isurveyFacade.GetUserInfo(User);
                    if (result != null && result.User.Count() > 0)
                    {
                        FormsAuthentication.SetAuthCookie(CurrentUserName.Split('\\')[0].ToString(), false);
                        string UserId = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(result.User[0].UserId.ToString());
                        Session[SessionKeys.UserId] = UserId;
                        //Session[SessionKeys.UsertRole] = result.User.Role;
                        Session[SessionKeys.UserHighestRole] = result.User[0].UserHighestRole;
                        Session[SessionKeys.UserEmailAddress] = result.User[0].EmailAddress;
                        Session[SessionKeys.UserFirstName] = result.User[0].FirstName;
                        Session[SessionKeys.UserLastName] = result.User[0].LastName;
                        Session[SessionKeys.UGuid] = result.User[0].UGuid;
                        return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Home", new { surveyid = "" });
                    }
                    else
                    {

                        return View("Index");
                    }
                }
                catch (Exception ex)
                {

                    return View("Index");
                }
            }

        }


        [HttpPost]

        public ActionResult Index(UserLoginModel Model, string Action, string ReturnUrl)
        {

            return ValidateUser(Model.UserName, Model.Password, ReturnUrl);

            //if (ReturnUrl == null || !ReturnUrl.Contains("/"))
            //{
            //    ReturnUrl = "/Home/Index";
            //}


            //Epi.Web.Enter.Common.Message.UserAuthenticationResponse result = _isurveyFacade.ValidateUser(Model.UserName, Model.Password);

            //if (result.UserIsValid)
            //{
            //    if (result.User.ResetPassword)
            //    {
            //        return ResetPassword(Model.UserName);
            //    }
            //    else
            //    {

            //        FormsAuthentication.SetAuthCookie(Model.UserName, false);
            //        string UserId = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(result.User.UserId.ToString());
            //        Session[SessionKeys.UserId] = UserId;
            //        return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Home", new { surveyid = "" });
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
                    return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Login");
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

            bool success = _isecurityFacade.UpdateUser(new Enter.Common.DTO.UserDTO() { UserName = Model.UserName, Operation = Epi.Web.Enter.Common.Constants.Constant.OperationMode.UpdatePassword });
            if (success)
            {
                return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Login");
            }
            else
            {
                ModelState.AddModelError("", "Error sending email.");
                return View("ForgotPassword", Model);
            }

        }

        [HttpPost]
        public ActionResult ResetPassword(UserResetPasswordModel Model, string Action, string ReturnUrl)
        {

            switch (Action.ToUpper())
            {
                case "CANCEL":
                    return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Login");
                default:
                    break;
            }

            if (!ModelState.IsValid)
            {
                UserResetPasswordModel model = new UserResetPasswordModel();
                model.UserName = Model.UserName;
                ModelState.AddModelError("", "Passwords do not match. Please try again.");
                return View("ResetPassword", Model);
            }

            if (!ValidatePassword(Model))
            {
                ModelState.AddModelError("", "Password is not strong enough. Please try again.");
                return View("ResetPassword", Model);
            }

            _isecurityFacade.UpdateUser(new Enter.Common.DTO.UserDTO() { UserName = Model.UserName, PasswordHash = Model.Password, Operation = Epi.Web.Enter.Common.Constants.Constant.OperationMode.UpdatePassword, ResetPassword = true });

            return ValidateUser(Model.UserName, Model.Password, ReturnUrl);

        }

        private ActionResult ValidateUser(string UserName, string Password, string ReturnUrl)
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
                Epi.Web.Enter.Common.Message.UserAuthenticationResponse result = _isecurityFacade.ValidateUser(UserName, Password);
                if (result.UserIsValid)
                {
                    if (result.User.ResetPassword)
                    {
                        UserResetPasswordModel model = new UserResetPasswordModel();
                        model.UserName = UserName;
                        model.FirstName = result.User.FirstName;
                        model.LastName = result.User.LastName;
                        ReadPasswordPolicy(model);
                        return ResetPassword(model);
                    }
                    else
                    {

                        FormsAuthentication.SetAuthCookie(UserName, false);
                        string UserId = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(result.User.UserId.ToString());
                        Session[SessionKeys.UserId] = UserId;
                        //Session[SessionKeys.UsertRole] = result.User.Role;
                        Session[SessionKeys.UserHighestRole] = result.User.UserHighestRole;
                        Session[SessionKeys.UserEmailAddress] = result.User.EmailAddress;
                        Session[SessionKeys.UserFirstName] = result.User.FirstName;
                        Session[SessionKeys.UserLastName] = result.User.LastName;
                        Session[SessionKeys.UGuid] = result.User.UGuid;
                        return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Home", new { surveyid = formId });
                        //return Redirect(ReturnUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The email or password you entered is incorrect.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "The email or password you entered is incorrect.");
                return View();
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
