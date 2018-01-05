using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Common.Model;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.MVC.Constants;
using Epi.Cloud.MVC.Extensions;
using Epi.Cloud.MVC.Models;
using Epi.Cloud.MVC.Utility;
using Epi.Common.Core.DataStructures;
using Epi.Core.EnterInterpreter;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.DataEntryServices.Extensions;

namespace Epi.Cloud.MVC.Controllers
{
    [Authorize]
    public class SurveyController : BaseSurveyController
    {
        private readonly ISecurityFacade _securityFacade;

        private string _requiredList = "";
        private string RootFormId = "";
        private string RootResponseId = "";
        private bool IsEditMode;
        private List<SurveyAnswerDTO> ListSurveyAnswerDTO = new List<SurveyAnswerDTO>();
        private int referrerPageNum;

        public SurveyController(ISurveyFacade surveyFacade,
                                ISecurityFacade securityFacade,
                                IProjectMetadataProvider projectMetadataProvider)
        {
            _surveyFacade = surveyFacade;
            _securityFacade = securityFacade;
            _projectMetadataProvider = projectMetadataProvider;
        }

        /// <summary>
        /// create the new resposeid and put it in temp data. create the form object. create the first survey response
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(string responseId, int PageNumber = 1, string EDIT = "", string FormValuesHasChanged = "", string surveyId = "")
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                if (GetStringSessionValue(UserSession.Key.RootResponseId) == responseId)
                {
                    RemoveSessionValue(UserSession.Key.RelateButtonPageId);
                }
                bool isAndroid = this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!IsSessionValueNull(UserSession.Key.IsEditMode))
                {
                    this.IsEditMode = GetBoolSessionValue(UserSession.Key.IsEditMode);
                }

                var surveyModel = GetIndex(responseId, PageNumber, IsEditMode, surveyId, isAndroid);

                string DateFormat = currentCulture.DateTimeFormat.ShortDatePattern;
                DateFormat = DateFormat.Remove(DateFormat.IndexOf("y"), 2);
                surveyModel.CurrentCultureDateFormat = DateFormat;
                return View(ViewActions.Index, surveyModel);
            }
            catch (Exception ex)
            {
                Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);

                ExceptionModel ExModel = new ExceptionModel();
                ExModel.ExceptionDetail = "Stack Trace : " + ex.StackTrace;
                ExModel.Message = ex.Message;

                return View(ViewActions.Exception, ExModel);
            }
        }

        private SurveyModel GetIndex(string responseId, int viewPageNumber, bool isEditMode, string surveyId, bool isAndroid = false)
        {
            SetGlobalVariable();
            ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Edit = IsEditMode ? "Edit" : string.Empty;

            var forms = MetadataAccessor.FormDigests;

            string formId = "";
            int requestedViewId;

            if (!string.IsNullOrEmpty(surveyId))
            {
                requestedViewId = forms.Where(x => x.FormId == surveyId).Select(x => x.ViewId).FirstOrDefault();
                SetSessionValue(UserSession.Key.IsSqlProject, true);
                SetSessionValue(UserSession.Key.RequestedViewId, requestedViewId);
            }
            else
            {
                requestedViewId = GetIntSessionValue(UserSession.Key.RequestedViewId, defaultValue: 0);
                if (requestedViewId != 0)
                {
                    surveyId = formId = forms.SingleOrDefault(x => x.ViewId == requestedViewId).FormId;
                }
            }

            //Update Status
            SurveyAnswerDTO surveyAnswerDTO = UpdateStatus(responseId, formId, RecordStatus.InProcess, RecordStatusChangeReason.OpenForEdit);


            //Mobile Section
            bool isMobileDevice = false;
            isMobileDevice = this.Request.Browser.IsMobileDevice;
            if (isMobileDevice == false)
            {
                isMobileDevice = Epi.Cloud.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }

            //List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();
            //SurveyAnswerDTO surveyAnswerDTO = (SurveyAnswerDTO)FormsHierarchy.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == responseId);

            if (isEditMode)
            {
                if (isMobileDevice == false)
                {
                    SetSessionValue(UserSession.Key.RootFormId, surveyAnswerDTO.RootFormId);
                }
            }

            PreValidationResultEnum ValidationTest;
            ValidationTest = PreValidateResponse(surveyAnswerDTO.ToSurveyAnswerModel(responseId));

            if (surveyAnswerDTO.ResponseDetail == null) surveyAnswerDTO.ResponseDetail = InitializeFormResponseDetail();
            var responseDetail = surveyAnswerDTO.ResponseDetail.FindFormResponseDetail(responseId);
            surveyAnswerDTO = responseDetail.ToSurveyAnswerDTO(MetadataAccessor);

            formId = responseDetail.FormId;
            string formName = MetadataAccessor.GetFormDigest(formId).FormName;

            if (viewPageNumber == 0) viewPageNumber = responseDetail.LastPageVisited;

            switch (ValidationTest)
            {
                case PreValidationResultEnum.Success:
                default:

                    //formId = surveyAnswerDTO != null && !string.IsNullOrWhiteSpace(surveyAnswerDTO.SurveyId) ? surveyAnswerDTO.SurveyId : surveyId;
                    var form = _surveyFacade.GetSurveyFormData(formId, viewPageNumber, surveyAnswerDTO, isMobileDevice, null, GetFormsHierarchy(), isAndroid);

                    ////////////////Assign data to a child
                    TempData[TempDataKeys.Width] = form.Width + 5;
                    // if redirect then perform server validation before displaying
                    if (TempData.ContainsKey(TempDataKeys.IsRedirect) && !string.IsNullOrWhiteSpace(TempData[TempDataKeys.IsRedirect].ToString()))
                    {
                        form.Validate(form.RequiredFieldsList);
                    }
                    //if (string.IsNullOrEmpty(Edit))
                    //    {
                    //    surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;

                    //    }
                    if (!isEditMode)
                    {
                        this.SetCurrentPage(surveyAnswerDTO, viewPageNumber);
                    }
                    //PassCode start
                    if (isMobileDevice)
                    {
                        form = SetFormPassCode(form, responseId);
                    }
                    form.StatusId = surveyAnswerDTO.Status;
                    if (isEditMode)
                    {
                        if (surveyAnswerDTO.IsDraftMode)
                        {
                            form.IsDraftModeStyleClass = "draft";
                        }
                    }
                    if (!IsSessionValueNull(UserSession.Key.FormValuesHasChanged))
                    {
                        form.FormValuesHasChanged = GetStringSessionValue(UserSession.Key.FormValuesHasChanged);
                    }
                    form.RequiredFieldsList = this._requiredList;
                    //passCode end
                    SurveyModel SurveyModel = new SurveyModel();
                    SurveyModel.Form = form;
                    SurveyModel.RelateModel = surveyAnswerDTO.ToRelateModel();
                    return SurveyModel;
            }
        }

        private SurveyAnswerDTO UpdateStatus(string responseId, string surveyId, int statusId, RecordStatusChangeReason statusChangeReason)
        {
            ResponseContext responseContext = InitializeResponseContext(formId: surveyId, responseId: responseId) as ResponseContext;

            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest { ResponseContext = responseContext };

            surveyAnswerRequest.SurveyAnswerList.Add(responseContext.ToSurveyAnswerDTOLite());
            surveyAnswerRequest.Criteria.StatusId = statusId;
            surveyAnswerRequest.Criteria.StatusChangeReason = statusChangeReason;

            surveyAnswerRequest.Criteria.UserOrganizationId = responseContext.UserOrgId;
            surveyAnswerRequest.Criteria.UserId = responseContext.UserId;
            surveyAnswerRequest.Criteria.UserName = responseContext.UserName;
            if (!string.IsNullOrEmpty(surveyId))
            {
                surveyAnswerRequest.Criteria.SurveyId = surveyId;
            }

            SurveyAnswerResponse surveyAnswerResponse = _surveyFacade.UpdateResponseStatus(surveyAnswerRequest);
            var responseDetail = surveyAnswerResponse.SurveyResponseList[0].ResponseDetail.FindFormResponseDetail(responseId);
            SurveyAnswerDTO surveyAnswerDTO = responseDetail.ToSurveyAnswerDTO(MetadataAccessor);

            //SurveyAnswerDTO surveyAnswerDTO = surveyAnswerResponse != null && surveyAnswerResponse.SurveyResponseList.Count == 1 
            //    ? surveyAnswerResponse.SurveyResponseList[0] : null;
            return surveyAnswerDTO;
        }

        [HttpPost]
        //  [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        [ValidateAntiForgeryToken]

        // User Action: Page Navigation, Save
        public ActionResult Index(SurveyAnswerModel surveyAnswerModel,
            string Submitbutton,
            string Savebutton,
            string ContinueButton,
            string PreviousButton,
            string Close,
            string CloseButton,
            int pageNumber = 0,
            string Form_Has_Changed = "",
            string Requested_View_Id = "",
            bool Log_Out = false
            )
        {

            SetGlobalVariable();
            ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);
            string responseId = surveyAnswerModel.ResponseId;
            string rootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId);
            SetSessionValue(UserSession.Key.FormValuesHasChanged, Form_Has_Changed);

            bool isAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isAndroid = true;
            }
            if (rootResponseId == responseId)
            {
                RemoveSessionValue(UserSession.Key.RelateButtonPageId);
            }

            bool isMobileDevice = false;
            isMobileDevice = this.Request.Browser.IsMobileDevice;
            if (isMobileDevice == false)
            {
                isMobileDevice = Epi.Cloud.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }

          

            try
            {
                string formValuesHasChanged = Form_Has_Changed;

                List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();
                SurveyAnswerDTO surveyAnswer = (SurveyAnswerDTO)FormsHierarchy.SelectMany(x => x.ResponseIds).First(z => z.ResponseId == responseId);
                surveyAnswer.RequestedViewId = Requested_View_Id;
                surveyAnswer.CurrentPageNumber = pageNumber != 0 ? pageNumber : 1;

                // Fill in any missing organization ids
                surveyAnswer.UserOrgId = surveyAnswer.UserOrgId != 0 ? surveyAnswer.UserOrgId : orgId;
                surveyAnswer.LoggedInUserOrgId = surveyAnswer.LoggedInUserOrgId != 0 ? surveyAnswer.LoggedInUserOrgId : orgId;
                surveyAnswer.LastActiveOrgId = surveyAnswer.LastActiveOrgId != 0 ? surveyAnswer.LastActiveOrgId : orgId;

                //object temp = System.Web.HttpContext.Current.Cache;
                SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswer.SurveyId, FormsHierarchy);

                //////////////////////Update Survey Mode//////////////////////////
                surveyAnswer.IsDraftMode = surveyInfoModel.IsDraftMode;
                PreValidationResultEnum validationTest = PreValidateResponse(surveyAnswer.ToSurveyAnswerModel());

                switch (validationTest)
                {
                    case PreValidationResultEnum.SurveyIsPastClosingDate:
                        return View("SurveyClosedError");

                    case PreValidationResultEnum.SurveyIsAlreadyCompleted:
                        return View("IsSubmitedError");

                    case PreValidationResultEnum.Success:
                    default:

                        MvcDynamicForms.Form form = UpdateSurveyModel(surveyInfoModel, isMobileDevice, formValuesHasChanged, surveyAnswer);

                        if (isMobileDevice)
                        {
                            form = SetFormPassCode(form, responseId);
                        }

                        form.StatusId = surveyAnswer.Status;
                        bool isSubmited = false;
                        bool isSaved = false;

                        form = SetLists(form);

                        _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswer, isSubmited, isSaved, pageNumber, orgId, userId, userName);

                        if (!string.IsNullOrEmpty(this.Request.Form["is_save_action"]) && this.Request.Form["is_save_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            form = SaveCurrentForm(form, surveyInfoModel, surveyAnswer, responseId, orgId, userId, userName, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, pageNumber, FormsHierarchy);
                            form = SetLists(form);
                            TempData[TempDataKeys.Width] = form.Width + 5;
                            SurveyModel SurveyModel = new SurveyModel();
                            SurveyModel.Form = form;
                            SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(form.SurveyInfo.SurveyId);

                            return View(ViewActions.Index, SurveyModel);
                        }
                        else if (!string.IsNullOrEmpty(this.Request.Form["Go_Home_action"]) && this.Request.Form["Go_Home_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            isSaved = true;
                            form = SaveCurrentForm(form, surveyInfoModel, surveyAnswer, responseId, orgId, userId, userName, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, pageNumber, FormsHierarchy);
                            form = SetLists(form);
                            TempData[TempDataKeys.Width] = form.Width + 5;
                            SurveyModel SurveyModel = new SurveyModel();
                            SurveyModel.Form = form;
                            SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(form.SurveyInfo.SurveyId);

                            return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = RootResponseId, PageNumber = 1 });
                        }
                        else if (!string.IsNullOrEmpty(this.Request.Form["Go_One_Level_Up_action"]) && this.Request.Form["Go_One_Level_Up_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            isSaved = true;

                            string parentResponseId = "";
                        	form = SaveCurrentForm(form, surveyInfoModel, surveyAnswer, responseId, orgId, userId, userName, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, pageNumber, FormsHierarchy);
                            form = SetLists(form);
                            TempData[TempDataKeys.Width] = form.Width + 5;
                            SurveyModel surveyModel = new SurveyModel();
                            surveyModel.Form = form;
                            surveyModel.RelateModel = FormsHierarchy.ToRelateModel(form.SurveyInfo.SurveyId);

                            var CurentRecordParent = FormsHierarchy.Single(x => x.FormId == surveyInfoModel.SurveyId);
                            foreach (var item in CurentRecordParent.ResponseIds)
                            {
                                if (item.ResponseId == responseId && !string.IsNullOrEmpty(item.ParentResponseId))
                                {

                                    parentResponseId = item.ParentResponseId;
                                    break;
                                }
                            }

                            Dictionary<string, int> SurveyPagesList = GetSessionValue<Dictionary<string, int>>(UserSession.Key.RelateButtonPageId);
                            if (SurveyPagesList != null)
                            {
                                pageNumber = SurveyPagesList[parentResponseId];
                            }
                            if (!string.IsNullOrEmpty(parentResponseId))
                            {
                                return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = parentResponseId, PageNumber = pageNumber });
                            }
                            else
                            {
                                return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = RootResponseId, PageNumber = pageNumber });
                            }
                        }
                        else if (!string.IsNullOrEmpty(this.Request.Form["Get_Child_action"]) && this.Request.Form["Get_Child_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            int RequestedViewId;

                            SetRelateSession(responseId, pageNumber);
                            RequestedViewId = int.Parse(this.Request.Form["Requested_View_Id"]);
                            surveyAnswer.RootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId);
                            form = SaveCurrentForm(form, surveyInfoModel, surveyAnswer, responseId, orgId, userId, userName, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, pageNumber, FormsHierarchy);

                            // After this point we are dealing with child of child
                            form = SetLists(form);
                            TempData[TempDataKeys.Width] = form.Width + 5;
                            SetSessionValue(UserSession.Key.RequestedViewId, RequestedViewId);
                            SurveyModel SurveyModel = new SurveyModel();
                            SurveyModel.Form = form;
                            SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(form.SurveyInfo.SurveyId);
                            SurveyModel.RequestedViewId = RequestedViewId;
                            int.TryParse(this.Request.Form["Requested_View_Id"].ToString(), out RequestedViewId);
                            var RelateSurveyId = FormsHierarchy.Single(x => x.ViewId == RequestedViewId);

                            int ViewId = int.Parse(Requested_View_Id);

                            string ChildResponseId = AddNewChild(RelateSurveyId.FormId, ViewId, responseId, formValuesHasChanged, "1");
                            return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = ChildResponseId, PageNumber = 1 });
                        }
                        //Read_Response_action
                        else if (!string.IsNullOrEmpty(this.Request.Form["Read_Response_action"]) && this.Request.Form["Read_Response_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            SetRelateSession(responseId, pageNumber);

                            this.UpdateStatus(surveyAnswerModel.ResponseId, surveyAnswerModel.SurveyId, RecordStatus.Saved, RecordStatusChangeReason.ReadResponse);

                            int RequestedViewId = int.Parse(this.Request.Form["Requested_View_Id"]);

                            return RedirectToRoute(new { Controller = "FormResponse", Action = "Index", formid = form.SurveyInfo.SurveyId, ViewId = RequestedViewId, responseid = responseId, Pagenumber = 1 });
                        }
                        else if (!string.IsNullOrEmpty(this.Request.Form[RequestAction.DontSave]) && this.Request.Form[RequestAction.DontSave].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {

                            this.IsEditMode = GetBoolSessionValue(UserSession.Key.IsEditMode);
                            SurveyAnswerRequest SARequest = new SurveyAnswerRequest();
                            SARequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = GetStringSessionValue(UserSession.Key.RootResponseId) });
                            SARequest.Criteria.UserId = GetIntSessionValue(UserSession.Key.UserId);
                            SARequest.Criteria.IsEditMode = this.IsEditMode;
                            SARequest.Criteria.IsSqlProject = GetBoolSessionValue(UserSession.Key.IsSqlProject);
                            SARequest.Criteria.IsDeleteMode = true;
                            SARequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteInEditMode;
                            SARequest.Action = RequestAction.DontSave;

                            SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);
                            return RedirectToRoute(new { Controller = "FormResponse", Action = "Index", formid = GetStringSessionValue(UserSession.Key.RootFormId), ViewId = 0, pageNumber });
                        }
                        else if (!string.IsNullOrEmpty(this.Request.Form["is_goto_action"]) && this.Request.Form["is_goto_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            //This is a Navigation to a url
                            var requestedViewId = Convert.ToInt32(Requested_View_Id);
                            var formId = MetadataAccessor.GetFormIdByViewId(requestedViewId);
                            var responseContext = InitializeResponseContext(formId: formId, rootResponseId: rootResponseId, responseId: responseId) as ResponseContext;

                            form = SetLists(form);

                            _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswer, isSubmited, isSaved, pageNumber, orgId, userId, userName);
                            responseContext.FormId = surveyInfoModel.SurveyId;
                            var surveyAnswerRequest = new SurveyAnswerRequest { ResponseContext = responseContext };

                            surveyAnswer = _surveyFacade.GetSurveyAnswerDTO(surveyAnswerRequest);
                            form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, pageNumber, surveyAnswer, isMobileDevice, null, FormsHierarchy, isAndroid);
                            form.FormValuesHasChanged = formValuesHasChanged;
                            TempData[TempDataKeys.Width] = form.Width + 5;
                            SurveyModel SurveyModel = new SurveyModel();
                            SurveyModel.Form = form;
                            SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(form.SurveyInfo.SurveyId);

                            return View(ViewActions.Index, SurveyModel);
                        }

                        else if (form.Validate(form.RequiredFieldsList))
                        {
                            if (!string.IsNullOrEmpty(Submitbutton) || !string.IsNullOrEmpty(CloseButton) || (!string.IsNullOrEmpty(this.Request.Form["is_save_action_Mobile"]) && this.Request.Form["is_save_action_Mobile"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase)))
                            {

                            	KeyValuePair<string, int> ValidateValues = ValidateAll(form, orgId, userId, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, userName);

                                if (!string.IsNullOrEmpty(formValuesHasChanged))
                                {
                                    if (!string.IsNullOrEmpty(ValidateValues.Key) && !string.IsNullOrEmpty(ValidateValues.Value.ToString()))
                                    {
                                        return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = ValidateValues.Key, PageNumber = ValidateValues.Value.ToString() });
                                    }
                                }

                                UpdateStatus(form.ResponseId, form.SurveyInfo.SurveyId, RecordStatus.Saved, RecordStatusChangeReason.SubmitOrClose);

                                if (!string.IsNullOrEmpty(CloseButton))
                                {
                                    if (!Log_Out)
                                    {
                                        return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = this.RootFormId, orgid = GetIntSessionValue(UserSession.Key.SelectedOrgId) });
                                    }
                                    else
                                    {
                                        return RedirectToAction(ViewActions.Index, ControllerNames.Post);
                                    }
                                }
                                else
                                {
                                    if (!isMobileDevice)
                                    {
                                        if (string.IsNullOrEmpty(this.RootFormId))
                                        {
                                            return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = surveyInfoModel.SurveyId, orgid = GetIntSessionValue(UserSession.Key.SelectedOrgId) });
                                        }
                                        else
                                        {
                                            return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = this.RootFormId, orgid = GetIntSessionValue(UserSession.Key.SelectedOrgId) });
                                        }
                                    }
                                    else
                                    {
                                        return RedirectToAction(ViewActions.Index, ControllerNames.FormResponse, new { formid = this.RootFormId, Pagenumber = GetIntSessionValue(UserSession.Key.PageNumber) });
                                    }
                                }
                            }
                            else
                            {
                                //This is a Navigation to a url

                                //////////////////////Update Survey Mode//////////////////////////
                                form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, pageNumber, surveyAnswer, isMobileDevice, null, FormsHierarchy, isAndroid);
                                form.FormValuesHasChanged = formValuesHasChanged;
                                TempData[TempDataKeys.Width] = form.Width + 5;

                                if (isMobileDevice)
                                {
                                    form = SetFormPassCode(form, responseId);
                                }

                                form.StatusId = surveyAnswer.Status;
                                SurveyModel SurveyModel = new SurveyModel();
                                SurveyModel.Form = form;


                                SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(form.SurveyInfo.SurveyId);
                                if (!string.IsNullOrEmpty(this.Request.Form["Click_Related_Form"]))
                                {
                                    IsEditMode = GetBoolSessionValue(UserSession.Key.IsEditMode);
                                    if (IsEditMode)
                                    {
                                        ViewBag.Edit = "Edit";
                                    }

                                    SurveyModel.RelatedButtonWasClicked = this.Request.Form["Click_Related_Form"].ToString();

                                    return View(ViewActions.Index, SurveyModel);
                                }
                                else
                                {
                                    return RedirectToAction(ViewActions.Index, ControllerNames.Survey, new { RequestId = form.ResponseId, PageNumber = form.CurrentPage });
                                }
                            }
                        }
                        else
                        {
                            //Invalid Data - stay on same page
                            int CurrentPageNum = surveyAnswer.CurrentPageNumber;
                            if (isMobileDevice)
                            {
                                CurrentPageNum--;
                            }

                            if (CurrentPageNum != pageNumber) // failed validation and navigating to different page// must keep url the same 
                            {
                                TempData[TempDataKeys.IsRedirect] = "true";
                                TempData[TempDataKeys.Width] = form.Width + 5;
                                return RedirectToAction(ViewActions.Index, ControllerNames.Survey, new { RequestId = form.ResponseId, PageNumber = CurrentPageNum });
                            }
                            else
                            {
                                TempData[TempDataKeys.Width] = form.Width + 5;
                                SurveyModel SurveyModel = new SurveyModel();
                                SurveyModel.Form = form;
                                SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(form.SurveyInfo.SurveyId);

                                return View(ViewActions.Index, SurveyModel);
                            }
                        }
                }
            }

            catch (Exception ex)
            {
                Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);
                ExceptionModel ExModel = new ExceptionModel();
                ExModel.ExceptionDetail = "Stack Trace : " + ex.StackTrace;
                ExModel.Message = ex.Message;

                return View(ViewActions.Exception, ExModel);
            }
        }

        private List<FormsHierarchyDTO> GetFormsHierarchy()
        {
            FormsHierarchyResponse formsHierarchyResponse = new FormsHierarchyResponse();
            FormsHierarchyRequest formsHierarchyRequest = new FormsHierarchyRequest();
            ResponseContext responseContext = GetSessionValue<ResponseContext>(UserSession.Key.ResponseContext);
            var rootFormId = GetStringSessionValue(UserSession.Key.RootFormId) ?? responseContext.RootFormId;
            var rootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId) ?? responseContext.RootResponseId;
            var userId = GetIntSessionValue(UserSession.Key.UserId);
            var orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);

            if (rootFormId != null && rootResponseId != null)
            {
                formsHierarchyRequest.SurveyInfo.FormId = rootFormId.ToString();

                formsHierarchyRequest.SurveyResponseInfo.RootFormId = rootFormId.ToString();
                formsHierarchyRequest.SurveyResponseInfo.ResponseId = rootResponseId.ToString();
                formsHierarchyRequest.SurveyResponseInfo.UserId = userId;
                formsHierarchyRequest.SurveyResponseInfo.UserOrgId = orgId;
                formsHierarchyRequest.SurveyResponseInfo.ResolveMetadataDependencies();
                formsHierarchyResponse = _surveyFacade.GetFormsHierarchy(formsHierarchyRequest);
            }

            return formsHierarchyResponse.FormsHierarchy;
        }

        private void SetCurrentPage(SurveyAnswerDTO surveyAnswerDTO, int viewPageNumber)
        {
            //surveyAnswerDTO.ResponseDetail.LastPageVisited = viewPageNumber;

            SurveyAnswerRequest sar = new SurveyAnswerRequest();
            sar.Action = RequestAction.Update;
            sar.Criteria.UserId = GetIntSessionValue(UserSession.Key.UserId);
            sar.SurveyAnswerList.Add(surveyAnswerDTO);
            // TODO: !! Update FormInfo with current UserId and LastPageVisited !!
            //_surveyFacade.SaveSurveyAnswer(sar);
        }

        private enum PreValidationResultEnum
        {
            Success,
            SurveyIsPastClosingDate,
            SurveyIsAlreadyCompleted
        }

        private PreValidationResultEnum PreValidateResponse(SurveyAnswerModel SurveyAnswer)
        {
            PreValidationResultEnum result = PreValidationResultEnum.Success;

            //if (DateTime.Now > SurveyInfo.ClosingDate)
            //    {
            //    return PreValidationResultEnum.SurveyIsPastClosingDate;
            //    }


            if (SurveyAnswer.Status == RecordStatus.Completed)
            {
                return PreValidationResultEnum.SurveyIsAlreadyCompleted;
            }

            return result;
        }

        private int GetSurveyPageNumber(FormResponseDetail responseDetail)
        {
            int pageNumber = responseDetail != null ? (responseDetail.LastPageVisited != 0 ? responseDetail.LastPageVisited : 1) : 1;
            return pageNumber;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateResponse(string NameList, string Value, string responseId)
        {
            bool IsAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                IsAndroid = true;
            }
            var rootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId);
            var rootFormId = GetStringSessionValue(UserSession.Key.RootFormId);
            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);
            try
            {
                if (!string.IsNullOrEmpty(NameList))
                {
                    string[] nameList = null;


                    nameList = NameList.Split(',');

                    bool IsMobileDevice = false;

                    IsMobileDevice = this.Request.Browser.IsMobileDevice;

                    ResponseContext responseContext = InitializeResponseContext(responseId: responseId) as ResponseContext;


                    var surveyAnswerRequest = new SurveyAnswerRequest { ResponseContext = responseContext };


                    SurveyAnswerDTO SurveyAnswer = _surveyFacade.GetSurveyAnswerDTO(surveyAnswerRequest);

                    var surveyId = SurveyAnswer.SurveyId;
                    SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyId);

                    int numberOfPages = surveyInfoModel.GetFormDigest(surveyId).NumberOfPages;

                    foreach (string Name in nameList)
                    {
                        for (int i = numberOfPages; i > 0; i--)
                        {
                            responseContext.ResponseId = SurveyAnswer.SurveyId;
                            responseContext.FormId = surveyId;
                            responseContext = (ResponseContext)responseContext.ResolveMetadataDependencies();

                            SurveyAnswer = _surveyFacade.GetSurveyAnswerDTO(surveyAnswerRequest);

                            MvcDynamicForms.Form formRs = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, i, SurveyAnswer, IsMobileDevice, null, null, IsAndroid);

                            formRs = Epi.Cloud.MVC.Utility.SurveyHelper.UpdateControlsValues(formRs, Name, Value);

                            _surveyFacade.UpdateSurveyResponse(surveyInfoModel, SurveyAnswer.ResponseId, formRs, SurveyAnswer, false, false, i, orgId, userId, userName);
                        }
                    }
                    return Json(true);
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveSurvey(string Key, int Value, string responseId)
        {
            ResponseContext responseContext = GetSessionValue<ResponseContext>(UserSession.Key.ResponseContext);

            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);
            try
            {
                bool IsMobileDevice = false;
                int PageNumber = Value;
                IsMobileDevice = this.Request.Browser.IsMobileDevice;


                SurveyAnswerDTO SurveyAnswer = GetSurveyAnswer(responseId);
                bool IsAndroid = false;

                if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    IsAndroid = true;
                }


                //SurveyInfoModel surveyInfoModel = _isurveyFacade.GetSurveyInfoModel(SurveyAnswer.SurveyId);
                SurveyInfoModel surveyInfoModel = GetSurveyInfo(SurveyAnswer.SurveyId);
                PreValidationResultEnum ValidationTest = PreValidateResponse(SurveyAnswer.ToSurveyAnswerModel());
                var form = _surveyFacade.GetSurveyFormData(SurveyAnswer.SurveyId, PageNumber, SurveyAnswer, IsMobileDevice, null, null, IsAndroid);

                form.StatusId = SurveyAnswer.Status;
                var IsSaved = false;
                form.IsSaved = true;
                SurveyAnswer = GetSurveyAnswer(responseId, SurveyAnswer.SurveyId);

                var pageNumber = GetSurveyPageNumber(SurveyAnswer.ResponseDetail);
                form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, pageNumber == 0 ? 1 : pageNumber, SurveyAnswer, IsMobileDevice, null, null, IsAndroid);
                //Update the model
                UpdateModel(form);
                //Save the child form
                _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, SurveyAnswer, false, IsSaved, PageNumber, orgId, userId, userName);
                //  SetCurrentPage(SurveyAnswer, PageNumber);
                //Save the parent form 
                IsSaved = form.IsSaved = true;
                _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, SurveyAnswer, false, IsSaved, PageNumber, orgId, userId, userName);
                return Json(true);

            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        public SurveyInfoModel GetSurveyInfo(string SurveyId, List<FormsHierarchyDTO> FormsHierarchyDTOList = null)
        {
            SurveyInfoModel surveyInfoModel = new SurveyInfoModel();
            if (FormsHierarchyDTOList != null)
            {
                var surveyInfoDTO = FormsHierarchyDTOList.FirstOrDefault(x => x.FormId == SurveyId).SurveyInfo;
                surveyInfoModel = surveyInfoDTO.ToSurveyInfoModel();
            }
            else
            {
                surveyInfoModel = _surveyFacade.GetSurveyInfoModel(SurveyId);
            }
            return surveyInfoModel;

        }

        public MvcDynamicForms.Form SetLists(MvcDynamicForms.Form form)
        {
            form.HiddenFieldsList = this.Request.Form["HiddenFieldsList"].ToString();

            form.HighlightedFieldsList = this.Request.Form["HighlightedFieldsList"].ToString();

            form.DisabledFieldsList = this.Request.Form["DisabledFieldsList"].ToString();

            form.RequiredFieldsList = this.Request.Form["RequiredFieldsList"].ToString();

            form.AssignList = this.Request.Form["AssignList"].ToString();

            return form;
        }

        [HttpPost]
        public ActionResult Delete(string responseId)//List<FormInfoModel> ModelList, string formid)
        {
            var rootFormId = GetStringSessionValue(UserSession.Key.RootFormId);
            var formId = GetStringSessionValue(UserSession.Key.CurrentFormId);

            var responseContext = InitializeResponseContext(responseId: responseId);

            IsEditMode = GetBoolSessionValue(UserSession.Key.IsEditMode);

            SurveyAnswerRequest SARequest = new SurveyAnswerRequest { ResponseContext = responseContext };
            var surveyAnswerDTO = new SurveyAnswerDTO() { ResponseDetail = responseContext.ToFormResponseDetail() };
            SARequest.SurveyAnswerList.Add(surveyAnswerDTO);
            SARequest.Criteria.UserId = GetIntSessionValue(UserSession.Key.UserId);
            SARequest.Criteria.IsEditMode = this.IsEditMode;
            SARequest.Criteria.IsDeleteMode = true;
            SARequest.Criteria.IsSqlProject = GetBoolSessionValue(UserSession.Key.IsSqlProject);
            SARequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteInEditMode;
            SARequest.Action = RequestAction.DontSave;
            SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);

            return Json(GetStringSessionValue(UserSession.Key.RootFormId));
        }

        [HttpPost]
        public ActionResult DeleteBranch(string responseId)//List<FormInfoModel> ModelList, string formid)
        {
            SurveyAnswerRequest SARequest = new SurveyAnswerRequest();
            SARequest.SurveyAnswerList.Add(new SurveyAnswerDTO()
            {
                ResponseId = responseId,
                RootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId),
                RootFormId = GetStringSessionValue(UserSession.Key.RootFormId)
            }.ResolveMetadataDependencies() as SurveyAnswerDTO);
            SARequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[UserSession.Key.UserId].ToString());
            SARequest.Criteria.IsEditMode = false;
            SARequest.Criteria.IsDeleteMode = true;
            SARequest.Criteria.IsSqlProject = GetBoolSessionValue(UserSession.Key.IsSqlProject);
            SARequest.Criteria.SurveyId = GetStringSessionValue(UserSession.Key.RootFormId);
            SARequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteResponse;
            SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);

            return Json(GetStringSessionValue(UserSession.Key.RootFormId));
        }

        [HttpGet]
        public ActionResult LogOut()
        {
            this.UpdateStatus(GetStringSessionValue(UserSession.Key.RootResponseId), null, RecordStatus.InProcess, RecordStatusChangeReason.Logout);
            FormsAuthentication.SignOut();
            ClearSession();
            return RedirectToAction(ViewActions.Index, ControllerNames.Login);
        }

        [HttpPost]
        public JsonResult AddChild(string surveyId, int viewId, string responseId, string formValuesHasChanged, string currentPage)
        {
            var parentResponseId = responseId;

            SetSessionValue(UserSession.Key.RequestedViewId, viewId);

            //1-Get the child Id

            SurveyInfoRequest SurveyInfoRequest = new SurveyInfoRequest();
            SurveyInfoResponse SurveyInfoResponse = new SurveyInfoResponse();
            SurveyInfoDTO SurveyInfoDTO = new SurveyInfoDTO();
            SurveyInfoDTO.SurveyId = surveyId;
            SurveyInfoDTO.ViewId = viewId;
            SurveyInfoRequest.SurveyInfoList.Add(SurveyInfoDTO);
            SurveyInfoResponse = _surveyFacade.GetChildFormInfo(SurveyInfoRequest);

            //3-Create a new response for the child 
            //string ChildResponseId = CreateResponse(SurveyInfoResponse.SurveyInfoList[0].SurveyId, ResponseId);
            string childResponseId = AddNewChild(SurveyInfoResponse.SurveyInfoList[0].SurveyId, viewId, parentResponseId, formValuesHasChanged, currentPage);

            return Json(childResponseId);
        }

        private string AddNewChild(string childFormId, int viewId, string parentResponseId, string formValuesHasChanged, string currentPage)
        {
            SetSessionValue(UserSession.Key.RequestedViewId, viewId);
            bool IsMobileDevice = this.Request.Browser.IsMobileDevice;
            if (IsMobileDevice == false)
            {
                IsMobileDevice = Epi.Cloud.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }
            int UserId = GetIntSessionValue(UserSession.Key.UserId);

            string childResponseId = CreateResponse(childFormId, parentResponseId);
            //this.UpdateStatus(responseId, surveyId, RecordStatus.InProcess, RecordStatusChangeReason.NewChild);

            return childResponseId;
        }

        [HttpPost]
        public JsonResult HasResponse(string surveyId, int viewId, string responseId)
        {
            var rootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId);
            var parentResponseId = responseId;
            var childFormId = MetadataAccessor.GetFormIdByViewId(viewId);

            var responseContext = InitializeResponseContext(formId: childFormId, rootResponseId: rootResponseId, parentResponseId: parentResponseId);

            var surveyAnswerRequest = new SurveyAnswerRequest { ResponseContext = responseContext };

            bool hasResponse = _surveyFacade.HasResponse(surveyAnswerRequest);

            return Json(hasResponse);
        }

        public string CreateResponse(string childFormId, string parentResponseId)
        {
            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);
            IsEditMode = GetBoolSessionValue(UserSession.Key.IsEditMode);

            bool IsAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                IsAndroid = true;
            }
            bool IsMobileDevice = this.Request.Browser.IsMobileDevice;


            if (IsMobileDevice == false)
            {
                IsMobileDevice = Epi.Cloud.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }
            //create the responseid
            Guid childResponseId = Guid.NewGuid();
            TempData[TempDataKeys.ResponseId] = childResponseId.ToString();
            var rootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId);

            // create the first survey response
            var responseContext = InitializeResponseContext(formId: childFormId, rootResponseId: rootResponseId, parentResponseId: parentResponseId, responseId: childResponseId.ToString(), isNewRecord: !IsEditMode);
            SurveyAnswerDTO SurveyAnswer = _surveyFacade.CreateSurveyAnswer(responseContext);

            List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();
            SurveyInfoModel surveyInfoModel = GetSurveyInfo(SurveyAnswer.SurveyId, FormsHierarchy);

            // set the survey answer to be production or test 
            SurveyAnswer.IsDraftMode = surveyInfoModel.IsDraftMode;

            MvcDynamicForms.Form form = _surveyFacade.GetSurveyFormData(SurveyAnswer.SurveyId, 1, SurveyAnswer, IsMobileDevice, null, null, IsAndroid);

            TempData[TempDataKeys.Width] = form.Width + 100;

            string checkcode = MetadataAccessor.GetFormDigest(childFormId).CheckCode;
            form.FormCheckCodeObj = form.GetCheckCodeObj(MetadataAccessor.GetFieldDigests(childFormId), SurveyAnswer.ResponseDetail, checkcode);

            ///////////////////////////// Execute - Record Before - start//////////////////////
            Dictionary<string, string> ContextDetailList = new Dictionary<string, string>();
            EnterRule FunctionObject_B = (EnterRule)form.FormCheckCodeObj.GetCommand("level=record&event=before&identifier=");
            SurveyResponseBuilder surveyResponseBuilder = new SurveyResponseBuilder(_requiredList);
            if (FunctionObject_B != null && !FunctionObject_B.IsNull())
            {
                try
                {
                    PageDigest[] pageDigests = form.MetadataAccessor.GetCurrentFormPageDigests();
                    var responseDetail = SurveyAnswer.ResponseDetail;

                    responseDetail = surveyResponseBuilder.CreateResponseDocument(responseContext, pageDigests);

                    SetSessionValue(UserSession.Key.RequiredList, surveyResponseBuilder.RequiredList);
                    this._requiredList = surveyResponseBuilder.RequiredList;
                    form.RequiredFieldsList = this._requiredList;
                    FunctionObject_B.Context.HiddenFieldList = form.HiddenFieldsList;
                    FunctionObject_B.Context.HighlightedFieldList = form.HighlightedFieldsList;
                    FunctionObject_B.Context.DisabledFieldList = form.DisabledFieldsList;
                    FunctionObject_B.Context.RequiredFieldList = form.RequiredFieldsList;

                    FunctionObject_B.Execute();

                    // field list
                    form.HiddenFieldsList = FunctionObject_B.Context.HiddenFieldList;
                    form.HighlightedFieldsList = FunctionObject_B.Context.HighlightedFieldList;
                    form.DisabledFieldsList = FunctionObject_B.Context.DisabledFieldList;
                    form.RequiredFieldsList = FunctionObject_B.Context.RequiredFieldList;


                    ContextDetailList = Epi.Cloud.MVC.Utility.SurveyHelper.GetContextDetailList(FunctionObject_B);
                    form = Epi.Cloud.MVC.Utility.SurveyHelper.UpdateControlsValuesFromContext(form, ContextDetailList);

                    _surveyFacade.UpdateSurveyResponse(surveyInfoModel, childResponseId.ToString(), form, SurveyAnswer, false, false, 0, orgId, userId, userName);
                }
                catch (Exception ex)
                {
                    // do nothing so that processing
                    // can continue
                }
            }
            else
            {
                PageDigest[] pageDigestArray = form.MetadataAccessor.GetCurrentFormPageDigests();

                SurveyAnswer.ResponseDetail = surveyResponseBuilder.CreateResponseDocument(responseContext, pageDigestArray);

                this._requiredList = surveyResponseBuilder.RequiredList;
                SetSessionValue(UserSession.Key.RequiredList, surveyResponseBuilder.RequiredList);
                form.RequiredFieldsList = _requiredList;
                _surveyFacade.UpdateSurveyResponse(surveyInfoModel, SurveyAnswer.ResponseId, form, SurveyAnswer, false, false, 0, orgId, userId, userName);
            }

            var surveyAnswerDTO = GetSurveyAnswer(SurveyAnswer.ResponseId, SurveyAnswer.SurveyId);
            if (surveyAnswerDTO != null)
            {
                SurveyAnswer = surveyAnswerDTO;
            }

            return childResponseId.ToString();
        }

        private MvcDynamicForms.Form SetFormPassCode(MvcDynamicForms.Form form, string responseId)
        {
            Epi.Cloud.Common.Message.UserAuthenticationResponse AuthenticationResponse = _securityFacade.GetAuthenticationResponse(responseId);

            string strPassCode = Epi.Cloud.MVC.Utility.SurveyHelper.GetPassCode();
            if (string.IsNullOrEmpty(AuthenticationResponse.PassCode))
            {
                _securityFacade.UpdatePassCode(responseId, strPassCode);
            }
            if (AuthenticationResponse.PassCode == null)
            {
                form.PassCode = strPassCode;

            }
            else
            {
                form.PassCode = AuthenticationResponse.PassCode;
            }

            return form;
        }

        private MvcDynamicForms.Form UpdateSurveyModel(SurveyInfoModel surveyInfoModel, bool IsMobileDevice, string formValuesHasChanged, SurveyAnswerDTO surveyAnswer, bool isSaveAndClose = false, List<FormsHierarchyDTO> formsHierarchy = null)
        {
            var surveyId = surveyInfoModel.SurveyId;

            MvcDynamicForms.Form form = new MvcDynamicForms.Form();
            int currentPageNumber = surveyAnswer.CurrentPageNumber;

            bool isAndroid = this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0;

            string url = "";
            if (this.Request.UrlReferrer == null)
            {
                url = this.Request.Url.ToString();
            }
            else
            {
                url = this.Request.UrlReferrer.ToString();
            }
            int lastIndex = url.LastIndexOf("/");
            string stringNumber = null;
            if (url.Length - lastIndex + 1 <= url.Length)
            {
                stringNumber = url.Substring(lastIndex, url.Length - lastIndex);
                stringNumber = stringNumber.Trim('/');
                if (stringNumber.Contains('?'))
                {
                    int Index = stringNumber.IndexOf('?');
                    stringNumber = stringNumber.Remove(Index);
                }
            }
            if (isSaveAndClose)
            {
                stringNumber = "1";
            }
            if (int.TryParse(stringNumber, out referrerPageNum))
            {
                var formProvider = IsMobileDevice ? new MobileFormProvider(surveyId) : new FormProvider(surveyId);

                if (referrerPageNum != currentPageNumber)
                {
                    form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, referrerPageNum, surveyAnswer, IsMobileDevice, null, formsHierarchy, isAndroid);
                    form.FormValuesHasChanged = formValuesHasChanged;
                    formProvider.UpdateHiddenFields(referrerPageNum, form, this.ControllerContext.RequestContext.HttpContext.Request.Form);
                }
                else
                {
                    form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, currentPageNumber, surveyAnswer, IsMobileDevice, null, formsHierarchy, isAndroid);
                    form.FormValuesHasChanged = formValuesHasChanged;
                    formProvider.UpdateHiddenFields(currentPageNumber, form, this.ControllerContext.RequestContext.HttpContext.Request.Form);
                }

                if (!isSaveAndClose)
                {
                    UpdateModel(form);
                }
            }
            else
            {
                //get the survey form
                form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, GetSurveyPageNumber(surveyAnswer.ResponseDetail), surveyAnswer, IsMobileDevice, null, formsHierarchy, isAndroid);
                form.FormValuesHasChanged = formValuesHasChanged;
                form.ClearAllErrors();
                if (referrerPageNum == 0)
                {
                    int index = 1;
                    if (stringNumber.Contains("?RequestId="))
                    {
                        index = stringNumber.IndexOf("?");
                    }

                    referrerPageNum = int.Parse(stringNumber.Substring(0, index));

                }
                if (referrerPageNum == currentPageNumber)
                {
                    UpdateModel(form);
                }
                UpdateModel(form);
            }
            return form;
        }

        private KeyValuePair<string, int> ValidateAll(MvcDynamicForms.Form form, int orgId, int userId, bool isSubmited, bool isSaved, bool isMobileDevice, string formValuesHasChanged,string userName)
        {
            List<FormsHierarchyDTO> formsHierarchy = GetFormsHierarchy();
            KeyValuePair<string, int> result = new KeyValuePair<string, int>();
            // foreach (var FormObj in FormsHierarchy)
            for (int j = formsHierarchy.Count() - 1; j >= 0; --j)
            {
                foreach (var surveyAnswerDTO in formsHierarchy[j].ResponseIds)
                {
                    var surveyId = surveyAnswerDTO.SurveyId;
                    var responseId = surveyAnswerDTO.ResponseId;
                    SurveyAnswerDTO surveyAnswer = GetSurveyAnswer(responseId, surveyAnswerDTO.SurveyId);

                    SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswer.SurveyId, formsHierarchy);
                    surveyAnswer.IsDraftMode = surveyInfoModel.IsDraftMode;
                    form = UpdateSurveyModel(surveyInfoModel, isMobileDevice, formValuesHasChanged, surveyAnswer, true, formsHierarchy);
                    var formProvider = new FormProvider(surveyId);
                    for (int i = 1; i < form.NumberOfPages + 1; i++)
                    {
                        form = formProvider.GetForm(form.SurveyInfo, i, surveyAnswer);
                        if (!form.Validate(form.RequiredFieldsList))
                        {
                            TempData[TempDataKeys.IsRedirect] = "true";
                            TempData[TempDataKeys.Width] = form.Width + 5;
                            _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswer, isSubmited, isSaved, i, orgId, userId, userName);

                            result = new KeyValuePair<string, int>(responseId, i);
                            return result;
                        }
                    }
                }
            }

            return result;
        }

        private MvcDynamicForms.Form SaveCurrentForm(MvcDynamicForms.Form form, SurveyInfoModel surveyInfoModel, SurveyAnswerDTO surveyAnswerDTO, string responseId, int orgId, int userId, string userName, bool isSubmited, bool isSaved,
            bool isMobileDevice, string formValuesHasChanged, int pageNumber, List<FormsHierarchyDTO> formsHierarchyDTOList = null
        )
        {
            surveyAnswerDTO = formsHierarchyDTOList.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == responseId);
            bool IsAndroid = this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0;
            surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;

            var lastPageNumber = GetSurveyPageNumber(surveyAnswerDTO.ResponseDetail);
            form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, lastPageNumber == 0 ? 1 : lastPageNumber, surveyAnswerDTO, isMobileDevice, null, formsHierarchyDTOList, IsAndroid);
            form.FormValuesHasChanged = formValuesHasChanged;

            UpdateModel(form);

            form.IsSaved = true;
            form.StatusId = surveyAnswerDTO.Status;

            // Pass Code Logic  start 
            form = SetFormPassCode(form, responseId);
            // Pass Code Logic  end

            _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswerDTO, isSubmited, isSaved, pageNumber, orgId, userId,userName);

            return form;
        }

        private void SetGlobalVariable()
        {
            RootFormId = GetStringSessionValue(UserSession.Key.RootFormId, defaultValue: string.Empty);
            RootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId, defaultValue: string.Empty);
            _requiredList = GetStringSessionValue(UserSession.Key.RequiredList, defaultValue: string.Empty);
            IsEditMode = GetBoolSessionValue(UserSession.Key.IsEditMode);
        }

        private FormResponseInfoModel GetFormResponseInfoModel(string SurveyId, string ResponseId, List<FormsHierarchyDTO> FormsHierarchyDTOList = null)
        {
            int UserId = GetIntSessionValue(UserSession.Key.UserId);
            FormResponseInfoModel FormResponseInfoModel = new FormResponseInfoModel();

            var formHieratchyDTO = FormsHierarchyDTOList.FirstOrDefault(h => h.FormId == SurveyId);

            SurveyResponseBuilder surveyResponseBuilder = new SurveyResponseBuilder();
            if (!string.IsNullOrEmpty(SurveyId))
            {
                //SurveyAnswerRequest FormResponseReq = new SurveyAnswerRequest();
                FormSettingRequest FormSettingReq = new FormSettingRequest { ProjectId = GetStringSessionValue(UserSession.Key.ProjectId) };

                //Populating the request

                FormSettingReq.FormInfo.FormId = SurveyId;
                FormSettingReq.FormInfo.UserId = UserId;
                //Getting Column Name  List
                FormSettingResponse FormSettingResponse = _surveyFacade.GetFormSettings(FormSettingReq);
                _columns = FormSettingResponse.FormSetting.ColumnNameList.ToList();
                _columns.Sort(Compare);

                // Setting  Column Name  List
                FormResponseInfoModel.Columns = _columns;

                //Getting Resposes
                var ResponseListDTO = FormsHierarchyDTOList.FirstOrDefault(x => x.FormId == SurveyId).ResponseIds;

                // If we don't have any data for this child form yet then create a response 
                if (ResponseListDTO.Count == 0)
                {
                    var surveyAnswerDTO = new SurveyAnswerDTO();
                    surveyAnswerDTO.FormId = SurveyId;
                    surveyAnswerDTO.CurrentPageNumber = 1;
                    surveyAnswerDTO.DateUpdated = DateTime.UtcNow;
                    surveyAnswerDTO.RootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId);
                    surveyAnswerDTO.ParentResponseId = ResponseId;
                    surveyAnswerDTO.ResponseId = Guid.NewGuid().ToString();
                    var responseContext = surveyAnswerDTO.ToResponseContext();
                    ResponseListDTO.Add(surveyAnswerDTO);
                }

                //Setting Resposes List
                List<ResponseModel> ResponseList = new List<ResponseModel>();
                foreach (var item in ResponseListDTO)
                {
                    if (item.ParentResponseId == ResponseId)
                    {
                        if (item.SqlData != null)
                        {
                            ResponseList.Add(ConvertRowToModel(item, _columns, "ChildGlobalRecordID"));
                        }
                        else
                        {
                            ResponseList.Add(item.ToResponseModel(_columns));
                        }
                    }
                }

                FormResponseInfoModel.ResponsesList = ResponseList;

                FormResponseInfoModel.PageSize = ReadPageSize();

                FormResponseInfoModel.CurrentPage = 1;
            }
            return FormResponseInfoModel;
        }

        private int ReadPageSize()
        {
            return AppSettings.GetIntValue(AppSettings.Key.ResponsePageSize);
        }

        [HttpPost]
        public ActionResult ReadResponseInfo(string SurveyId, int ViewId, string ResponseId, string CurrentPage)
        {
            int UserId = GetIntSessionValue(UserSession.Key.UserId);
            int PageNumber = int.Parse(CurrentPage);
            bool IsAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                IsAndroid = true;
            }
            SetSessionValue(UserSession.Key.CurrentFormId, SurveyId);

            SetRelateSession(ResponseId, PageNumber);
            bool IsMobileDevice = this.Request.Browser.IsMobileDevice;
            if (IsMobileDevice == false)
            {
                List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();

                int RequestedViewId;
                RequestedViewId = ViewId;
                SetSessionValue(UserSession.Key.RequestedViewId, RequestedViewId);

                SurveyModel SurveyModel = new SurveyModel();
                SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(SurveyId);
                SurveyModel.RequestedViewId = RequestedViewId;

                var RelateSurveyId = FormsHierarchy.Single(x => x.ViewId == ViewId);

                SurveyAnswerRequest FormResponseReq = new SurveyAnswerRequest();


                SurveyModel.FormResponseInfoModel = GetFormResponseInfoModel(RelateSurveyId.FormId, ResponseId, FormsHierarchy);
                SurveyModel.FormResponseInfoModel.NumberOfResponses = SurveyModel.FormResponseInfoModel.ResponsesList.Count();

                SurveyAnswerDTO surveyAnswerDTO = new SurveyAnswerDTO();
                surveyAnswerDTO.SurveyId = RelateSurveyId.SurveyInfo.SurveyId;
                if (RelateSurveyId.ResponseIds.Count > 0)
                {
                    surveyAnswerDTO = FormsHierarchy.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == RelateSurveyId.ResponseIds[0].ResponseId);
                    SurveyModel.Form = _surveyFacade.GetSurveyFormData(RelateSurveyId.ResponseIds[0].SurveyId, 1, surveyAnswerDTO, IsMobileDevice, null, FormsHierarchy, IsAndroid);
                }
                else
                {
                    if (SurveyModel.FormResponseInfoModel.NumberOfResponses > 0)
                    {
                        surveyAnswerDTO = GetSurveyAnswer(SurveyModel.FormResponseInfoModel.ResponsesList[0].Column0, RelateSurveyId.FormId);
                    }
                    SurveyModel.Form = _surveyFacade.GetSurveyFormData(surveyAnswerDTO.SurveyId, 1, surveyAnswerDTO, IsMobileDevice, null, FormsHierarchy, IsAndroid);
                }

                return PartialView(ViewActions.ListResponses, SurveyModel);
            }
            else
            {
                return RedirectToAction(ViewActions.Index, ControllerNames.FormResponse, new { SurveyId = SurveyId, ViewId = ViewId, ResponseId = ResponseId, CurrentPage = CurrentPage });
            }
        }

        public void SetRelateSession(string ResponseId, int CurrentPage)
        {
            Dictionary<string, int> List = GetSessionValue<Dictionary<string, int>>(UserSession.Key.RelateButtonPageId);
            if (List == null)
            {
                List = new Dictionary<string, int>();
                List.Add(ResponseId, CurrentPage);
                SetSessionValue(UserSession.Key.RelateButtonPageId, List);
            }
            else
            {
                if (!List.ContainsKey(ResponseId))
                {
                    List.Add(ResponseId, CurrentPage);
                    SetSessionValue(UserSession.Key.RelateButtonPageId, List);
                }
            }
        }
    }
}
