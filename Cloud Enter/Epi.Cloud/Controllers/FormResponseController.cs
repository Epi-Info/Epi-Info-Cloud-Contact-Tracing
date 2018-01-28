using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
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

namespace Epi.Cloud.MVC.Controllers
{
    [Authorize]
    public class FormResponseController : BaseSurveyController
    {
        private IEnumerable<AbridgedFieldInfo> _pageFields;
        private string _requiredList = "";
        private bool _isNewRequest = true; //Added for retain search and sort

        public FormResponseController(ISurveyFacade isurveyFacade,
                                      IProjectMetadataProvider projectMetadataProvider
)
        {
            _surveyFacade = isurveyFacade;
            _projectMetadataProvider = projectMetadataProvider;
        }

        [HttpGet]
        //string responseid,string SurveyId, int ViewId, string CurrentPage
        // View =0 Root form
        public ActionResult Index(string formid, string responseId, int pagenumber = 1, int viewId = 0)
        {
            bool reset = false;
            string sortField = null;
            string sortOrder = null;
            if (Request.QueryString["sortfield"] != null)
            {
                sortField = Request.QueryString["sortfield"];
            }
            if (Request.QueryString["sort"] != null)
            {
                sortOrder = Request.QueryString["sort"];
            }
            bool.TryParse(Request.QueryString["reset"], out reset);
            if (reset)
            {
                RemoveSessionValue(UserSession.Key.SortOrder);
                RemoveSessionValue(UserSession.Key.SortField);
                RemoveSessionValue(UserSession.Key.SearchCriteria);
            }
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            bool isAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isAndroid = true;
            }
            if (viewId == 0)
            {
                //Following code checks if request is for new or selected form.
                if (GetStringSessionValue(UserSession.Key.RootFormId) == formid)
                {
                    _isNewRequest = false;
                }

                SetSessionValue(UserSession.Key.RootFormId, formid);
                RemoveSessionValue(UserSession.Key.RootResponseId);
                RemoveSessionValue(UserSession.Key.FormValuesHasChanged);
                SetSessionValue(UserSession.Key.IsEditMode, false);

                var model = new FormResponseInfoModel();
                model.ViewId = viewId;
                int currentOrgId = GetIntSessionValue(UserSession.Key.CurrentOrgId, defaultValue: -1);
                model = GetSurveyResponseInfoModel(formid, pagenumber, sortOrder, sortField, currentOrgId);
                SetSessionValue(UserSession.Key.SelectedOrgId, model.FormInfoModel.OrganizationId);
                return View("Index", model);
            }
            else
            {
                List<FormsHierarchyDTO> formsHierarchy = GetFormsHierarchy();
                int userId = SurveyHelper.GetDecryptUserId(Session[UserSession.Key.UserId].ToString());

                bool isMobileDevice = this.Request.Browser.IsMobileDevice;

                int requestedViewId = viewId;

                SetSessionValue(UserSession.Key.RequestedViewId, requestedViewId);
                SurveyModel surveyModel = new SurveyModel();

                surveyModel.RelateModel = formsHierarchy.ToRelateModel(formid);
                surveyModel.RequestedViewId = requestedViewId;

                var relateSurveyId = formsHierarchy.Single(x => x.ViewId == viewId);

                if (!string.IsNullOrEmpty(responseId))
                {
                    surveyModel.FormResponseInfoModel = GetFormResponseInfoModels(relateSurveyId.FormId, responseId, formsHierarchy);
                    surveyModel.FormResponseInfoModel.NumberOfResponses = surveyModel.FormResponseInfoModel.ResponsesList.Count();

                    surveyModel.FormResponseInfoModel.ParentResponseId = responseId;
                }

                if (relateSurveyId.ResponseIds.Count() > 0)
                {
                    SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(relateSurveyId.ResponseIds[0].ResponseId, relateSurveyId.FormId);
                    var form = _surveyFacade.GetSurveyFormData(relateSurveyId.ResponseIds[0].SurveyId, 1, surveyAnswerDTO, isMobileDevice, null, null, isAndroid);
                    surveyModel.Form = form;
                    if (string.IsNullOrEmpty(responseId))
                    {
                        surveyModel.FormResponseInfoModel = GetFormResponseInfoModels(relateSurveyId.FormId, responseId, formsHierarchy);
                        surveyModel.FormResponseInfoModel.ParentResponseId = relateSurveyId.ResponseIds[0].ParentResponseId;
                    }

                    surveyModel.FormResponseInfoModel.FormInfoModel.FormName = form.SurveyInfo.SurveyName;
                    surveyModel.FormResponseInfoModel.FormInfoModel.FormId = form.SurveyInfo.SurveyId;
                    surveyModel.FormResponseInfoModel.NumberOfResponses = surveyModel.FormResponseInfoModel.ResponsesList.Count();
                }
                else
                {
                    FormResponseInfoModel responseInfoModel = new FormResponseInfoModel();
                    if (surveyModel.FormResponseInfoModel.ResponsesList.Count() > 0)
                    {
                        SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(surveyModel.FormResponseInfoModel.ResponsesList[0].Column0, relateSurveyId.FormId);
                        responseInfoModel = GetFormResponseInfoModels(relateSurveyId.FormId, responseId, formsHierarchy);
                        surveyModel.Form = _surveyFacade.GetSurveyFormData(surveyAnswerDTO.SurveyId, 1, surveyAnswerDTO, isMobileDevice, null, null, isAndroid);
                        responseInfoModel.FormInfoModel.FormName = surveyModel.Form.SurveyInfo.SurveyName.ToString();
                        responseInfoModel.FormInfoModel.FormId = surveyModel.Form.SurveyInfo.SurveyId.ToString();
                        responseInfoModel.ParentResponseId = responseId;//SurveyModel.FormResponseInfoModel.ResponsesList[0].Column0;
                        responseInfoModel.NumberOfResponses = surveyModel.FormResponseInfoModel.ResponsesList.Count();
                    }
                    else
                    {
                        var form1 = _surveyFacade.GetSurveyInfoModel(relateSurveyId.FormId);
                        responseInfoModel.FormInfoModel.FormName = form1.SurveyName.ToString();
                        responseInfoModel.FormInfoModel.FormId = form1.SurveyId.ToString();
                        responseInfoModel.ParentResponseId = responseId;
                    }
                    surveyModel.FormResponseInfoModel = responseInfoModel;
                }
                surveyModel.FormResponseInfoModel.ViewId = viewId;

                return View("Index", surveyModel.FormResponseInfoModel);
            }
        }

        [HttpPost]
        public ActionResult Index(string surveyId, string addNewFormId, string editForm, string cancel)
        {
            // Assign "editForm" parameter to a less confusing name. 
            // editForm contains the responseId of the record being edited.
            string editResponseId = editForm;

            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);
            bool isMobileDevice = this.Request.Browser.IsMobileDevice;
            FormsAuthentication.SetAuthCookie("BeginSurvey", false);
            bool isEditMode = false;
            if (isMobileDevice == true)
            {
                isMobileDevice = Epi.Cloud.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }
            bool isAndroid = this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!string.IsNullOrEmpty(/*FromURL*/cancel))
            {
                int pageNumber;
                int.TryParse(/*FromURL*/cancel, out pageNumber);
                Dictionary<string, int> surveyPagesList = GetSessionValue<Dictionary<string, int>>(UserSession.Key.RelateButtonPageId);
                if (surveyPagesList != null)
                {
                    pageNumber = surveyPagesList[this.Request.Form["Parent_Response_Id"].ToString()];
                }

                return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = this.Request.Form["Parent_Response_Id"].ToString(), PageNumber = pageNumber });
            }
            if (string.IsNullOrEmpty(/*FromURL*/editResponseId) && string.IsNullOrEmpty(/*FromURL*/addNewFormId) && !IsSessionValueNull(UserSession.Key.EditResponseId))
            {
                editResponseId = GetStringSessionValue(UserSession.Key.EditResponseId);
            }

            if (!string.IsNullOrEmpty(editResponseId))
            {
                if (IsSessionValueNull(UserSession.Key.RootResponseId))
                {
                    SetSessionValue(UserSession.Key.RootResponseId, editResponseId);
                }

                isEditMode = true;
                SetSessionValue(UserSession.Key.IsEditMode, isEditMode);

                SurveyAnswerDTO surveyAnswer = GetSurveyAnswer(editResponseId, GetStringSessionValue(UserSession.Key.RootFormId));
                if (!IsSessionValueNull(UserSession.Key.RecoverLastRecordVersion))
                {
                    surveyAnswer.RecoverLastRecordVersion = GetBoolSessionValue(UserSession.Key.RecoverLastRecordVersion);
                }
                string childRecordId = GetChildRecordId(surveyAnswer);
                return RedirectToAction(ViewActions.Index, ControllerNames.Survey, new { responseid = surveyAnswer.ParentResponseId, PageNumber = 1, Edit = "Edit" });
            }

            //create the responseid
            Guid responseId = Guid.NewGuid();
            if (IsSessionValueNull(UserSession.Key.RootResponseId))
            {
                SetSessionValue(UserSession.Key.RootResponseId, responseId);
            }

            var rootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId);

            TempData[TempDataKeys.ResponseId] = responseId.ToString();

            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);

            var responseContext = InitializeResponseContext(formId: /*FromURL*/addNewFormId, responseId: responseId.ToString(), parentResponseId: this.Request.Form["Parent_Response_Id"].ToString(), isNewRecord: !isEditMode);

            // create the first survey response
            SurveyAnswerDTO surveyAnswerDTO = _surveyFacade.CreateSurveyAnswer(responseContext);
            List<FormsHierarchyDTO> formsHierarchy = GetFormsHierarchy();
            SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswerDTO.SurveyId, formsHierarchy);
            MetadataAccessor metadataAccessor = surveyInfoModel as MetadataAccessor;

            // set the survey answer to be production or test 
            surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;

            MvcDynamicForms.Form form = _surveyFacade.GetSurveyFormData(surveyAnswerDTO.SurveyId, 1, surveyAnswerDTO, isMobileDevice, null, formsHierarchy, isAndroid);

            TempData[TempDataKeys.Width] = form.Width + 100;

            var formDigest = metadataAccessor.GetFormDigest(surveyAnswerDTO.SurveyId);
            string checkcode = formDigest.CheckCode;

            FormResponseDetail responseDetail = surveyAnswerDTO.ResponseDetail;

            form.FormCheckCodeObj = form.GetCheckCodeObj(MetadataAccessor.GetFieldDigests(surveyAnswerDTO.SurveyId), responseDetail, checkcode);

            ///////////////////////////// Execute - Record Before - start//////////////////////
            Dictionary<string, string> ContextDetailList = new Dictionary<string, string>();
            EnterRule functionObject_B = (EnterRule)form.FormCheckCodeObj.GetCommand("level=record&event=before&identifier=");
            SurveyResponseBuilder surveyResponseBuilder = new SurveyResponseBuilder(_requiredList);
            if (functionObject_B != null && !functionObject_B.IsNull())
            {
                try
                {
                    PageDigest[] pageDigests = form.MetadataAccessor.GetCurrentFormPageDigests();
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


                    ContextDetailList = SurveyHelper.GetContextDetailList(functionObject_B);
                    form = SurveyHelper.UpdateControlsValuesFromContext(form, ContextDetailList);

                    _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId.ToString(), form, surveyAnswerDTO, false, false, 0, orgId, userId, userName);
                }
                catch (Exception ex)
                {
                    // do nothing so that processing
                    // can continue
                }
            }
            else
            {
                PageDigest[] pageDigestArray = form.MetadataAccessor.GetCurrentFormPageDigests();// metadataAccessor.GetPageDigests(surveyInfoModel.SurveyId);

                surveyAnswerDTO.ResponseDetail = surveyResponseBuilder.CreateResponseDocument(responseContext, pageDigestArray);

                _requiredList = surveyResponseBuilder.RequiredList;
                SetSessionValue(UserSession.Key.RequiredList, _requiredList);
                form.RequiredFieldsList = _requiredList;
                _surveyFacade.UpdateSurveyResponse(surveyInfoModel, surveyAnswerDTO.ResponseId, form, surveyAnswerDTO, false, false, 0, orgId, userId, userName);
            }

            surveyAnswerDTO = (SurveyAnswerDTO)formsHierarchy.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == surveyAnswerDTO.ResponseId);

            ///////////////////////////// Execute - Record Before - End//////////////////////
            return RedirectToAction(ViewActions.Index, ControllerNames.Survey, new { responseid = responseId, PageNumber = 1 });
        }


        public FormResponseInfoModel GetSurveyResponseInfoModel(string surveyId, int pageNumber, string sort = "", string sortfield = "", int orgid = -1)
        {
            // Initialize the Metadata Accessor
            MetadataAccessor.CurrentFormId = surveyId;

            FormResponseInfoModel formResponseInfoModel = null;
            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);

            if (!string.IsNullOrEmpty(surveyId))
            {
                formResponseInfoModel = GetFormResponseInfoModel(surveyId, orgid, userId);
                FormSettingResponse formSettingResponse = formResponseInfoModel.FormSettingResponse;
                var surveyResponseBuilder = new SurveyResponseBuilder();
                formResponseInfoModel.FormInfoModel.IsShared = formSettingResponse.FormInfo.IsShared;
                formResponseInfoModel.FormInfoModel.IsShareable = formSettingResponse.FormInfo.IsShareable;
                formResponseInfoModel.FormInfoModel.FormName = formSettingResponse.FormInfo.FormName;
                formResponseInfoModel.FormInfoModel.FormNumber = formSettingResponse.FormInfo.FormNumber;


                // Set User Role 
                //if (formResponseInfoModel.FormInfoModel.IsShared)
                //{

                //    SetUserRole(UserId, orgid);
                //}
                //else
                //{
                //SetUserRole(UserId, FormSettingResponse.FormInfo.OrganizationId);
                //}
                //SetUserRole(userId, orgid);

                var responseContext = InitializeResponseContext(formId: surveyId);

                SurveyAnswerRequest formResponseReq = new SurveyAnswerRequest { ResponseContext = responseContext };
                formResponseReq.Criteria.SurveyId = surveyId.ToString();
                formResponseReq.Criteria.PageNumber = /*FromURL*/pageNumber;
                formResponseReq.Criteria.UserId = userId;
                formResponseReq.Criteria.IsSqlProject = formSettingResponse.FormInfo.IsSQLProject;
                formResponseReq.Criteria.IsShareable = formSettingResponse.FormInfo.IsShareable;
                formResponseReq.Criteria.DataAccessRuleId = formSettingResponse.FormSetting.SelectedDataAccessRule;
                //formResponseReq.Criteria.IsMobile = true;
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
                if (!string.IsNullOrEmpty(sortfield) && sortfield.Length > 0)
                {
                    formResponseReq.Criteria.Sortfield = sortfield;
                }
                formResponseReq.Criteria.SurveyQAList = _columns.ToDictionary(c => c.Key.ToString(), c => c.Value);
                formResponseReq.Criteria.FieldDigestList = formResponseInfoModel.ColumnDigests.ToDictionary(c => c.Key, c => c.Value);
                formResponseReq.Criteria.SearchDigestList = ToSearchDigestList(formResponseInfoModel.SearchModel, surveyId);


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
                    sortfield = sortfield.ToLower();
                    foreach (var column in _columns)
                    {
                        if (column.Value.ToLower() == sortfield)
                        {
                            sortFieldcolumn = "Column" + column.Key;
                        }
                    }
                }

                var sortList = responseList;
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
                formResponseInfoModel.sortfield = /*FromURL*/sortfield;
                formResponseInfoModel.sortOrder = /*FromURL*/sort;
                formResponseInfoModel.CurrentPage = /*FromURL*/pageNumber;
            }
            return formResponseInfoModel;
        }

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
                SetSessionValue(UserSession.Key.RootFormId, formId);

                ResponseContext responseContext = InitializeResponseContext(formId: formId) as ResponseContext;
                SetSessionValue(UserSession.Key.ResponseContext, responseContext);

                RemoveSessionValue(UserSession.Key.SortOrder);
                RemoveSessionValue(UserSession.Key.SortField);
                SetSessionValue(UserSession.Key.PageNumber, page.Value);
            }

            //Code added to retain Search Ends. 

            var formResponseInfoModel = GetSurveyResponseInfoModel(formId, page.Value, sort, sortField, orgId);

            if (isMobileDevice == false)
            {
                return PartialView("ListResponses", formResponseInfoModel);
            }
            else
            {
                return View("ListResponses", formResponseInfoModel);
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

        public SurveyInfoModel GetSurveyInfo(string surveyId, List<FormsHierarchyDTO> formsHierarchyDTOList = null)
        {
            SurveyInfoModel surveyInfoModel = new SurveyInfoModel();
            if (formsHierarchyDTOList != null)
            {
                var formsHierarchyDTO = formsHierarchyDTOList.FirstOrDefault(x => x.FormId == surveyId).SurveyInfo;
                surveyInfoModel = formsHierarchyDTO.ToSurveyInfoModel();
            }
            else
            {
                surveyInfoModel = _surveyFacade.GetSurveyInfoModel(surveyId);
            }
            return surveyInfoModel;
        }

        private string GetChildRecordId(SurveyAnswerDTO surveyAnswerDTO)
        {
            SurveyAnswerRequest SurveyAnswerRequest = new SurveyAnswerRequest();
            SurveyAnswerResponse SurveyAnswerResponse = new SurveyAnswerResponse();
            string ChildId = Guid.NewGuid().ToString();
            surveyAnswerDTO.ParentResponseId = surveyAnswerDTO.ResponseId;
            surveyAnswerDTO.ResponseId = ChildId;
            surveyAnswerDTO.Status = RecordStatus.InProcess;
            surveyAnswerDTO.ReasonForStatusChange = RecordStatusChangeReason.CreateMulti;
            SurveyAnswerRequest.SurveyAnswerList.Add(surveyAnswerDTO);
            string result;

            //responseId = TempData[TempDataKeys.ResponseId].ToString();
            SurveyAnswerRequest.Criteria.UserId = GetIntSessionValue(UserSession.Key.UserId);
            SurveyAnswerRequest.RequestId = ChildId;
            SurveyAnswerRequest.Action = RequestAction.CreateMulti;
            SurveyAnswerResponse = _surveyFacade.SetChildRecord(SurveyAnswerRequest);
            result = SurveyAnswerResponse.SurveyResponseList[0].ResponseId.ToString();
            return result;
        }

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
            surveyAnswerRequest.SurveyAnswerList.Add(responseContext.ToSurveyAnswerDTOLite());
            surveyAnswerRequest.Criteria.UserOrganizationId = orgId;
            surveyAnswerRequest.Criteria.UserId = userId;
            surveyAnswerRequest.Criteria.IsSqlProject = GetBoolSessionValue(UserSession.Key.IsSqlProject);
            surveyAnswerRequest.Criteria.SurveyId = rootFormId;
            surveyAnswerRequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteResponse;
            surveyAnswerRequest.Action = RequestAction.Delete;
            SurveyAnswerResponse surveyAnswerResponse = _surveyFacade.DeleteResponse(surveyAnswerRequest);
            return Json(string.Empty);
        }
       
        [HttpPost]
        public ActionResult DeleteBranch(string ResponseId)//List<FormInfoModel> ModelList, string formid)
        {

            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
            surveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = ResponseId });
            surveyAnswerRequest.Criteria.UserId = GetIntSessionValue(UserSession.Key.UserId);
            surveyAnswerRequest.Criteria.IsEditMode = false;
            surveyAnswerRequest.Criteria.IsDeleteMode = false;
            surveyAnswerRequest.Criteria.IsSqlProject = GetBoolSessionValue(UserSession.Key.IsSqlProject);
            surveyAnswerRequest.Criteria.SurveyId = GetStringSessionValue(UserSession.Key.RootFormId);
            SurveyAnswerResponse saResponse = _surveyFacade.DeleteResponse(surveyAnswerRequest);

            return Json(string.Empty);
        }

        private List<FormsHierarchyDTO> GetFormsHierarchy()
        {
            FormsHierarchyResponse formsHierarchyResponse = new FormsHierarchyResponse();
            FormsHierarchyRequest formsHierarchyRequest = new FormsHierarchyRequest();
            string rootFormId = GetStringSessionValue(UserSession.Key.RootFormId, defaultValue: null);
            string rootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId, defaultValue: null);
            if (rootFormId != null && rootResponseId != null)
            {
                formsHierarchyRequest.SurveyInfo.FormId = rootFormId;
                formsHierarchyRequest.SurveyResponseInfo.ResponseId = rootResponseId;
                formsHierarchyResponse = _surveyFacade.GetFormsHierarchy(formsHierarchyRequest);
            }
            return formsHierarchyResponse.FormsHierarchy;
        }

        private FormResponseInfoModel GetFormResponseInfoModels(string surveyId, string responseId, List<FormsHierarchyDTO> formsHierarchyDTOList = null)
        {
            FormResponseInfoModel formResponseInfoModel = new FormResponseInfoModel();

            var formHieratchyDTO = formsHierarchyDTOList.FirstOrDefault(h => h.FormId == surveyId);

            SurveyResponseBuilder surveyResponseBuilder = new SurveyResponseBuilder();
            if (!string.IsNullOrEmpty(surveyId))
            {
                SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
                FormSettingRequest formSettingRequest = new FormSettingRequest { ProjectId = GetStringSessionValue(UserSession.Key.ProjectId) };

                //Populating the request

                formSettingRequest.FormInfo.FormId = surveyId;
                formSettingRequest.FormInfo.UserId = GetIntSessionValue(UserSession.Key.UserId);
                //Getting Column Name  List
                FormSettingResponse formSettingResponse = _surveyFacade.GetFormSettings(formSettingRequest);
                _columns = formSettingResponse.FormSetting.ColumnNameList.ToList();
                _columns.Sort(Compare);

                // Setting  Column Name  List
                formResponseInfoModel.Columns = _columns;

                //Getting Resposes
                var ResponseListDTO = formsHierarchyDTOList.FirstOrDefault(x => x.FormId == surveyId).ResponseIds;

                // If we don't have any data for this child form yet then create a response 
                if (ResponseListDTO.Count == 0)
                {
                    var formResponseDetail = InitializeFormResponseDetail();
                    formResponseDetail.ParentResponseId = responseId;
                    formResponseDetail.ResponseId = Guid.NewGuid().ToString();
                    formResponseDetail.ResolveMetadataDependencies();

                    var surveyAnswerDTO = new SurveyAnswerDTO(formResponseDetail);
                    surveyAnswerDTO.CurrentPageNumber = 1;
                    ResponseListDTO.Add(surveyAnswerDTO);
                }

                //Setting Resposes List
                List<ResponseModel> ResponseList = new List<ResponseModel>();
                foreach (var item in ResponseListDTO)
                {
                    if (item.ParentResponseId == responseId)
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

                formResponseInfoModel.ResponsesList = ResponseList;

                formResponseInfoModel.PageSize = ReadPageSize();

                formResponseInfoModel.CurrentPage = 1;
            }
            return formResponseInfoModel;
        }

        private int ReadPageSize()
        {
            return Convert.ToInt16(WebConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"].ToString());
        }

        [HttpGet]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction(ViewActions.Index, ControllerNames.Login);
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CheckForConcurrency(String responseId)
        {
            ResponseContext responseContext = GetSetResponseContext(responseId);

            var surveyAnswerStateDTO = GetSurveyAnswerState(responseContext);
            SetSessionValue(UserSession.Key.EditResponseId, responseId);

            // Minimize the amount of Json data by serializing only pertinent state information
            var json = Json(surveyAnswerStateDTO);
            return json;
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Unlock(String responseId, bool recoverLastRecordVersion)
        {
            return UnlockResponse(responseId, recoverLastRecordVersion);
        }
    }
}
