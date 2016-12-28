using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using Epi.Common.EmailServices;

namespace Epi.Web.Controllers
{
    public class PostController : Controller
    {
        //
        // GET: /Notify/

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public JsonResult Notify(string emailAddress, string redirectUrl, string surveyName, string passCode, string EmailSubject)
        {
            try
            {
                var email = new Email();
                email.Body = redirectUrl + " and Pass Code is: " + passCode;
                email.From = ConfigurationManager.AppSettings["EMAIL_FROM"].ToString();
                email.Subject = "Link for Survey: " + surveyName;// EmailSubject;
                List<string> tempList = new List<string>();
                tempList.Add(emailAddress);
                email.To = tempList;

                if (EmailHandler.SendMessage(email))
                {
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }


        /// <summary>
        /// Sign out a Survey Instance
        /// </summary>
        /// <returns></returns>
        [HttpPost]

        // [ValidateAntiForgeryToken]
        public JsonResult SignOut()
        {
            try
            {
                FormsAuthentication.SignOut();
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        public static string UnescapeCodes(string src)
        {
            src = Uri.UnescapeDataString(src);
            var rx = new Regex("\\\\([0-9A-Fa-f]+)");
            var res = new StringBuilder();
            var pos = 0;
            foreach (Match m in rx.Matches(src))
            {
                res.Append(src.Substring(pos, m.Index));
                pos = m.Index + m.Length;
                res.Append((char)Convert.ToInt32(m.Groups[1].ToString(), 16));
            }
            res.Append(src.Substring(pos));


            return res.ToString();
        }

    }
}
