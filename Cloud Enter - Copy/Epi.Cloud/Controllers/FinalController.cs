using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Security;
using Epi.Cloud.DataEntryServices.Model;
using Epi.Web.MVC.Facade;
using Epi.Web.MVC.Models;
namespace Epi.Web.MVC.Controllers
{
    public class FinalController : Controller
    {
        private ISurveyFacade _isurveyFacade;

        /// <summary>
        /// Injecting SurveyTransactionObject through constructor
        /// </summary>
        /// <param name="iSurveyInfoRepository"></param>
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

                string SurveyMode = "";
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
                bool IsMobileDevice = false;
                IsMobileDevice = this.Request.Browser.IsMobileDevice;
                Omniture OmnitureObj = Epi.Web.MVC.Utility.OmnitureHelper.GetSettings(SurveyMode, IsMobileDevice);

                ViewBag.Omniture = OmnitureObj;
                return View(Epi.Web.MVC.Constants.Constant.INDEX_PAGE, surveyInfoModel);
            }
            catch (Exception ex)
            {
                Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);
                return View(Epi.Web.MVC.Constants.Constant.EXCEPTION_PAGE);
            }
        }

        [HttpPost]
        public ActionResult Index(string surveyId, SurveyAnswerModel surveyAnswerModel)
        {
            try
            {
                bool IsMobileDevice = this.Request.Browser.IsMobileDevice;

                if (IsMobileDevice == false)
                {
                    IsMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
                }

                FormsAuthentication.SetAuthCookie("BeginSurvey", false);
                Guid responseId = Guid.NewGuid();
                Epi.Web.Enter.Common.DTO.SurveyAnswerDTO SurveyAnswer = _isurveyFacade.CreateSurveyAnswer(surveyId, responseId.ToString(), 0);
                SurveyInfoModel surveyInfoModel = GetSurveyInfo(SurveyAnswer.SurveyId);

                MvcDynamicForms.Form form = _isurveyFacade.GetSurveyFormData(SurveyAnswer.SurveyId, 1, SurveyAnswer, IsMobileDevice);

                List<string> requiredFields = null;

                foreach (var field in surveyInfoModel.CurrentFormFieldDigests) 
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

                _isurveyFacade.UpdateSurveyResponse(surveyInfoModel, SurveyAnswer.ResponseId, form, SurveyAnswer, false, false, 1, 0);

                return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseId = responseId, PageNumber = 1 });
            }
            catch (Exception ex)
            {
                Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);
                return View(Epi.Web.MVC.Constants.Constant.EXCEPTION_PAGE);
            }

        }
        public SurveyInfoModel GetSurveyInfo(string SurveyId)
        {
            SurveyInfoModel surveyInfoModel = _isurveyFacade.GetSurveyInfoModel(SurveyId);
            return surveyInfoModel;
        }
    }
}
