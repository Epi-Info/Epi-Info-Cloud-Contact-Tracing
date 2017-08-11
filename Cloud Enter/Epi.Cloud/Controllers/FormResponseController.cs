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
            bool.TryParse(Request.QueryString["reset"], out reset);
            if (reset)
            {
                RemoveSessionValue(UserSession.Key.SortOrder);
                RemoveSessionValue(UserSession.Key.SortField);
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
                model = GetSurveyResponseInfoModel(formid, pagenumber, null, null, currentOrgId);
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
            if (string.IsNullOrEmpty(/*FromURL*/editForm) && string.IsNullOrEmpty(/*FromURL*/addNewFormId) && !IsSessionValueNull(UserSession.Key.EditForm))
            {
                editForm = GetStringSessionValue(UserSession.Key.EditForm);
            }

            var editFormResponseId = /*FromURL*/editForm;

            if (!string.IsNullOrEmpty(editFormResponseId))
            {
                if (IsSessionValueNull(UserSession.Key.RootResponseId))
                {
                    SetSessionValue(UserSession.Key.RootResponseId, editFormResponseId);
                }

                isEditMode = true;
                SetSessionValue(UserSession.Key.IsEditMode, isEditMode);

                SurveyAnswerDTO surveyAnswer = GetSurveyAnswer(editFormResponseId, GetStringSessionValue(UserSession.Key.RootFormId));
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

            var responseContext = new ResponseContext
            {
                FormId = /*FromURL*/addNewFormId,
                ResponseId = responseId.ToString(),
                ParentResponseId = this.Request.Form["Parent_Response_Id"].ToString(),
                RootResponseId = rootResponseId,
                IsNewRecord = !isEditMode,
                UserOrgId = orgId,
                UserId = userId,
                UserName = userName
            }.ResolveMetadataDependencies() as ResponseContext;

            // create the first survey response
            SurveyAnswerDTO surveyAnswerDTO = _surveyFacade.CreateSurveyAnswer(responseContext, isEditMode);
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

            int userId = GetIntSessionValue(UserSession.Key.UserId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);

            if (!string.IsNullOrEmpty(surveyId))
            {
                formResponseInfoModel = GetFormResponseInfoModel(surveyId, orgid, userId);
                FormSettingResponse formSettingResponse = formResponseInfoModel.FormSettingResponse;
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

                var responseContext = new ResponseContext
                {
                    RootFormId = surveyId,
                    UserOrgId = orgid,
                    UserId = userId,
                    UserName = userName
                }.ResolveMetadataDependencies();

                SurveyAnswerRequest formResponseReq = new SurveyAnswerRequest { ResponseContext = responseContext };
                formResponseReq.Criteria.SurveyId = surveyId.ToString();
                formResponseReq.Criteria.PageNumber = /*FromURL*/pageNumber;
                formResponseReq.Criteria.UserId = userId;
                formResponseReq.Criteria.IsSqlProject = formSettingResponse.FormInfo.IsSQLProject;
                formResponseReq.Criteria.IsShareable = formSettingResponse.FormInfo.IsShareable;
                formResponseReq.Criteria.DataAccessRuleId = formSettingResponse.FormSetting.SelectedDataAccessRule;
                formResponseReq.Criteria.IsMobile = true;
                formResponseReq.Criteria.UserOrganizationId = orgid;

                SetSessionValue(UserSession.Key.IsSqlProject, formSettingResponse.FormInfo.IsSQLProject);
                SetSessionValue(UserSession.Key.IsOwner, formSettingResponse.FormInfo.IsOwner);

                // Following code retain search starts
                string searchCriteria = GetStringSessionValue(UserSession.Key.SearchCriteria, defaultValue: null);
                if (!string.IsNullOrEmpty(searchCriteria) &&
                    (Request.QueryString["col1"] == null || Request.QueryString["col1"] == "undefined"))
                {
                    formResponseReq.Criteria.SearchCriteria = searchCriteria;
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

                if (!string.IsNullOrWhiteSpace(/*FromURL*/sort))
                {
                    formResponseReq.Criteria.SortOrder = sort;
                }
                if (!string.IsNullOrWhiteSpace(/*FromURL*/sortfield))
                {
                    formResponseReq.Criteria.Sortfield = /*FromURL*/sortfield;
                }
                formResponseReq.Criteria.SurveyQAList = _columns.ToDictionary(c => c.Key.ToString(), c => c.Value);
                formResponseReq.Criteria.FieldDigestList = formResponseInfoModel.ColumnDigests.ToDictionary(c => c.Key, c => c.Value);
                formResponseReq.Criteria.SearchDigestList = ToSearchDigestList(formResponseInfoModel.SearchModel, surveyId);


                SurveyAnswerResponse formResponseList = _surveyFacade.GetFormResponseList(formResponseReq);

                //foreach (var item in formResponseList.SurveyResponseList)
                //{
                //   SurveyAnswerDTO surveyAnswer = new SurveyAnswerDTO();
                //   surveyAnswer.IsLocked = false;
                //   surveyAnswer.ResponseId = item.ResponseId;
                //   //var pageResponseDetail = surveyAnswer.ResponseDetail.PageResponseDetailList.Where(p => p.PageNumber == criteria.PageNumber).SingleOrDefault();
                //   //if (pageResponseDetail == null)
                //   //{
                //   //    pageResponseDetail = new Cloud.Common.EntityObjects.PageResponseDetail() { PageNumber = criteria.PageNumber };
                //   //    surveyAnswer.ResponseDetail.AddPageResponseDetail(pageResponseDetail);
                //   //}
                //   //pageResponseDetail.ResponseQA = item.ResponseDetail != null ? item.ResponseDetail.FlattenedResponseQA() : new Dictionary<string, string>();
                //   surveyAnswer.ResponseDetail = item.ResponseDetail;
                //   formResponseList.SurveyResponseList.Add(surveyAnswer);
                //}

                //var ResponseTableList ; //= FormSettingResponse.FormSetting.DataRows;
                //Setting Resposes List
                List<ResponseModel> ResponseList = new List<ResponseModel>();
                foreach (var item in formResponseList.SurveyResponseList)
                {
                    if (item.SqlData != null)
                    {
                        ResponseList.Add(ConvertRowToModel(item, _columns, "GlobalRecordId"));
                    }
                    else
                    {
                        ResponseList.Add(item.ToResponseModel(_columns));
                    }
                }

                formResponseInfoModel.ResponsesList = ResponseList;
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
            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
            surveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = responseId });
            surveyAnswerRequest.Criteria.UserId = GetIntSessionValue(UserSession.Key.UserId); ;
            surveyAnswerRequest.Criteria.IsSqlProject = GetBoolSessionValue(UserSession.Key.IsSqlProject);
            surveyAnswerRequest.Criteria.SurveyId = GetStringSessionValue(UserSession.Key.RootFormId);
            surveyAnswerRequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteResponse;
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
                    var surveyAnswerDTO = new SurveyAnswerDTO();
                    surveyAnswerDTO.CurrentPageNumber = 1;
                    surveyAnswerDTO.DateUpdated = DateTime.UtcNow;
                    surveyAnswerDTO.ParentResponseId = responseId;
                    surveyAnswerDTO.ResponseId = Guid.NewGuid().ToString();
                    surveyAnswerDTO.ResponseDetail = new FormResponseDetail();
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
            surveyAnswerStateDTO.LoggedInUserOrgId = responseContext.UserOrgId;
            surveyAnswerStateDTO.LoggedInUserId = responseContext.UserId;
            SetSessionValue(UserSession.Key.EditForm, responseId);

            // Minimize the amount of Json data by serializing only pertinent state information
            var json = Json(surveyAnswerStateDTO.ToSurveyAnswerDTO());
            return json;
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Unlock(String ResponseId, bool RecoverLastRecordVersion)
        {
            try
            {
                SurveyAnswerRequest SurveyAnswerRequest = new SurveyAnswerRequest();
                SurveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = ResponseId });
                SurveyAnswerRequest.Criteria.StatusId = RecordStatus.Saved;
                SetSessionValue(UserSession.Key.RecoverLastRecordVersion, RecoverLastRecordVersion);
            }
            catch (Exception ex)
            {
                return Json("Erorr");
            }
            return Json("Success");
        }
    }
}
