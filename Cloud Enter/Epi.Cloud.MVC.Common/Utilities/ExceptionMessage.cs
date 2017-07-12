using System;
using System.Web;
using Epi.Cloud.Common.Constants;
using Epi.Common.EmailServices.Constants;

namespace Epi.Web.Utility
{
    public class ExceptionMessage
    {

        /// <summary>
        /// the following method takes email and responseUrl as argument and email redirection url to the user 
        /// </summary>
        /// <param name="emailAddress">email address for sending message (email is NOT saved)</param>
        /// <param name="redirectUrl">url for resuming the saved survey</param>
        /// <param name="surveyName">Name of the survey</param>
        /// <param name="passCode"> Code for accessing an unfinished survey </param>
        /// <returns></returns>
        public static bool SendMessage(string emailAddress, string redirectUrl, string surveyName, string passCode, string EmailSubject)
        {
            try
            {
                bool isAuthenticated = false;
                bool isUsingSSL = false;
                int SMTPPort = 25;

                // App Config Settings:
                // EMAIL_USE_AUTHENTICATION [ True | False ] default is False
                // EMAIL_USE_SSL [ True | False] default is False
                // SMTP_HOST [ url or ip address of smtp server ]
                // SMTP_PORT [ port number to use ] default is 25
                // EMAIL_FROM [ email address of sender and authenticator ]
                // EMAIL_PASSWORD [ password of sender and authenticator ]


                isAuthenticated = EmailAppSettings.GetBoolValue(EmailAppSettings.Key.EmailUseAuthentication);
                isUsingSSL = EmailAppSettings.GetBoolValue(EmailAppSettings.Key.EmailUseSSL);
                SMTPPort = EmailAppSettings.GetIntValue(EmailAppSettings.Key.SmtpPort);

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.To.Add(emailAddress);
                message.Subject = EmailSubject;      // "Link for Survey: " + surveyName; 
                message.From = new System.Net.Mail.MailAddress(EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailFrom));
                message.Body = redirectUrl + " and Pass Code is: " + passCode;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(EmailAppSettings.GetStringValue(EmailAppSettings.Key.SmtpHost));
                smtp.Port = SMTPPort;

                if (isAuthenticated)
                {
                    smtp.Credentials = new System.Net.NetworkCredential(EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailFrom), EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailPassword));
                }

                smtp.EnableSsl = isUsingSSL;

                smtp.Send(message);

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// the following method sends email messages from loggin errors 
        /// </summary>
        /// <param name="emailAddress">email address for sending message (email is NOT saved)</param>
        /// <param name="pSubjectLine">subject text</param>
        /// <param name="pMessage">Message body text</param>
        /// <returns></returns>
        //public static bool SendLogMessage(string emailAddress, string pSubjectLine, Exception exc, HttpContextBase Context = null)
        public static bool SendLogMessage(Exception exc, HttpContextBase Context = null)
        {
            try
            {
                bool isAuthenticated = false;
                bool isUsingSSL = false;
                int SMTPPort = 25;
                string AdminEmailAddress = "";
                bool IsEmailNotification = false;
                // App Config Settings:
                // EMAIL_USE_AUTHENTICATION [ True | False ] default is False
                // EMAIL_USE_SSL [ True | False] default is False
                // SMTP_HOST [ url or ip address of smtp server ]
                // SMTP_PORT [ port number to use ] default is 25
                // EMAIL_FROM [ email address of sender and authenticator ]
                // EMAIL_PASSWORD [ password of sender and authenticator ]
                string pMessage;

                pMessage = "Exception Message:\n" + exc.Message + "\n\n\n";
                if (Context != null)
                {
                    pMessage += "Exception Timestamp:\n" + Context.Timestamp + "\n\n\n"
                        + "Request Path:\n " + (Context.Request).Path + "\n\n\n"
                        + "Request Method:\n" + (Context.Request).HttpMethod + "\n\n\n";
                }
                pMessage += "Inner Exception :\n" + exc.InnerException + ";" +
                            "Exception StackTrace:\n" + exc.StackTrace + "\n\n\n";

                if (Context != null && !string.IsNullOrEmpty(Context.Session[SessionKeys.UserFirstName].ToString()))
                {
                    pMessage += "Logged in User: \n" + Context.Session[SessionKeys.UserFirstName].ToString() + " " + Context.Session[SessionKeys.UserLastName].ToString() + "\n\n\n"; ;
                    pMessage += "Form Id: \n" + Context.Session[SessionKeys.RootFormId] + "\n\n\n"; ;
                    pMessage += "Response Id: \n" + Context.Session[SessionKeys.RootResponseId] + "\n\n\n"; ;
                }

                AdminEmailAddress = EmailAppSettings.GetStringValue(EmailAppSettings.Key.LoggingAdminEmailAddress);

                IsEmailNotification = EmailAppSettings.GetBoolValue(EmailAppSettings.Key.LoggingSendEmailNotification);

                isAuthenticated = EmailAppSettings.GetBoolValue(EmailAppSettings.Key.EmailUseAuthentication);

                isUsingSSL = EmailAppSettings.GetBoolValue(EmailAppSettings.Key.EmailUseSSL);

                SMTPPort = EmailAppSettings.GetIntValue(EmailAppSettings.Key.SmtpPort);

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                //message.To.Add(emailAddress);
                message.To.Add(AdminEmailAddress);
                message.Subject = EmailAppSettings.GetStringValue(EmailAppSettings.Key.LoggingEmailSubject);
                message.From = new System.Net.Mail.MailAddress(EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailFrom));
                message.Body = pMessage;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(EmailAppSettings.GetStringValue(EmailAppSettings.Key.SmtpHost));
                smtp.Port = SMTPPort;

                if (isAuthenticated)
                {
                    smtp.Credentials = new System.Net.NetworkCredential(EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailFrom), EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailPassword));
                }


                smtp.EnableSsl = isUsingSSL;

                if (IsEmailNotification)
                {
                    smtp.Send(message);
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }


}