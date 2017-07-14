using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Common.EmailServices;
using Epi.Common.EmailServices.Constants;
using Epi.Common.Security;
using Epi.Common.Security.Constants;

namespace Epi.Web.BLL
{
    public class User
    {
        public IUserDao _userDao;

        public User(IUserDao userDao)
        {
            _userDao = userDao;
        }

        public UserBO GetUser(UserBO user)
        {
            UserBO userResponseBO;
            string keyForUserPasswordSalt = ReadSalt();
            PasswordHasher passwordHasher = new PasswordHasher(keyForUserPasswordSalt);
            string salt = passwordHasher.CreateSalt(user.UserName);

            user.PasswordHash = passwordHasher.HashPassword(salt, user.PasswordHash);

            userResponseBO = _userDao.GetUser(user);
            if (userResponseBO != null)
                userResponseBO.UserHighestRole = _userDao.GetUserHighestRole(userResponseBO.UserId);

            return userResponseBO;
        }

        public bool GetExistingUser(UserBO user)
        {
            bool exists = false;
            exists = _userDao.GetExistingUser(user);

            return exists;
        }

        public bool IsUserExistsInOrganizaion(UserBO user, OrganizationBO orgBO)
        {
            bool exists = false;
            exists = _userDao.IsUserExistsInOrganizaion(user, orgBO);

            return exists;
        }

        private string ReadSalt()
        {
            return SecurityAppSettings.GetStringValue(SecurityAppSettings.Key.KeyForUserPasswordSalt);
        }

        public UserBO GetUserByUserId(UserBO user)
        {
            UserBO UserResponseBO;

            UserResponseBO = _userDao.GetUserByUserId(user);

            return UserResponseBO;
        }

        public bool UpdateUser(UserBO user, OrganizationBO orgBO)
        {
            bool success = false;
            switch (user.Operation)
            {
                case OperationMode.UpdatePassword:
                    string password = string.Empty;

                    if (user.ResetPassword)
                    {
                        password = user.PasswordHash;
                        user.ResetPassword = false;
                    }
                    else
                    {
                        PasswordGenerator passGen = new PasswordGenerator();
                        password = passGen.Generate();
                        user.ResetPassword = true;
                    }

                    string keyForUserPasswordSalt = ReadSalt();
                    PasswordHasher passwordHasher = new PasswordHasher(keyForUserPasswordSalt);
                    string salt = passwordHasher.CreateSalt(user.UserName);

                    user.PasswordHash = passwordHasher.HashPassword(salt, password);
                    success = _userDao.UpdateUserPassword(user);

                    if (success)
                    {
                        List<string> emailList = new List<string>();
                        emailList.Add(user.UserName);
                        Email email = new Email()
                        {
                            To = emailList,
                            Password = password
                        };

                        if (user.ResetPassword)
                        {
                            success = SendEmail(email, EmailCombinationEnum.ResetPassword);
                        }
                        else
                        {
                            success = SendEmail(email, EmailCombinationEnum.PasswordChanged);
                        }
                    }
                    return success;

                case OperationMode.UpdateUserInfo:
                    success = _userDao.UpdateUserInfo(user, orgBO);
                    //if (success)
                    //{
                    //    //List<string> EmailList = new List<string>();
                    //    //EmailList.Add(User.EmailAddress);
                    //    Email email = new Email();
                    //    email.To = new List<string>();
                    //    email.To.Add(User.EmailAddress);
                    //    success = SendEmail(email, EmailCombinationEnum.UpdateUserInfo);
                    //}
                    return success;

                default:
                    break;
            }
            return false;
        }

        private bool SendEmail(Email email, EmailCombinationEnum combination)
        {
            //   Epi.Common.Email.Email Email = new Web.Common.Email.Email();

            switch (combination)
            {
                case EmailCombinationEnum.ResetPassword:
                    //email.Subject = "Your Epi Info Cloud Enter Password";
                    //email.Body = string.Format("You recently accessed our Forgot Password service for Epi Info™ Cloud Enter. \n \n Your new temporary password is: {0}\n \n If you have not accessed password help, please contact the administrator. \n \nLog in with your temporary password. You will then be asked to create a new password.", email.Password);
                    email.Subject = ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.ResetPassword_Subject);
                    email.Body = string.Format(ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.ResetPassword_Body), email.Password);
                    break;
                case EmailCombinationEnum.PasswordChanged:
                    //email.Subject = "Your Epi Info Cloud Enter Password has been updated";
                    //email.Body = "You recently updated your password for Epi Info™ Cloud Enter. \n \n If you have not accessed password help, please contact the administrator for you organization. \n \n ";
                    email.Subject = ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.PasswordChanged_Subject);
                    email.Body = ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.PasswordChanged_Body);
                    break;
                case EmailCombinationEnum.UpdateUserInfo:
                    //email.Subject = "Your Epi Info Cloud Enter Account info has been updated";
                    //email.Body = " You account info has been updated in Epi Info™ Cloud Enter system.";
                    email.Subject = ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.UpdateUserInfo_Subject);
                    email.Body = ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.UpdateUserInfo_Body);
                    break;
                case EmailCombinationEnum.InsertUser:
                    //email.Subject = "An Epi Info Cloud Enter account has been created for your organization.";
                    email.Subject = ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.InsertUser_Subject);
                    break;
                default:
                    break;
            }

            //email.Body = email.Body.ToString() + " \n \nPlease click the link below to launch Epi Cloud Enter. \n" + AppSettings.GetStringValue(AppSettings.Key.BaseURL) + "\nThank you.";
            email.From = EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailUserName);

            return EmailHandler.SendMessage(email);
        }

        public UserBO GetUserByEmail(UserBO user)
        {
            UserBO userResponseBO;

            userResponseBO = _userDao.GetUserByEmail(user);
            if (userResponseBO != null)
                userResponseBO.UserHighestRole = _userDao.GetUserHighestRole(userResponseBO.UserId);
            return userResponseBO;
        }

        public List<UserBO> GetUsersByOrgId(int orgId)
        {
            List<UserBO> list = _userDao.GetUserByOrgId(orgId);
            return list;
        }

        public UserBO GetUserByUserIdAndOrgId(UserBO UserBO, OrganizationBO OrgBO)
        {
            UserBO UserResponseBO;

            UserResponseBO = _userDao.GetUserByUserIdAndOrgId(UserBO, OrgBO);

            return UserResponseBO;
        }

        public bool SetUserInfo(UserBO userBO, OrganizationBO orgBO)
        {
            //UserBO ExistingUser; //= GetUser(UserBO);
            //ExistingUser = UserDao.GetUserByEmail(UserBO);
            //ExistingUser.Role = UserDao.GetUserHighestRole(ExistingUser.UserId);

            bool success;
            if (userBO.UserName == null)
            {
                string keyForUserPasswordSalt = ReadSalt();
                PasswordHasher PasswordHasher = new PasswordHasher(keyForUserPasswordSalt);
                string salt = PasswordHasher.CreateSalt(userBO.EmailAddress);
                userBO.ResetPassword = true;
                PasswordGenerator passGen = new PasswordGenerator();
                string tempPassword = passGen.Generate();
                userBO.PasswordHash = PasswordHasher.HashPassword(salt, tempPassword);// "PassWord1");
                //UserBO.PasswordHash = PasswordHasher.HashPassword(salt, "PassWord1");
                success = _userDao.InsertUser(userBO, orgBO);
                StringBuilder body = new StringBuilder();
                var orgKey = Epi.Common.Security.Cryptography.Decrypt(orgBO.OrganizationKey);
                if (success)
                {
                    Email email = new Email();
                    body.Append("Welcome to Epi Info™ Cloud Enter. \nYour account has now been created for organization - " + orgBO.Organization + ".");
                    body.Append("\n\nEmail: " + userBO.EmailAddress + "\nPassword: " + tempPassword);
                    body.Append("\nOrganization Key: " + orgKey);
                    body.Append("\n\nPlease click the link below to launch the Epi Info™ Cloud Enter and log in with your email and temporary password. You will then be asked to create a new password. \n" + AppSettings.GetStringValue(AppSettings.Key.BaseURL));
                    //Add email and temporary password for new user. 



                    body.Append("\n\nPlease follow the steps below in order to start publishing forms to the web using Epi Info™ 7.");
                    body.Append("\n\tStep 1: Download and install the latest version of Epi Info™ 7 from:" + AppSettings.GetStringValue(AppSettings.Key.EpiInfoDownloadURL));
                    body.Append("\n\tStep 2: On the Main Menu, click on “Tools” and select “Options”");
                    body.Append("\n\tStep 3: On the Options dialog, click on the “Cloud Enter” Tab.");
                    body.Append("\n\tStep 4: On the Cloud Enter tab, enter the following information.");

                    body.Append("\n\t\t-Endpoint Address:" + AppSettings.GetStringValue(AppSettings.Key.EndpointAddress) + "\n\t\t-Connect using Windows Authentication:  " + AppSettings.GetStringValue(AppSettings.Key.WindowAuthentication));
                    body.Append("\n\t\t-Binding Protocol:" + AppSettings.GetStringValue(AppSettings.Key.BindingProtocol));

                    body.Append("\n\tStep 5:Click “OK’ button.");
                    body.Append("\nOrganization key provided here is to be used in Epi Info™ 7 during publish process.");
                    body.Append("\n\nPlease contact the system administrator for any questions.");

                    email.To = new List<string>();
                    email.To.Add(userBO.EmailAddress);
                    email.Body = body.ToString();
                    success = SendEmail(email, EmailCombinationEnum.InsertUser);
                }
            }
            else
            {
                //UserBO.Role = UserBO.Role;
                //UserBO.IsActive = UserBO.IsActive;
                success = _userDao.UpdateUserOrganization(userBO, orgBO);
                if (success)
                {
                    Email email = new Email();

                    StringBuilder body = new StringBuilder();

                    body.Append("Welcome to Epi Info™ Cloud Enter. \nYour account has now been created for organization - " + orgBO.Organization + ".");
                    // var orgKey = OrgBO.OrganizationKey;
                    var orgKey = Epi.Common.Security.Cryptography.Decrypt(orgBO.OrganizationKey);
                    body.Append("\n\nOrganization Key: " + orgKey);
                    body.Append("\n\nPlease click the link below to launch Epi Info™ Cloud Enter. \n" + AppSettings.GetStringValue(AppSettings.Key.BaseURL) + "\n\nThank you.");
                    email.Body = body.ToString();
                    email.To = new List<string>();
                    email.To.Add(userBO.EmailAddress);

                    success = SendEmail(email, EmailCombinationEnum.InsertUser);
                }
            }
            return success;
        }
    }
}
