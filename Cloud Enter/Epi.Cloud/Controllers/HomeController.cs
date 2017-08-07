using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Security;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Common.Model;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.MVC.Constants;
using Epi.Cloud.MVC.Extensions;
using Epi.Cloud.MVC.Models;
using Epi.Cloud.MVC.Utility;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Common.Core.DataStructures;
using Epi.Common.EmailServices;
using Epi.Common.EmailServices.Constants;
using Epi.Core.EnterInterpreter;
using Epi.DataPersistence.Constants;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.MVC.Controllers
{
    [Authorize]
    public class HomeController : BaseSurveyController
    {
        private readonly ISecurityFacade _securityFacade;

        private readonly ISurveyResponseDao _surveyResponseDao;

        private IEnumerable<AbridgedFieldInfo> _pageFields;
        private string _requiredList = "";

        /// <summary>
        /// injecting surveyFacade to the constructor 
        /// </summary>
        /// <param name="surveyFacade"></param>
        public HomeController(ISurveyFacade surveyFacade,
                              ISecurityFacade securityFacade,
                              Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider projectMetadataProvider,
                              ISurveyResponseDao surveyResponseDao) : base()
        {
            _surveyFacade = surveyFacade;
            _securityFacade = securityFacade;
            _projectMetadataProvider = projectMetadataProvider;
            _surveyResponseDao = surveyResponseDao;
        }

        public ActionResult Default()
        {
            return View("Default");
        }

        [HttpGet]
        public ActionResult Index(string surveyId, int orgId = -1)
        {

            RemoveSessionValue(UserSession.Key.ResponseContext);
            RemoveSessionValue(UserSession.Key.EditForm);

            int userId = GetIntSessionValue(UserSession.Key.UserId);
            int orgnizationId;

            Guid userIdGuid = new Guid();
            try
            {

                FormModel formModel = GetFormModel(surveyId, userId, userIdGuid, out orgnizationId);
                SetSessionValue(UserSession.Key.SelectedOrgId, orgId == -1 ? orgnizationId : orgId);
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(\r\n|\r|\n)+");



                bool isMobileDevice = false;
                isMobileDevice = this.Request.Browser.IsMobileDevice;
                if (isMobileDevice) // Because mobile doesn't need RootFormId until button click. 
                {
                    RemoveSessionValue(UserSession.Key.RootFormId);
                    RemoveSessionValue(UserSession.Key.PageNumber);
                    RemoveSessionValue(UserSession.Key.SortOrder);
                    RemoveSessionValue(UserSession.Key.SortField);
                    RemoveSessionValue(UserSession.Key.SearchCriteria);
                    RemoveSessionValue(UserSession.Key.SearchModel);
                }

                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                ViewBag.Version = version;

                return View(ViewActions.Index, formModel);
            }
            catch (Exception ex)
            {
                Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);
                ExceptionModel ExModel = new ExceptionModel();
                ExModel.ExceptionDetail = ex.StackTrace;
                ExModel.Message = ex.Message;
                return View(ViewActions.Exception, ExModel);
            }
        }

        private FormModel GetFormModel(string surveyId, int userId, Guid userIdGuid, out int orgnizationId)
        {
            FormModel formModel = new Models.FormModel();
            formModel.UserHighestRole = GetIntSessionValue(UserSession.Key.UserHighestRole);
            // Get OrganizationList
            OrganizationRequest request = new OrganizationRequest();
            request.UserId = userId;
            request.UserRole = formModel.UserHighestRole;
            OrganizationResponse organizations = _securityFacade.GetOrganizationsByUserId(request);

            formModel.OrganizationList = organizations.OrganizationList.ToOrganizationModelList();
            //Get Forms
            orgnizationId = organizations.OrganizationList[0].OrganizationId;
            formModel.FormList = GetFormsInfoList(userIdGuid, orgnizationId);
            // Set user Info

            formModel.UserFirstName = GetStringSessionValue(UserSession.Key.UserFirstName);
            formModel.UserLastName = GetStringSessionValue(UserSession.Key.UserLastName);
            formModel.SelectedForm = surveyId;
            return formModel;
        }

        /// <summary>
        /// redirecting to Survey controller to action method Index
        /// </summary>
        /// <param name="surveyModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string surveyId, string addNewFormId, string editForm)
        {
            bool isNewRecord = false;

            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);
            SetSessionValue(UserSession.Key.FormValuesHasChanged, string.Empty);

            var editFormFromSession = GetStringSessionValue(UserSession.Key.EditForm);
            if (string.IsNullOrEmpty(editForm) && editFormFromSession != null)
            {
                editForm = editFormFromSession;
            }

            var editResponseId = editForm;

            if (!string.IsNullOrEmpty(editForm) && string.IsNullOrEmpty(addNewFormId))
            {
                SetSessionValue(UserSession.Key.RootResponseId, editForm);

                SetSessionValue(UserSession.Key.IsEditMode, true);
                isNewRecord = false;
                SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(editForm, GetStringSessionValue(UserSession.Key.RootFormId));

                SetSessionValue(UserSession.Key.RequestedViewId, surveyAnswerDTO.ViewId);

                surveyAnswerDTO.RecoverLastRecordVersion = GetBoolSessionValue(UserSession.Key.RecoverLastRecordVersion);

                RemoveSessionValue(UserSession.Key.RecoverLastRecordVersion);
                return RedirectToAction(ViewActions.Index, ControllerNames.Survey, new { responseid = editResponseId, PageNumber = 1, surveyid = surveyAnswerDTO.SurveyId, Edit = "Edit" });
            }
            else
            {
                SetSessionValue(UserSession.Key.IsEditMode, false);
                isNewRecord = true;
            }
            bool isMobileDevice = this.Request.Browser.IsMobileDevice;


            if (isMobileDevice == false)
            {
                isMobileDevice = Epi.Cloud.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }

            FormsAuthentication.SetAuthCookie("BeginSurvey", false);

            //create the responseid
            Guid responseId = Guid.NewGuid();
            TempData[TempDataKeys.ResponseId] = responseId.ToString();

            // create the first survey response
            // Epi.Cloud.Common.DTO.SurveyAnswerDTO SurveyAnswer = _isurveyFacade.CreateSurveyAnswer(surveyModel.SurveyId, ResponseID.ToString());
            SetSessionValue(UserSession.Key.RootFormId, addNewFormId);
            var rootResponseId = responseId;
            SetSessionValue(UserSession.Key.RootResponseId, rootResponseId);

            var responseContext = new ResponseContext
            {
                FormId = addNewFormId,
                ResponseId = responseId.ToString(),
                IsNewRecord = isNewRecord,
                UserOrgId = orgId,
                UserId = userId,
                UserName = userName
            }.ResolveMetadataDependencies() as ResponseContext;

            int currentOrgId = GetIntSessionValue(UserSession.Key.SelectedOrgId);

            SurveyAnswerDTO surveyAnswer = _surveyFacade.CreateSurveyAnswer(responseContext, false, currentOrgId);
            surveyId = /*FromURL*/surveyId ?? surveyAnswer.SurveyId;

            // Initialize the Metadata Accessor
            MetadataAccessor.CurrentFormId = surveyId;

            MvcDynamicForms.Form form = _surveyFacade.GetSurveyFormData(surveyAnswer.SurveyId, 1, surveyAnswer, isMobileDevice);
            SurveyInfoModel surveyInfoModel = form.SurveyInfo.ToFormInfoModel();

            MetadataAccessor metadataAccessor = form.SurveyInfo as MetadataAccessor;

            // set the survey answer to be production or test 
            surveyAnswer.IsDraftMode = form.SurveyInfo.IsDraftMode;

            TempData[TempDataKeys.Width] = form.Width + 100;

            string checkcode = MetadataAccessor.GetFormDigest(surveyId).CheckCode;
            form.FormCheckCodeObj = form.GetCheckCodeObj(metadataAccessor.GetFieldDigests(surveyId), surveyAnswer.ResponseDetail, checkcode);

            ///////////////////////////// Execute - Record Before - start//////////////////////
            Dictionary<string, string> contextDetailList = new Dictionary<string, string>();
            EnterRule functionObject_B = (EnterRule)form.FormCheckCodeObj.GetCommand("level=record&event=before&identifier=");
            SurveyResponseBuilder surveyResponseBuilder = new SurveyResponseBuilder(_requiredList);
            if (functionObject_B != null && !functionObject_B.IsNull())
            {
                try
                {
                    PageDigest[] pageDigests = form.MetadataAccessor.GetCurrentFormPageDigests();
                    var responseDetail = surveyAnswer.ResponseDetail;

                    responseDetail = surveyResponseBuilder.CreateResponseDocument(responseContext, pageDigests);
                    SetSessionValue(UserSession.Key.RequiredList, surveyResponseBuilder.RequiredList);
                    _requiredList = surveyResponseBuilder.RequiredList;
                    form.RequiredFieldsList = _requiredList;
                    functionObject_B.Context.HiddenFieldList = form.HiddenFieldsList;
                    functionObject_B.Context.HighlightedFieldList = form.HighlightedFieldsList;
                    functionObject_B.Context.DisabledFieldList = form.DisabledFieldsList;
                    functionObject_B.Context.RequiredFieldList = form.RequiredFieldsList;

                    functionObject_B.Execute();

                    // field list
                    form.HiddenFieldsList = functionObject_B.Context.HiddenFieldList;
                    form.HighlightedFieldsList = functionObject_B.Context.HighlightedFieldList;
                    form.DisabledFieldsList = functionObject_B.Context.DisabledFieldList;
                    form.RequiredFieldsList = functionObject_B.Context.RequiredFieldList;


                    contextDetailList = Epi.Cloud.MVC.Utility.SurveyHelper.GetContextDetailList(functionObject_B);
                    form = Epi.Cloud.MVC.Utility.SurveyHelper.UpdateControlsValuesFromContext(form, contextDetailList);

                    _surveyFacade.UpdateSurveyResponse(surveyInfoModel,
                                                        responseId.ToString(),
                                                        form,
                                                        surveyAnswer,
                                                        false,
                                                        false,
                                                        0,
                                                        orgId,
                                                        userId,
                                                        userName);
                }
                catch (Exception ex)
                {
                    // do nothing so that processing
                    // can continue
                }
            }
            else
            {
                PageDigest[] pageDigestArray = metadataAccessor.GetPageDigests(surveyInfoModel.SurveyId);

                surveyAnswer.ResponseDetail = surveyResponseBuilder.CreateResponseDocument(responseContext, pageDigestArray);

                _requiredList = surveyResponseBuilder.RequiredList;
                SetSessionValue(UserSession.Key.RequiredList, surveyResponseBuilder.RequiredList);
                form.RequiredFieldsList = _requiredList;
            }

            ///////////////////////////// Execute - Record Before - End//////////////////////
            return RedirectToAction(ViewActions.Index, ControllerNames.Survey, new { responseid = responseId, PageNumber = 1, surveyid = surveyInfoModel.SurveyId });
        }

        private string GetChildRecordId(SurveyAnswerDTO surveyAnswerDTO)
        {
            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
            SurveyAnswerResponse surveyAnswerResponse = new SurveyAnswerResponse();
            string childResponseId = Guid.NewGuid().ToString();
            surveyAnswerDTO.ParentResponseId = surveyAnswerDTO.ResponseId;
            surveyAnswerDTO.ResponseId = childResponseId;
            surveyAnswerDTO.Status = RecordStatus.InProcess;
            surveyAnswerDTO.ReasonForStatusChange = RecordStatusChangeReason.CreateMulti;
            surveyAnswerRequest.SurveyAnswerList.Add(surveyAnswerDTO);
            string result;

            //responseId = TempData[TempDataKeys.ResponseId].ToString();
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            surveyAnswerRequest.Criteria.UserId = userId;
            surveyAnswerRequest.RequestId = childResponseId;
            surveyAnswerRequest.Action = RequestAction.CreateMulti;
            surveyAnswerResponse = _surveyFacade.SetChildRecord(surveyAnswerRequest);
            result = surveyAnswerResponse.SurveyResponseList[0].ResponseId;
            return result;
        }

        [HttpGet]
        [Authorize]
        public ActionResult ReadResponseInfo(string formid, int page = 1)
        {
            bool isMobileDevice = this.Request.Browser.IsMobileDevice;

            var model = new FormResponseInfoModel();

            SetSessionValue(UserSession.Key.RootFormId, formid);
            model = GetFormResponseInfoModel(formid, page);

            if (isMobileDevice == false)
            {
                return PartialView("ListResponses", model);
            }
            else
            {
                return View("ListResponses", model);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult ReadSortedResponseInfo(string formId, int? page, string sort, string sortField, int orgId, bool reset = false)
        {
            page = page.HasValue ? page.Value : 1;
            sortField = sortField.ToLower();
            bool isMobileDevice = this.Request.Browser.IsMobileDevice;

            //Code added to retain Search Starts

            if (reset)
            {
                RemoveSessionValue(UserSession.Key.SortOrder);
                RemoveSessionValue(UserSession.Key.SortField);
            }

            if (IsSessionValueNull(UserSession.Key.ProjectId))
            {
                // This will prime the cache if the project is not already loaded into cache.
                SetSessionValue(UserSession.Key.ProjectId, _projectMetadataProvider.GetProjectId_RetrieveProjectIfNecessary());
            }

            SetSessionValue(UserSession.Key.SelectedOrgId, orgId);
            var rootFormId = GetStringSessionValue(UserSession.Key.RootFormId, defaultValue: null);
            if (rootFormId == formId)
            {
                string sessionSortOrder = GetStringSessionValue(UserSession.Key.SortOrder, defaultValue: null);
                if (!string.IsNullOrEmpty(sessionSortOrder) && string.IsNullOrEmpty(sort))
                {
                    sort = sessionSortOrder;
                }

                string sessionSortField = GetStringSessionValue(UserSession.Key.SortField, defaultValue: null);
                if (!string.IsNullOrEmpty(sessionSortField) && string.IsNullOrEmpty(sortField))
                {
                    sortField = sessionSortField;
                }

                SetSessionValue(UserSession.Key.SortOrder, sort);
                SetSessionValue(UserSession.Key.SortField, sortField);
                SetSessionValue(UserSession.Key.PageNumber, page.Value);
            }
            else
            {
                ResponseContext responseContext = new ResponseContext { FormId = formId, RootFormId = formId };
                SetSessionValue(UserSession.Key.ResponseContext, responseContext);

                RemoveSessionValue(UserSession.Key.SortOrder);
                RemoveSessionValue(UserSession.Key.SortField);
                SetSessionValue(UserSession.Key.RootFormId, formId);
                SetSessionValue(UserSession.Key.PageNumber, page.Value);

            }

            //Code added to retain Search Ends. 

            var formResponseInfoModel = GetFormResponseInfoModel(formId, page.Value, sort, sortField, orgId);

            if (isMobileDevice == false)
            {
                return PartialView("ListResponses", formResponseInfoModel);
            }
            else
            {
                return View("ListResponses", formResponseInfoModel);
            }
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ResetSort(string formId)
        {
            RemoveSessionValue(UserSession.Key.SortOrder);
            RemoveSessionValue(UserSession.Key.SortField);
            return Json(true);
        }

        /// <summary>
        /// Following Action method takes ResponseId as a parameter and deletes the response.
        /// For now it returns nothing as a confirmation of deletion, we may add some error/success
        /// messages later. TBD
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(string responseId)
        {
            var rootFormId = GetStringSessionValue(UserSession.Key.RootFormId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            var responseContext = new ResponseContext
            {
                ResponseId = responseId,
                RootResponseId = responseId,
                FormId = rootFormId,
                UserOrgId = orgId,
                UserId = userId
            }.ResolveMetadataDependencies() as ResponseContext;

            SurveyAnswerRequest surveyAnswerRequest = responseContext.ToSurveyAnswerRequest();
            surveyAnswerRequest.SurveyAnswerList.Add(responseContext.ToSurveyAnswerDTO());
            surveyAnswerRequest.Criteria.UserOrganizationId = orgId;
            surveyAnswerRequest.Criteria.UserId = userId;
            surveyAnswerRequest.Criteria.IsSqlProject = GetBoolSessionValue(UserSession.Key.IsSqlProject);
            surveyAnswerRequest.Criteria.SurveyId = rootFormId;
            surveyAnswerRequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteResponse;
            surveyAnswerRequest.Action = RequestAction.Delete;
            SurveyAnswerResponse surveyAnswerResponse = _surveyFacade.DeleteResponse(surveyAnswerRequest);
            return Json(string.Empty);
        }

        public SurveyInfoModel GetSurveyInfo(string SurveyId)
        {
            SurveyInfoModel surveyInfoModel = _surveyFacade.GetSurveyInfoModel(SurveyId);
            return surveyInfoModel;
        }

        public List<FormInfoModel> GetFormsInfoList(Guid UserId, int OrgID)
        {
            FormsInfoRequest formReq = new FormsInfoRequest();

            formReq.Criteria.UserId = GetIntSessionValue(UserSession.Key.UserId);
            formReq.Criteria.CurrentOrgId = OrgID;

            List<FormInfoModel> listOfFormsInfoModel = _surveyFacade.GetFormsInfoModelList(formReq);

            return listOfFormsInfoModel;
        }

        public FormResponseInfoModel GetFormResponseInfoModel(string surveyId, int pageNumber, string sort = "", string sortfield = "", int orgid = -1)
        {
            // Initialize the Metadata Accessor
            MetadataAccessor.CurrentFormId = /*FromURL*/surveyId;

            FormResponseInfoModel formResponseInfoModel = null;

            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);
            if (!string.IsNullOrEmpty(/*FromURL*/surveyId))
            {
                formResponseInfoModel = GetFormResponseInfoModel(/*FromURL*/surveyId, orgid, userId);
                FormSettingResponse formSettingResponse = formResponseInfoModel.FormSettingResponse;

                var surveyResponseBuilder = new SurveyResponseBuilder();

                formResponseInfoModel.FormInfoModel.IsShared = formSettingResponse.FormInfo.IsShared;
                formResponseInfoModel.FormInfoModel.IsShareable = formSettingResponse.FormInfo.IsShareable;
                formResponseInfoModel.FormInfoModel.FormName = formSettingResponse.FormInfo.FormName;
                formResponseInfoModel.FormInfoModel.FormNumber = formSettingResponse.FormInfo.FormNumber;


                // Set User Role 
                SetUserRole(userId, orgid);

                var responseContext = new ResponseContext
                {
                    RootFormId = surveyId,
                    UserOrgId = orgid,
                    UserId = userId,
                    UserName = userName
                }.ResolveMetadataDependencies();

                SurveyAnswerRequest formResponseReq = new SurveyAnswerRequest { ResponseContext = responseContext };
                formResponseReq.Criteria.SurveyId = /*FromURL*/surveyId.ToString();
                formResponseReq.Criteria.PageNumber = pageNumber;
                formResponseReq.Criteria.UserId = userId;
                formResponseReq.Criteria.IsSqlProject = formSettingResponse.FormInfo.IsSQLProject;
                formResponseReq.Criteria.IsShareable = formSettingResponse.FormInfo.IsShareable;
                formResponseReq.Criteria.DataAccessRuleId = formSettingResponse.FormSetting.SelectedDataAccessRule;

                formResponseReq.Criteria.UserOrganizationId = orgid;

                SetSessionValue(UserSession.Key.IsSqlProject, formSettingResponse.FormInfo.IsSQLProject);
                SetSessionValue(UserSession.Key.IsOwner, formSettingResponse.FormInfo.IsOwner);

                // Following code retain search starts
                string sessionSearchCriteria = GetStringSessionValue(UserSession.Key.SearchCriteria, defaultValue: null);
                if (!string.IsNullOrEmpty(sessionSearchCriteria) &&
                    (Request.QueryString["col1"] == null || Request.QueryString["col1"] == "undefined"))
                {
                    formResponseReq.Criteria.SearchCriteria = sessionSearchCriteria;
                    formResponseInfoModel.SearchModel = GetSessionValue<SearchBoxModel>(UserSession.Key.SearchModel);
                }
                else
                {
                    formResponseReq.Criteria.SearchCriteria = CreateSearchCriteria(Request.QueryString, formResponseInfoModel.SearchModel, formResponseInfoModel);
                    SetSessionValue(UserSession.Key.SearchModel, formResponseInfoModel.SearchModel);
                    SetSessionValue(UserSession.Key.SearchCriteria, formResponseReq.Criteria.SearchCriteria);
                }
                // Following code retain search ends
                PopulateDropDownlists(formResponseInfoModel, formSettingResponse.FormSetting.FormControlNameList.ToList());

                if (sort != null && sort.Length > 0)
                {
                    formResponseReq.Criteria.SortOrder = sort;
                }
                if (sortfield.Length > 0)
                {
                    formResponseReq.Criteria.Sortfield = sortfield;
                }

                formResponseReq.Criteria.SurveyQAList = _columns.ToDictionary(c => c.Key.ToString(), c => c.Value);
                formResponseReq.Criteria.FieldDigestList = formResponseInfoModel.ColumnDigests.ToDictionary(c => c.Key, c => c.Value);
                formResponseReq.Criteria.SearchDigestList = ToSearchDigestList(formResponseInfoModel.SearchModel, /*FromURL*/surveyId);


                SurveyAnswerResponse formResponseList = _surveyFacade.GetFormResponseList(formResponseReq);

                var surveyResponse = formResponseList.SurveyResponseList;//.Skip((pageNumber - 1) * 20).Take(20);
                formResponseList.SurveyResponseList = surveyResponse.ToList();
                List<ResponseModel> responseList = new List<ResponseModel>();
                List<ResponseModel> responseListModel = new List<ResponseModel>();
                Dictionary<string, string> dictory = new Dictionary<string, string>();
                List<Dictionary<string, string>> dictoryList = new List<Dictionary<string, string>>(); ;
                foreach (var item in formResponseList.SurveyResponseList)
                {
                    if (item.SqlData != null)
                    {
                        responseList.Add(ConvertRowToModel(item, _columns, "GlobalRecordId"));
                    }
                    else
                    {
                        responseList.Add(item.ToResponseModel(_columns));
                    }

                }
                string sortFieldcolumn = string.Empty;
                if (!string.IsNullOrEmpty(sortfield))
                {
                    foreach (var column in _columns)
                    {
                        if (column.Value.ToLower() == sortfield)
                        {
                            sortFieldcolumn = "Column" + column.Key;
                        }
                    }
                }

                var sortList = responseList;
                sortfield = sortfield.ToLower();
                if (!string.IsNullOrEmpty(sortfield))
                {
                    if (sort != "ASC")
                    {
                        switch (sortFieldcolumn)
                        {
                            case "Column1":
                                responseListModel = sortList.OrderByDescending(x => x.Column1).ToList();
                                break;
                            case "Column2":
                                responseListModel = sortList.OrderByDescending(x => x.Column2).ToList();
                                break;
                            case "Column3":
                                responseListModel = sortList.OrderByDescending(x => x.Column3).ToList();
                                break;
                            case "Column4":
                                responseListModel = sortList.OrderByDescending(x => x.Column4).ToList();
                                break;
                            case "Column5":
                                responseListModel = sortList.OrderByDescending(x => x.Column5).ToList();
                                break;
                        }
                    }
                    else
                    {
                        switch (sortFieldcolumn)
                        {
                            case "Column1":
                                responseListModel = sortList.OrderBy(x => x.Column1).ToList();
                                break;
                            case "Column2":
                                responseListModel = sortList.OrderBy(x => x.Column2).ToList();
                                break;
                            case "Column3":
                                responseListModel = sortList.OrderBy(x => x.Column3).ToList();
                                break;
                            case "Column4":
                                responseListModel = sortList.OrderBy(x => x.Column4).ToList();
                                break;
                            case "Column5":
                                responseListModel = sortList.OrderBy(x => x.Column5).ToList();
                                break;
                        }
                    }
                    formResponseInfoModel.ResponsesList = responseListModel.Skip((pageNumber - 1) * 20).Take(20).ToList();

                }
                if (string.IsNullOrEmpty(sort))
                {
                    formResponseInfoModel.ResponsesList = responseList.Skip((pageNumber - 1) * 20).Take(20).ToList();
                }


                //Setting Form Info 
                formResponseInfoModel.FormInfoModel = formResponseList.FormInfo.ToFormInfoModel();

                //Setting Additional Data
                formResponseInfoModel.NumberOfPages = formResponseList.NumberOfPages;
                formResponseInfoModel.PageSize = ReadPageSize();
                formResponseInfoModel.NumberOfResponses = formResponseList.NumberOfResponses;
                formResponseInfoModel.sortfield = sortfield;
                formResponseInfoModel.sortOrder = sort;
                formResponseInfoModel.CurrentPage = pageNumber;
            }
            return formResponseInfoModel;
        }

        private void SetUserRole(int userId, int orgId)
        {
            UserRequest userRequest = new UserRequest();
            userRequest.Organization.OrganizationId = orgId;
            userRequest.User.UserId = userId;
            var userResponse = _securityFacade.GetUserInfo(userRequest);
            if (userResponse.User.Count() > 0)
            {
                SetSessionValue(UserSession.Key.UsertRole, userResponse.User[0].Role);
            }
        }

        private void PopulateDropDownlists(FormResponseInfoModel formResponseInfoModel, List<KeyValuePair<int, string>> list)
        {
            PopulateDropDownlist(out formResponseInfoModel.SearchColumns1, formResponseInfoModel.SearchModel.SearchCol1, list);
            PopulateDropDownlist(out formResponseInfoModel.SearchColumns2, formResponseInfoModel.SearchModel.SearchCol2, list);
            PopulateDropDownlist(out formResponseInfoModel.SearchColumns3, formResponseInfoModel.SearchModel.SearchCol3, list);
            PopulateDropDownlist(out formResponseInfoModel.SearchColumns4, formResponseInfoModel.SearchModel.SearchCol4, list);
            PopulateDropDownlist(out formResponseInfoModel.SearchColumns5, formResponseInfoModel.SearchModel.SearchCol5, list);
        }

        private int ReadPageSize()
        {
            return AppSettings.GetIntValue(AppSettings.Key.ResponsePageSize);
        }

        [HttpGet]
        public ActionResult LogOut()
        {

            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction(ViewActions.Index, ControllerNames.Login);
        }

        [HttpGet]
        public ActionResult GetSettings(string formId)//List<FormInfoModel> ModelList, string formid)
        {
            var projectId = GetStringSessionValue(UserSession.Key.ProjectId);
            var userId = GetIntSessionValue(UserSession.Key.UserId);
            var currentOrgId = GetIntSessionValue(UserSession.Key.SelectedOrgId);

            List<KeyValuePair<int, string>> tempColumns = new List<KeyValuePair<int, string>>();
            //Get All forms
            List<FormsHierarchyDTO> formsHierarchy = GetFormsHierarchy(/*FromURL*/formId);
            List<SettingsInfoModel> modelList = new List<SettingsInfoModel>();
            List<FormSettingRequest> formSettingReqList = new List<FormSettingRequest>();
            foreach (var Item in formsHierarchy)
            {
                FormSettingRequest formSettingReq = new FormSettingRequest { ProjectId = projectId };
                formSettingReq.GetMetadata = true;
                formSettingReq.FormInfo.FormId = new Guid(Item.FormId).ToString();
                formSettingReq.FormInfo.UserId = userId;
                formSettingReq.CurrentOrgId = currentOrgId;
                formSettingReqList.Add(formSettingReq);
            }

            //Getting Column Name  List
            List<FormSettingResponse> formSettingResponseList = _surveyFacade.GetFormSettingsList(formSettingReqList);

            foreach (var formSettingResponse in formSettingResponseList)
            {
                _columns = formSettingResponse.FormSetting.ColumnNameList.ToList();
                tempColumns = _columns;
                _columns.Sort(Compare);

                Dictionary<int, string> dictionary = _columns.ToDictionary(pair => pair.Key, pair => pair.Value);
                SettingsInfoModel model = new SettingsInfoModel();
                model.SelectedControlNameList = dictionary;

                _columns = formSettingResponse.FormSetting.FormControlNameList.ToList();

                // Get Additional Metadata columns 
                var metadataColumns = Epi.Cloud.Common.Constants.Constant.MetadataColumnNames();
                Dictionary<int, string> columnDictionary = tempColumns.ToDictionary(pair => pair.Key, pair => pair.Value);

                foreach (var item in metadataColumns)
                {
                    if (!columnDictionary.ContainsValue(item))
                    {
                        _columns.Add(new KeyValuePair<int, string>(_columns.Count() + 1, item));
                    }
                }

                _columns.Sort(Compare);

                Dictionary<int, string> dictionary1 = _columns.ToDictionary(pair => pair.Key, pair => pair.Value);

                model.FormControlNameList = dictionary1;

                string sessionUserEmailAddress = GetStringSessionValue(UserSession.Key.UserEmailAddress);

                _columns = formSettingResponse.FormSetting.AssignedUserList.ToList();
                if (_columns.Exists(col => col.Value == sessionUserEmailAddress))
                {
                    _columns.Remove(_columns.First(u => u.Value == sessionUserEmailAddress));
                }

                //Columns.Sort(Compare);

                Dictionary<int, string> dictionary2 = _columns.ToDictionary(pair => pair.Key, pair => pair.Value);

                model.AssignedUserList = dictionary2;

                _columns = formSettingResponse.FormSetting.UserList.ToList();

                if (_columns.Exists(col => col.Value == sessionUserEmailAddress))
                {
                    _columns.Remove(_columns.First(u => u.Value == sessionUserEmailAddress));
                }

                Dictionary<int, string> dictionary3 = _columns.ToDictionary(pair => pair.Key, pair => pair.Value);

                model.UserList = dictionary3;

                _columns = formSettingResponse.FormSetting.AvailableOrgList.ToList();
                Dictionary<int, string> dictionary4 = _columns.ToDictionary(pair => pair.Key, pair => pair.Value);
                model.AvailableOrgList = dictionary4;

                _columns = formSettingResponse.FormSetting.SelectedOrgList.ToList();
                Dictionary<int, string> dictionary5 = _columns.ToDictionary(pair => pair.Key, pair => pair.Value);
                model.SelectedOrgList = dictionary5;

                model.IsShareable = formSettingResponse.FormInfo.IsShareable;
                model.IsDraftMode = formSettingResponse.FormInfo.IsDraftMode;
                model.FormOwnerFirstName = formSettingResponse.FormInfo.OwnerFName;
                model.FormOwnerLastName = formSettingResponse.FormInfo.OwnerLName;
                model.FormName = formSettingResponse.FormInfo.FormName;
                model.FormId = formSettingResponse.FormInfo.FormId;
                model.DataAccessRuleIds = formSettingResponse.FormSetting.DataAccessRuleIds;
                model.SelectedDataAccessRule = formSettingResponse.FormSetting.SelectedDataAccessRule;
                model.HasDraftModeData = formSettingResponse.FormInfo.HasDraftModeData;
                var DataAccessRuleDescription = "";
                foreach (var item in formSettingResponse.FormSetting.DataAccessRuleDescription)
                {
                    DataAccessRuleDescription = DataAccessRuleDescription + item.Key.ToString() + " : " + item.Value + "\n";
                }

                model.DataAccessRuleDescription = DataAccessRuleDescription;
                modelList.Add(model);
            }
            return PartialView("Settings", modelList);
        }

        [HttpPost]
        public ActionResult GotoMetaadmin()//List<FormInfoModel> ModelList, string formid)
        {
            return RedirectToAction("Index", "MetadataAdmin");
        }


        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CheckForConcurrency(String responseId)
        {
            ResponseContext responseContext = GetSetResponseContext(responseId);

            var surveyAnswerDTO = GetSurveyAnswerState(responseContext);
            if (surveyAnswerDTO.DateCreated == DateTime.MinValue)
            {
                surveyAnswerDTO.DateCreated = surveyAnswerDTO.DateUpdated;
            }
            surveyAnswerDTO.LoggedInUserOrgId = responseContext.UserOrgId;
            surveyAnswerDTO.LoggedInUserId = responseContext.UserId;
            SetSessionValue(UserSession.Key.EditForm, responseId);

            var json = Json(surveyAnswerDTO.ToSurveyAnswerDTO());
            return json;
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Notify(String responseId)
        {
            int userId = GetIntSessionValue(UserSession.Key.UserId);

            //Get current user info
            int currentOrgId = GetIntSessionValue(UserSession.Key.SelectedOrgId);
            var userInfo = _securityFacade.GetUserInfo(userId);
            //Get Organization admin info 
            var surveyAnswerDTO = GetSurveyAnswer(responseId, GetStringSessionValue(UserSession.Key.RootFormId));
            SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswerDTO.SurveyId);

            var OwnerInfo = _securityFacade.GetUserInfo(surveyAnswerDTO.FormOwnerId);

            var email = new Email();

            email.To = new List<string>();
            email.To.Add(OwnerInfo.User.EmailAddress);
            email.From = EmailAppSettings.GetStringValue(EmailAppSettings.Key.EmailFrom);
            //email.Subject = "Record locked notification.";
            //email.Body = "A user was unable to edit/delete a Epi Info™ Cloud Enter recored. \n \n Please login to Epi Info™ Cloud Enter system to Unlock this record.\n \n Below are the needed info to unlock the record.\n \n Response id: " + responseId + "\n\n User email: " + userInfo.User.EmailAddress + "\n\n";
            email.Subject = ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.RecordLocked_Subject);
            email.Body = string.Format(ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.RecordLocked_Body)
                , responseId, userInfo.User.EmailAddress);

            var success = EmailHandler.SendMessage(email);

            return Json(1);
        }

        //Unlock
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Unlock(String responseId, bool recoverLastRecordVersion = false)
        {
            try
            {
                SurveyAnswerRequest SurveyAnswerRequest = new SurveyAnswerRequest();
                SurveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = responseId });
                SurveyAnswerRequest.Criteria.StatusId = RecordStatus.Saved;
                SetSessionValue(UserSession.Key.RecoverLastRecordVersion, recoverLastRecordVersion);
                //  _isurveyFacade.UpdateResponseStatus(SurveyAnswerRequest);
            }
            catch (Exception ex)
            {
                return Json("Erorr");
            }
            return Json("Success");
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveSettings(string formid)
        {
            List<FormsHierarchyDTO> formsHierarchyDTOList = GetFormsHierarchy(formid);
            FormSettingRequest formSettingReq = new FormSettingRequest { ProjectId = GetStringSessionValue(UserSession.Key.ProjectId) };
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            foreach (var formsHierarchyDTO in formsHierarchyDTOList)
            {
                formSettingReq.GetMetadata = true;
                formSettingReq.FormInfo.FormId = new Guid(formid).ToString();
                formSettingReq.FormInfo.UserId = userId;
                FormSettingDTO formSetting = new FormSettingDTO();
                formSetting.FormId = formsHierarchyDTO.FormId;
                formSetting.ColumnNameList = GetDictionary(this.Request.Form["SelectedColumns_" + formsHierarchyDTO.FormId]);
                formSetting.AssignedUserList = GetDictionary(this.Request.Form["SelectedUser"]);
                formSetting.SelectedOrgList = GetDictionary(this.Request.Form["SelectedOrg"]);
                formSetting.IsShareable = GetBoolValue(this.Request.Form["IsShareable"]);
                formSetting.SelectedDataAccessRule = int.Parse(this.Request.Form["DataAccessRuleId"]);

                if (!string.IsNullOrEmpty(this.Request.Form["SoftDeleteForm"]) && this.Request.Form["SoftDeleteForm"].ToUpper() == "ON")
                {
                    formSetting.IsDisabled = true;
                }
                if (!string.IsNullOrEmpty(this.Request.Form["RemoveTestData"]) && this.Request.Form["RemoveTestData"].ToUpper() == "ON")
                {
                    formSetting.DeleteDraftData = true;
                }
                formSettingReq.FormSetting.Add(formSetting);
                formSettingReq.FormInfo.IsDraftMode = GetBoolValue(this.Request.Form["Mode"]);
            }
            FormSettingResponse formSettingResponse = _surveyFacade.SaveSettings(formSettingReq);

            bool isMobileDevice = this.Request.Browser.IsMobileDevice;

            var model = new FormResponseInfoModel();

            int currentOrgId = GetIntSessionValue(UserSession.Key.SelectedOrgId);
            model = GetFormResponseInfoModel(formid, 1, "", "", currentOrgId);

            if (isMobileDevice == false)
            {
                if (!string.IsNullOrEmpty(this.Request.Form["SoftDeleteForm"]) && this.Request.Form["SoftDeleteForm"].ToUpper() == "ON")
                {
                    return Json(null);
                }
                else
                {
                    return PartialView("ListResponses", model);
                }
            }
            else
            {
                return View("ListResponses", model);
            }
        }

        private Dictionary<int, string> GetDictionary(string list)
        {
            Dictionary<int, string> Dictionary = new Dictionary<int, string>();
            if (!string.IsNullOrEmpty(list))
            {
                Dictionary = list.Split(',').ToList().Select((s, i) => new { s, i }).ToDictionary(x => x.i, x => x.s);
            }
            return Dictionary;
        }

        private bool GetBoolValue(string value)
        {
            bool BoolValue = false;
            if (!string.IsNullOrEmpty(value))
            {
                int val = int.Parse(value);
                if (val == 1)
                {
                    BoolValue = true;
                }
            }

            return BoolValue;
        }


        private List<FormsHierarchyDTO> GetFormsHierarchy(string formId)
        {
            FormsHierarchyResponse formsHierarchyResponse = new FormsHierarchyResponse();
            FormsHierarchyRequest formsHierarchyRequest = new FormsHierarchyRequest();

            formsHierarchyRequest.SurveyInfo.FormId = formId;
            formsHierarchyResponse = _surveyFacade.GetFormsHierarchy(formsHierarchyRequest);

            return formsHierarchyResponse.FormsHierarchy;
        }
    }
}
