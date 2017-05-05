using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Core.EnterInterpreter;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Common.Interfaces;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Model;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;
using Epi.Common.Core.DataStructures;
using Epi.Cloud.Common.Extensions;

namespace Epi.Web.MVC.Controllers
{
    [Authorize]
    public class SurveyController : BaseSurveyController
    {
        private readonly ISurveyPersistenceFacade _surveyPersistenceFacade;
        private readonly ISecurityFacade _securityFacade;

        private IEnumerable<AbridgedFieldInfo> _pageFields;
        private string _requiredList = "";
        private string RootFormId = "";
        private string RootResponseId = "";
        private bool IsEditMode;
        private List<SurveyAnswerDTO> ListSurveyAnswerDTO = new List<SurveyAnswerDTO>();
        private int referrerPageNum;

        public SurveyController(ISurveyFacade surveyFacade,
                                ISecurityFacade securityFacade,
                                IProjectMetadataProvider projectMetadataProvider,
                                ISurveyPersistenceFacade surveyPersistenceFacade)
        {
            _surveyFacade = surveyFacade;
            _securityFacade = securityFacade;
            _projectMetadataProvider = projectMetadataProvider;
            _surveyPersistenceFacade = surveyPersistenceFacade;
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
                if (Session[SessionKeys.RootResponseId] != null && Session[SessionKeys.RootResponseId].ToString() == responseId)
                {
                    Session[SessionKeys.RelateButtonPageId] = null;
                }
                bool isAndroid = this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0;
                if (Session[SessionKeys.IsEditMode] != null)
                {
                    bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);
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

            string relateSurveyId = "";
            int requestedViewId;

            if (!string.IsNullOrEmpty(surveyId))
            {
                requestedViewId = forms.Where(x => x.FormId == surveyId).Select(x => x.ViewId).FirstOrDefault();
                Session[SessionKeys.IsSqlProject] = true;
                Session[SessionKeys.RequestedViewId] = requestedViewId;
            }
            else
            {
                if (int.TryParse((Session[SessionKeys.RequestedViewId] ?? "0").ToString(), out requestedViewId) && requestedViewId != 0)
                {
                    surveyId = relateSurveyId = forms.SingleOrDefault(x => x.ViewId == requestedViewId).FormId;
                }
            }

            //Update Status
            UpdateStatus(responseId, relateSurveyId, RecordStatus.InProcess, RecordStatusChangeReason.OpenForEdit);

            //Mobile Section
            bool IsMobileDevice = false;
            IsMobileDevice = this.Request.Browser.IsMobileDevice;
            if (IsMobileDevice == false)
            {
                IsMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }

            List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();
            SurveyAnswerDTO surveyAnswerDTO = (SurveyAnswerDTO)FormsHierarchy.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == responseId);

            if (isEditMode)
            {
                if (IsMobileDevice == false)
                {
                    Session[SessionKeys.RootFormId] = FormsHierarchy[0].FormId;
                }
            }

            PreValidationResultEnum ValidationTest;
            ValidationTest = PreValidateResponse(surveyAnswerDTO.ToSurveyAnswerModel());

            string formId = surveyAnswerDTO.ResponseDetail.FormId;
            string formName = MetadataAccessor.GetFormDigest(formId).FormName;
            if (surveyAnswerDTO.ResponseDetail == null) surveyAnswerDTO.ResponseDetail = new FormResponseDetail { FormId = formId, FormName = formName, LastPageVisited = 1 };

            if (viewPageNumber == 0) viewPageNumber = surveyAnswerDTO.ResponseDetail.LastPageVisited;

            switch (ValidationTest)
            {
                case PreValidationResultEnum.Success:
                default:

                    formId = surveyAnswerDTO != null && !string.IsNullOrWhiteSpace(surveyAnswerDTO.SurveyId) ? surveyAnswerDTO.SurveyId : surveyId;
                    var form = _surveyFacade.GetSurveyFormData(surveyId, viewPageNumber, surveyAnswerDTO, IsMobileDevice, null, FormsHierarchy, isAndroid);

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
                    if (IsMobileDevice)
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
                    if (Session[SessionKeys.FormValuesHasChanged] != null)
                    {
                        form.FormValuesHasChanged = Session[SessionKeys.FormValuesHasChanged].ToString();
                    }
                    form.RequiredFieldsList = this._requiredList;
                    //passCode end
                    SurveyModel SurveyModel = new SurveyModel();
                    SurveyModel.Form = form;
                    SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(form.SurveyInfo.SurveyId);
                    return SurveyModel;
            }
        }

        private void UpdateStatus(string responseId, string surveyId, int statusId, RecordStatusChangeReason statusChangeReason)
        {
            ResponseContext responseContext;
            if (string.IsNullOrEmpty(surveyId))
            {
                responseContext = new ResponseContext
                {
                    ResponseId = responseId,
                    FormId = Session[SessionKeys.RootFormId].ToString(),
                    RootResponseId = Session[SessionKeys.RootResponseId].ToString(),
                    UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()),
                    UserName = Session[SessionKeys.UserName].ToString()
                }.ResolveMetadataDependencies() as ResponseContext;
            }
            else
            {
                responseContext = new ResponseContext
                {
                    FormId = surveyId,
                    ResponseId = responseId,
                    RootResponseId = Session[SessionKeys.RootResponseId].ToString(),
                    UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()),
                    UserName = Session[SessionKeys.UserName].ToString()
                }.ResolveMetadataDependencies() as ResponseContext;
            }

            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest { ResponseContext = responseContext };
            //SurveyAnswerRequest.Criteria.SurveyAnswerIdList.Add(responseId);

            surveyAnswerRequest.SurveyAnswerList.Add(responseContext.ToSurveyAnswerDTO());
            surveyAnswerRequest.Criteria.StatusId = statusId;
            surveyAnswerRequest.Criteria.StatusChangeReason = statusChangeReason;

            surveyAnswerRequest.Criteria.UserId = responseContext.UserId;
            surveyAnswerRequest.Criteria.UserName = responseContext.UserName;
            if (!string.IsNullOrEmpty(surveyId))
            {
                surveyAnswerRequest.Criteria.SurveyId = surveyId;
            }

            _surveyFacade.UpdateResponseStatus(surveyAnswerRequest);
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
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            string userName = Session[SessionKeys.UserName].ToString();
            string responseId = surveyAnswerModel.ResponseId;
            string rootResponseId = Session[SessionKeys.RootResponseId].ToString();
            Session[SessionKeys.FormValuesHasChanged] = Form_Has_Changed;

            bool isAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isAndroid = true;
            }
            if (rootResponseId == responseId)
            {
                Session[SessionKeys.RelateButtonPageId] = null;
            }

            bool isMobileDevice = false;
            isMobileDevice = this.Request.Browser.IsMobileDevice;
            if (isMobileDevice == false)
            {
                isMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }

            var requestedViewId = Convert.ToInt32(Requested_View_Id);
            var formId = MetadataAccessor.GetFormIdByViewId(requestedViewId);
            var parentFormId = MetadataAccessor.GetParentFormIdByViewId(requestedViewId);
            var rootFormId = MetadataAccessor.GetRootFormIdByViewId(requestedViewId);
            var responseContext = new ResponseContext
            {
                FormId = formId,
                ParentFormId = parentFormId,
                RootFormId = RootFormId,
                ResponseId = responseId,
                RootResponseId = rootResponseId,
                UserId = userId,
                UserName = userName
            }.ResolveMetadataDependencies() as ResponseContext;

            try
            {
                string formValuesHasChanged = Form_Has_Changed;

                List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();
                SurveyAnswerDTO surveyAnswer = (SurveyAnswerDTO)FormsHierarchy.SelectMany(x => x.ResponseIds).First(z => z.ResponseId == responseId);
                surveyAnswer.RequestedViewId = Requested_View_Id;
                surveyAnswer.CurrentPageNumber = pageNumber != 0 ? pageNumber : 1;

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

                        _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswer, isSubmited, isSaved, pageNumber, userId, userName);

                        if (!string.IsNullOrEmpty(this.Request.Form["is_save_action"]) && this.Request.Form["is_save_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            form = SaveCurrentForm(form, surveyInfoModel, surveyAnswer, responseId, userId, userName, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, pageNumber, FormsHierarchy);
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
                            form = SaveCurrentForm(form, surveyInfoModel, surveyAnswer, responseId, userId, userName, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, pageNumber, FormsHierarchy);
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
                        	form = SaveCurrentForm(form, surveyInfoModel, surveyAnswer, responseId, userId, userName, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, pageNumber, FormsHierarchy);
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

                            Dictionary<string, int> SurveyPagesList = (Dictionary<string, int>)Session[SessionKeys.RelateButtonPageId];
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
                            surveyAnswer.RootResponseId = Session[SessionKeys.RootResponseId].ToString();
                            form = SaveCurrentForm(form, surveyInfoModel, surveyAnswer, responseId, userId, userName, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, pageNumber, FormsHierarchy);

                            // After this point we are dealing with child of child
                            form = SetLists(form);
                            TempData[TempDataKeys.Width] = form.Width + 5;
                            Session[SessionKeys.RequestedViewId] = RequestedViewId;
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


                            bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);

                            SurveyAnswerRequest SARequest = new SurveyAnswerRequest();
                            SARequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = Session[SessionKeys.RootResponseId].ToString() });
                            SARequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
                            SARequest.Criteria.IsEditMode = this.IsEditMode;
                            SARequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
                            SARequest.Criteria.IsDeleteMode = true;
                            SARequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteInEditMode;
                            SARequest.Action = RequestAction.DontSave;

                            SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);
                            return RedirectToRoute(new { Controller = "FormResponse", Action = "Index", formid = Session[SessionKeys.RootFormId].ToString(), ViewId = 0, pageNumber });
                        }
                        else if (!string.IsNullOrEmpty(this.Request.Form["is_goto_action"]) && this.Request.Form["is_goto_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            //This is a Navigation to a url


                            form = SetLists(form);

                            _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswer, isSubmited, isSaved, pageNumber, userId, userName);
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

                            	KeyValuePair<string, int> ValidateValues = ValidateAll(form, userId, isSubmited, isSaved, isMobileDevice, formValuesHasChanged, userName);

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
                                        return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = this.RootFormId, orgid = (int)Session[SessionKeys.SelectedOrgId] });
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
                                            return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = surveyInfoModel.SurveyId, orgid = (int)Session[SessionKeys.SelectedOrgId] });
                                        }
                                        else
                                        {
                                            return RedirectToAction(ViewActions.Index, ControllerNames.Home, new { surveyid = this.RootFormId, orgid = (int)Session[SessionKeys.SelectedOrgId] });
                                        }
                                    }
                                    else
                                    {
                                        return RedirectToAction(ViewActions.Index, ControllerNames.FormResponse, new { formid = this.RootFormId, Pagenumber = Convert.ToInt32(Session[SessionKeys.PageNumber]) });
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
                                    bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);
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

            var rootFormId = Session[SessionKeys.RootFormId];
            var rootResponseId = Session[SessionKeys.RootResponseId];

            if (rootFormId != null && rootResponseId != null)
            {
                formsHierarchyRequest.SurveyInfo.FormId = rootFormId.ToString();

                formsHierarchyRequest.SurveyResponseInfo.RootFormId = rootFormId.ToString();
                formsHierarchyRequest.SurveyResponseInfo.ResponseId = rootResponseId.ToString();
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
            sar.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
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
            var rootResponseId = Session[SessionKeys.RootResponseId].ToString();
            var rootFormId = Session[SessionKeys.RootFormId].ToString();
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            string userName = Session[SessionKeys.UserName].ToString();
            try
            {
                if (!string.IsNullOrEmpty(NameList))
                {
                    string[] nameList = null;


                    nameList = NameList.Split(',');

                    bool IsMobileDevice = false;

                    IsMobileDevice = this.Request.Browser.IsMobileDevice;

                    ResponseContext responseContext = (ResponseContext)new ResponseContext
                    {
                        RootFormId = rootFormId,
                        ResponseId = responseId,
                        RootResponseId = rootResponseId ?? responseId,
                        UserId = userId,
                        UserName = userName
                    }.ResolveMetadataDependencies();

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

                            formRs = Epi.Web.MVC.Utility.SurveyHelper.UpdateControlsValues(formRs, Name, Value);

                            _surveyFacade.UpdateSurveyResponse(surveyInfoModel, SurveyAnswer.ResponseId, formRs, SurveyAnswer, false, false, i, userId, userName);
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
            ResponseContext responseContext = Session[SessionKeys.ResponseContext] as ResponseContext;

            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            string UserName = Session[SessionKeys.UserName].ToString();
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
                _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, SurveyAnswer, false, IsSaved, PageNumber, UserId, UserName);
                //  SetCurrentPage(SurveyAnswer, PageNumber);
                //Save the parent form 
                IsSaved = form.IsSaved = true;
                _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, SurveyAnswer, false, IsSaved, PageNumber, UserId, UserName);
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
        public ActionResult Delete(string responseid)//List<FormInfoModel> ModelList, string formid)
        {
            var responseContext = new ResponseContext
            {
                RootResponseId = Session[SessionKeys.RootResponseId].ToString(),
                ResponseId = responseid,
                FormId = Session[SessionKeys.CurrentFormId].ToString(),
                RootFormId = Session[SessionKeys.RootFormId].ToString(),
                UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()),
                UserName = Session[SessionKeys.UserName].ToString()
            }.ResolveMetadataDependencies() as ResponseContext;

            bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);

            SurveyAnswerRequest SARequest = new SurveyAnswerRequest { ResponseContext = responseContext };
            var surveyAnswerDTO = new SurveyAnswerDTO() { ResponseDetail = responseContext.ToFormResponseDetail() };
            SARequest.SurveyAnswerList.Add(surveyAnswerDTO);
            SARequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            SARequest.Criteria.IsEditMode = this.IsEditMode;
            SARequest.Criteria.IsDeleteMode = true;
            SARequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
            SARequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteInEditMode;
            SARequest.Action = RequestAction.DontSave;
            SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);

            return Json(Session[SessionKeys.RootFormId]);//string.Empty
        }

        [HttpPost]
        public ActionResult DeleteBranch(string ResponseId)//List<FormInfoModel> ModelList, string formid)
        {

            SurveyAnswerRequest SARequest = new SurveyAnswerRequest();
            SARequest.SurveyAnswerList.Add(new SurveyAnswerDTO()
            {
                ResponseId = ResponseId,
                RootResponseId = Session[SessionKeys.RootResponseId].ToString(),
                RootFormId = Session[SessionKeys.RootFormId].ToString()
            }.ResolveMetadataDependencies() as SurveyAnswerDTO);
            SARequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            SARequest.Criteria.IsEditMode = false;
            SARequest.Criteria.IsDeleteMode = true;
            SARequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
            SARequest.Criteria.SurveyId = Session[SessionKeys.RootFormId].ToString();
            SARequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteResponse;
            SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);

            return Json(Session[SessionKeys.RootFormId]);
        }

        [HttpGet]
        public ActionResult LogOut()
        {
            this.UpdateStatus(Session[SessionKeys.RootResponseId].ToString(), null, RecordStatus.InProcess, RecordStatusChangeReason.Logout);
            FormsAuthentication.SignOut();
            this.Session.Clear();
            return RedirectToAction(ViewActions.Index, ControllerNames.Login);
        }

        [HttpPost]
        public JsonResult AddChild(string surveyId, int viewId, string responseId, string formValuesHasChanged, string currentPage)
        {
            var parentResponseId = responseId;

            Session[SessionKeys.RequestedViewId] = viewId;

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
            Session[SessionKeys.RequestedViewId] = viewId;
            bool IsMobileDevice = this.Request.Browser.IsMobileDevice;
            if (IsMobileDevice == false)
            {
                IsMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());

            string childResponseId = CreateResponse(childFormId, parentResponseId);
            //this.UpdateStatus(responseId, surveyId, RecordStatus.InProcess, RecordStatusChangeReason.NewChild);

            return childResponseId;
        }

        [HttpPost]
        public JsonResult HasResponse(string surveyId, int viewId, string responseId)
        {
            var rootResponseId = Session[SessionKeys.RootResponseId].ToString();
            var parentResponseId = responseId;
            var childFormId = MetadataAccessor.GetFormIdByViewId(viewId);

            var responseContext = new ResponseContext
            {
                FormId = childFormId,
                ParentResponseId = parentResponseId,
                RootResponseId = rootResponseId
            }.ResolveMetadataDependencies();

            var surveyAnswerRequest = new SurveyAnswerRequest { ResponseContext = responseContext };

            bool hasResponse = _surveyFacade.HasResponse(surveyAnswerRequest);

            return Json(hasResponse);
        }

        public string CreateResponse(string childFormId, string parentResponseId)
        {
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            string userName = Session[SessionKeys.UserName].ToString();
            bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);

            bool IsAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                IsAndroid = true;
            }
            bool IsMobileDevice = this.Request.Browser.IsMobileDevice;


            if (IsMobileDevice == false)
            {
                IsMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }
            //create the responseid
            Guid childResponseId = Guid.NewGuid();
            TempData[TempDataKeys.ResponseId] = childResponseId.ToString();
            var rootResponseId = Session[SessionKeys.RootResponseId].ToString();

            var responseContext = new ResponseContext
            {
                RootResponseId = rootResponseId,
                ParentResponseId = parentResponseId,
                ResponseId = childResponseId.ToString(),
                FormId = childFormId,
                IsNewRecord = true,
                UserId = userId,
                UserName = userName
            }.ResolveMetadataDependencies() as ResponseContext;

            // create the first survey response
            // SurveyAnswerDTO SurveyAnswer = _isurveyFacade.CreateSurveyAnswer(surveyModel.SurveyId, ResponseID.ToString());
            int currentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());
            SurveyAnswerDTO SurveyAnswer = _surveyFacade.CreateSurveyAnswer(responseContext, this.IsEditMode, currentOrgId);

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
            SurveyResponseBuilder surveyResponseBuilder = new SurveyResponseBuilder(_pageFields, _requiredList);
            if (FunctionObject_B != null && !FunctionObject_B.IsNull())
            {
                try
                {
                    PageDigest[] pageDigests = form.MetadataAccessor.GetCurrentFormPageDigests();
                    var responseDetail = SurveyAnswer.ResponseDetail;

                    responseDetail = surveyResponseBuilder.CreateResponseDocument(responseContext, pageDigests);

                    Session[SessionKeys.RequiredList] = surveyResponseBuilder.RequiredList;
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


                    ContextDetailList = Epi.Web.MVC.Utility.SurveyHelper.GetContextDetailList(FunctionObject_B);
                    form = Epi.Web.MVC.Utility.SurveyHelper.UpdateControlsValuesFromContext(form, ContextDetailList);

                    _surveyFacade.UpdateSurveyResponse(surveyInfoModel, childResponseId.ToString(), form, SurveyAnswer, false, false, 0, SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()), userName);
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
                Session[SessionKeys.RequiredList] = surveyResponseBuilder.RequiredList;
                form.RequiredFieldsList = _requiredList;
                _surveyFacade.UpdateSurveyResponse(surveyInfoModel, SurveyAnswer.ResponseId, form, SurveyAnswer, false, false, 0, SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()), userName);
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

            string strPassCode = Epi.Web.MVC.Utility.SurveyHelper.GetPassCode();
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

        private KeyValuePair<string, int> ValidateAll(MvcDynamicForms.Form form, int userId, bool isSubmited, bool isSaved, bool isMobileDevice, string formValuesHasChanged,string userName)
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
                            _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswer, isSubmited, isSaved, i, userId, userName);

                            result = new KeyValuePair<string, int>(responseId, i);
                            return result;
                        }
                    }
                }
            }

            return result;
        }

        private MvcDynamicForms.Form SaveCurrentForm(MvcDynamicForms.Form form, SurveyInfoModel surveyInfoModel, SurveyAnswerDTO surveyAnswerDTO, string responseId, int userId,string userName, bool isSubmited, bool isSaved,
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

            _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswerDTO, isSubmited, isSaved, pageNumber, userId,userName);

            return form;
        }

        private void SetGlobalVariable()
        {
            this.RootFormId = (Session[SessionKeys.RootFormId] ?? string.Empty).ToString();
            this.RootResponseId = (Session[SessionKeys.RootResponseId] ?? string.Empty).ToString();
            this._requiredList = (Session[SessionKeys.RequiredList] ?? string.Empty).ToString();
            bool.TryParse((Session[SessionKeys.IsEditMode] ?? "False").ToString(), out this.IsEditMode);
        }

        private FormResponseInfoModel GetFormResponseInfoModel(string SurveyId, string ResponseId, List<FormsHierarchyDTO> FormsHierarchyDTOList = null)
        {
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            FormResponseInfoModel FormResponseInfoModel = new FormResponseInfoModel();

            var formHieratchyDTO = FormsHierarchyDTOList.FirstOrDefault(h => h.FormId == SurveyId);

            SurveyResponseBuilder surveyResponseBuilder = new SurveyResponseBuilder();
            if (!string.IsNullOrEmpty(SurveyId))
            {
                //SurveyAnswerRequest FormResponseReq = new SurveyAnswerRequest();
                FormSettingRequest FormSettingReq = new FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };

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
                    surveyAnswerDTO.RootResponseId = Session[SessionKeys.RootResponseId].ToString();
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
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            int PageNumber = int.Parse(CurrentPage);
            bool IsAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                IsAndroid = true;
            }
            Session[SessionKeys.CurrentFormId] = SurveyId;

            SetRelateSession(ResponseId, PageNumber);
            bool IsMobileDevice = this.Request.Browser.IsMobileDevice;
            if (IsMobileDevice == false)
            {
                List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();

                int RequestedViewId;
                RequestedViewId = ViewId;
                Session[SessionKeys.RequestedViewId] = RequestedViewId;

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
            // Session[SessionKeys.RelateButtonPageId] 
            var Obj = Session[SessionKeys.RelateButtonPageId];
            Dictionary<string, int> List = new Dictionary<string, int>();
            if (Obj == null)
            {
                List.Add(ResponseId, CurrentPage);
                Session[SessionKeys.RelateButtonPageId] = List;
            }
            else
            {
                List = (Dictionary<string, int>)Session[SessionKeys.RelateButtonPageId];
                if (!List.ContainsKey(ResponseId))
                {
                    List.Add(ResponseId, CurrentPage);
                    Session[SessionKeys.RelateButtonPageId] = List;
                }
            }
        }
    }
}
