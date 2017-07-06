using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Security;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Model;
using Epi.Cloud.Facades.Interfaces;
using Epi.Common.Core.DataStructures;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;

namespace Epi.Web.MVC.Controllers
{
    public class FinalController : Controller
    {
        private readonly ISurveyFacade _isurveyFacade;

		/// <summary>
		/// Inject ISurveyFacade
		/// </summary>
		/// <param name="isurveyFacade"></param>
        public FinalController(ISurveyFacade isurveyFacade)
        {
            _isurveyFacade = isurveyFacade;
        }

        [HttpGet]
        public ActionResult Index(string surveyId, string final)
        {
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                ViewBag.Version = version;

                string surveyMode = "";
                SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyId);
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(\r\n|\r|\n)+");

                string exitText = regex.Replace(surveyInfoModel.ExitText.Replace("  ", " &nbsp;"), "<br />");
                surveyInfoModel.ExitText = MvcHtmlString.Create(exitText).ToString();

                if (surveyInfoModel.IsDraftMode)
                {
                    surveyInfoModel.IsDraftModeStyleClass = "draft";
                }
                else
                {
                    surveyInfoModel.IsDraftModeStyleClass = "final";
                }
                bool isMobileDevice = false;
                isMobileDevice = this.Request.Browser.IsMobileDevice;

                return View(ViewActions.Index, surveyInfoModel);
            }
            catch (Exception ex)
            {
                Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);
                return View(ViewActions.Exception);
            }
        }

        [HttpPost]
        public ActionResult Index(string surveyId, SurveyAnswerModel surveyAnswerModel)
        {
            try
            {
                bool isMobileDevice = this.Request.Browser.IsMobileDevice;

                if (isMobileDevice == false)
                {
                    isMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
                }

                FormsAuthentication.SetAuthCookie("BeginSurvey", false);
                Guid responseId = Guid.NewGuid();
                string rootResponseId = Session[SessionKeys.RootResponseId].ToString();
                int orgId = Convert.ToInt32(Session[SessionKeys.CurrentOrgId]);
                int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());

                var responseContext = new ResponseContext
                {
                    FormId = surveyId,
                    ResponseId = responseId.ToString(),
                    RootResponseId = rootResponseId,
                    UserOrgId = orgId,
                }.ResolveMetadataDependencies() as ResponseContext;

                SurveyAnswerDTO surveyAnswer = _isurveyFacade.CreateSurveyAnswer(responseContext);
                SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswer.SurveyId);

                MvcDynamicForms.Form form = _isurveyFacade.GetSurveyFormData(surveyAnswer.SurveyId, 1, surveyAnswer, isMobileDevice);

                List<string> requiredFields = null;

                foreach (var field in surveyInfoModel.GetFieldDigests(surveyId))
                {
                    bool isRequired = field.IsRequired;

                    if (isRequired)
                    {
                        // if this is the first new required field then split the existing required CSV into a true list.
                        requiredFields = requiredFields ?? new List<string>(form.RequiredFieldsList.Split(','));

                        if (!requiredFields.Contains(field.FieldName))
                        {
                            requiredFields.Add(field.FieldName.ToLower());
                        }
                    }
                }

                // if we processed at least 1 reqired field then join the required field list into a CSV list
                if (requiredFields != null)
                {
                    form.RequiredFieldsList = string.Join(",", requiredFields);
                }

                _isurveyFacade.UpdateSurveyResponse(surveyInfoModel, surveyAnswer.ResponseId, form, surveyAnswer, false, false, 1, orgId, userId, null);

                return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseId = responseId, PageNumber = 1 });
            }
            catch (Exception ex)
            {
                Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);
                return View(ViewActions.Exception);
            }

        }
        public SurveyInfoModel GetSurveyInfo(string SurveyId)
        {
            SurveyInfoModel surveyInfoModel = _isurveyFacade.GetSurveyInfoModel(SurveyId);
            return surveyInfoModel;
        }
    }
}
