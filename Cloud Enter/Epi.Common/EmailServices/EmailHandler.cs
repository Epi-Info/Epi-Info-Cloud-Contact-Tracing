using System;
using System.Configuration;
using System.Net.Mail;
using Epi.Common.EmailServices.Constants;
using Epi.Common.Security;

namespace Epi.Common.EmailServices
{
    public class EmailHandler
    {

        /// <summary>
        /// the following method takes email and responseUrl as argument and email redirection url to the user 
        /// </summary>
        /// <param name="emailAddress">email address for sending message (email is NOT saved)</param>
        /// <param name="redirectUrl">url for resuming the saved survey</param>
        /// <param name="surveyName">Name of the survey</param>
        /// <param name="passCode"> Code for accessing an unfinished survey </param>
        /// <returns></returns>


        public static bool SendMessage(Email Email)
        {
            try
            {
                bool isAuthenticated = false;
                bool isUsingSSL = false;
                int smptPort = 25;

                // App Config Settings:
                // EMAIL_USE_AUTHENTICATION [ True | False ] default is False
                // EMAIL_USE_SSL [ True | False] default is False
                // SMTP_HOST [ url or ip address of smtp server ]
                // SMTP_PORT [ port number to use ] default is 25
                // EMAIL_FROM [ email address of sender and authenticator ]
                // EMAIL_PASSWORD [ password of sender and authenticator ]


                isAuthenticated = EmailAppSettings.GetBoolValue(EmailAppSettings.Key.EmailUseAuthentication);

                isUsingSSL = EmailAppSettings.GetBoolValue(EmailAppSettings.Key.EmailUseSSL);

                smptPort = EmailAppSettings.GetIntValue(EmailAppSettings.Key.SmtpPort);

                MailMessage message = new MailMessage();
                foreach (string item in Email.To)
                {
                    message.To.Add(item);
                }

                message.Subject = Email.Subject;

                var userName = EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailUserName);
                message.From = new MailAddress(EmailAppSettings.GetStringValue(EmailAppSettings.Key.LoggingAdminEmailAddress));
                var smtpHost = EmailAppSettings.GetStringValue(EmailAppSettings.Key.SmtpHost);
                message.Body = Email.Body;
                SmtpClient smtp = new SmtpClient(smtpHost, smptPort);

                var passWord = EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailPassword);

                if (isAuthenticated)
                {
                    smtp.Credentials = new System.Net.NetworkCredential(userName, passWord);
                }

                smtp.EnableSsl = isUsingSSL;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                smtp.Send(message);

                return true;

            }
            catch (System.Exception ex)
            {
                return false;
            }
        }



        public static string SendNotification(Email Email)
        {
            try
            {
                bool isAuthenticated = false;
                bool isUsingSSL = false;
                int smtpPort = 25;

                // App Config Settings:
                // EMAIL_USE_AUTHENTICATION [ True | False ] default is False
                // EMAIL_USE_SSL [ True | False] default is False
                // SMTP_HOST [ url or ip address of smtp server ]
                // SMTP_PORT [ port number to use ] default is 25
                // EMAIL_FROM [ email address of sender and authenticator ]
                // EMAIL_PASSWORD [ password of sender and authenticator ]


                string s = ConfigurationManager.AppSettings["EMAIL_USE_AUTHENTICATION"];
                if (!String.IsNullOrEmpty(s))
                {
                    if (s.ToUpper() == "TRUE")
                    {
                        isAuthenticated = true;
                    }
                }

                s = ConfigurationManager.AppSettings["EMAIL_USE_SSL"];
                if (!String.IsNullOrEmpty(s))
                {
                    if (s.ToUpper() == "TRUE")
                    {
                        isUsingSSL = true;
                    }
                }

                s = ConfigurationManager.AppSettings["SMTP_PORT"];
                if (!int.TryParse(s, out smtpPort))
                {
                    smtpPort = 25;
                }

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                foreach (string item in Email.To)
                {
                    message.To.Add(item);
                }

                message.Subject = Email.Subject;
                message.From = new System.Net.Mail.MailAddress(Email.From.ToString());
                // message.From = new MailAddress("renuka_yarakaraju@sra.com", "CloudEnter");
                message.Body = Email.Body;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(EmailAppSettings.GetStringValue(EmailAppSettings.Key.SmtpHost));

                if (isAuthenticated)
                {
                    smtp.Credentials = new System.Net.NetworkCredential(EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailUserName), EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailPassword));
                }


                smtp.EnableSsl = isUsingSSL;


                smtp.Send(message);

                return "Success";

            }
            catch (System.Exception ex)
            {
                return ex.InnerException.ToString();
            }
        }
    }
}


