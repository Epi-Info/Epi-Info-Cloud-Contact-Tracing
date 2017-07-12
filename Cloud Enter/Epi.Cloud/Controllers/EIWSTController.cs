using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web.Mvc;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Facades.Interfaces;
using Epi.Common.EmailServices;
using Epi.Common.EmailServices.Constants;
using Epi.Common.Security;
using Epi.Web.MVC.Models;

namespace Epi.Web.MVC.Controllers
{
    public class EIWSTController : Controller
    {
        private readonly ISurveyFacade _isurveyFacade;

        /// <summary>
        /// injecting surveyFacade to the constructor 
        /// </summary>
        /// <param name="surveyFacade"></param>
        public EIWSTController(ISurveyFacade isurveyFacade)
        {
            _isurveyFacade = isurveyFacade;
        }

        private enum TestResultEnum
        {
            Success,
            Error
        }

        [HttpGet]
        public ActionResult Index(string surveyid)
        {
            EIWSTModel TestModel = new EIWSTModel();
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                ViewBag.Version = version;

                string connectionString = ConnectionStrings.GetConnectionString(ConnectionStrings.Key.EWEADO);

                using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    TestModel.DBTestStatus = TestResultEnum.Success.ToString();

                    cmd.CommandText = "SELECT * FROM  lk_Status";
                    cmd.Parameters.AddWithValue("@StatusId", 1);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }
                        var TestValue = reader.GetString(reader.GetOrdinal("Status"));
                    }
                    TestModel.DBTestStatus = TestResultEnum.Success.ToString();
                }
            }
            catch (Exception ex)
            {

                TestModel.DBTestStatus = TestResultEnum.Error.ToString();
                TestModel.STestStatus = "Incomplete";
                TestModel.EFTestStatus = "Incomplete";
                TempData[TempDataKeys.ExceptionMessage] = ex.Message.ToString();
                TempData[TempDataKeys.ExceptionSource] = ex.Source.ToString();
                TempData[TempDataKeys.ExceptionStackTrace] = ex.StackTrace.ToString();

                return View(ViewActions.Index, TestModel);
            }

            try
            {
                Epi.Web.EF.EntityOrganizationDao NewEntity = new Epi.Web.EF.EntityOrganizationDao();
                List<OrganizationBO> OrganizationBO = new List<OrganizationBO>();
                OrganizationBO = NewEntity.GetOrganizationNames();
                if (OrganizationBO != null)
                {
                    TestModel.EFTestStatus = TestResultEnum.Success.ToString();
                }
            }
            catch (Exception ex)
            {
                TestModel.EFTestStatus = TestResultEnum.Error.ToString();
                TestModel.STestStatus = "Incomplete";
                TempData[TempDataKeys.ExceptionMessage] = ex.Message.ToString();
                TempData[TempDataKeys.ExceptionSource] = ex.Source.ToString();
                TempData[TempDataKeys.ExceptionStackTrace] = ex.StackTrace.ToString();

                return View(ViewActions.Index, TestModel);
            }

            return View(ViewActions.Index, TestModel);
        }

        // [AcceptVerbs(HttpVerbs.Post)]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult Notify(string emailAddress, string emailSubject)
        {
            string  message = "";
            try
            {
                var email = new Email();
                email.Body = "Test email From EWE System.";
                email.From = EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailFrom);
                email.Subject = emailSubject;

                List<string> tempList = new List<string>();
                tempList.Add(emailAddress);
                email.To = tempList;
                message = EmailHandler.SendNotification(email);
                if (message.Contains("Success"))
                {
                    return Json(true);
                }
                else
                {
                    return Json(message);
                }
            }
            catch (Exception ex)
            {
                return Json(message);
            }
        }
    }
}
