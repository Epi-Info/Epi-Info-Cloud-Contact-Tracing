using System;
using System.Collections.Generic;
using System.Configuration;
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
using Epi.Cloud.MVC.Extensions;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Common.Core.DataStructures;
using Epi.Common.EmailServices;
using Epi.Core.EnterInterpreter;
using Epi.DataPersistence.Constants;
using Epi.FormMetadata.DataStructures;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;

namespace Epi.Web.MVC.Controllers
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
                              ISurveyResponseDao surveyResponseDao)
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

            Session.Remove(SessionKeys.ResponseContext);
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            int orgnizationId;
            Session[SessionKeys.EditForm] = null;

            Guid userIdGuid = new Guid();
            try
            {

                FormModel formModel = GetFormModel(surveyId, userId, userIdGuid, out orgnizationId);

                if (orgId == -1)
                {
                    Session[SessionKeys.SelectedOrgId] = orgnizationId;
                }
                else
                {
                    Session[SessionKeys.SelectedOrgId] = orgId;
                }
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(\r\n|\r|\n)+");



                bool isMobileDevice = false;
                isMobileDevice = this.Request.Browser.IsMobileDevice;
                if (isMobileDevice) // Because mobile doesn't need RootFormId until button click. 
                {
                    Session[SessionKeys.RootFormId] = null;
                    Session[SessionKeys.PageNumber] = null;
                    Session[SessionKeys.SortOrder] = null;
                    Session[SessionKeys.SortField] = null;
                    Session[SessionKeys.SearchCriteria] = null;
                    Session[SessionKeys.SearchModel] = null;
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
            formModel.UserHighestRole = int.Parse(Session[SessionKeys.UserHighestRole].ToString());
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

            formModel.UserFirstName = Session[SessionKeys.UserFirstName].ToString();
            formModel.UserLastName = Session[SessionKeys.UserLastName].ToString();
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

            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            string userName = Session[SessionKeys.UserName].ToString();
            Session[SessionKeys.FormValuesHasChanged] = "";

            if (string.IsNullOrEmpty(editForm) && Session[SessionKeys.EditForm] != null)
            {
                editForm = Session[SessionKeys.EditForm].ToString();
            }

            var editResponseId = editForm;

            if (!string.IsNullOrEmpty(editForm) && string.IsNullOrEmpty(addNewFormId))
            {
                Session[SessionKeys.RootResponseId] = editForm;

                Session[SessionKeys.IsEditMode] = true;
                isNewRecord = false;
                SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(editForm, Session[SessionKeys.RootFormId].ToString());


                Session[SessionKeys.RequestedViewId] = surveyAnswerDTO.ViewId;
                if (Session[SessionKeys.RecoverLastRecordVersion] != null)
                {
                    surveyAnswerDTO.RecoverLastRecordVersion = bool.Parse(Session[SessionKeys.RecoverLastRecordVersion].ToString());
                }
                //string childRecordId = GetChildRecordId(surveyAnswerDTO);
                Session[SessionKeys.RecoverLastRecordVersion] = false;
                return RedirectToAction(ViewActions.Index, ControllerNames.Survey, new { responseid = editResponseId, PageNumber = 1, surveyid = surveyAnswerDTO.SurveyId, Edit = "Edit" });
            }
            else
            {
                Session[SessionKeys.IsEditMode] = false;
                isNewRecord = true;
            }
            bool isMobileDevice = this.Request.Browser.IsMobileDevice;


            if (isMobileDevice == false)
            {
                isMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }

            FormsAuthentication.SetAuthCookie("BeginSurvey", false);

            //create the responseid
            Guid responseId = Guid.NewGuid();
            TempData[TempDataKeys.ResponseId] = responseId.ToString();

            // create the first survey response
            // Epi.Cloud.Common.DTO.SurveyAnswerDTO SurveyAnswer = _isurveyFacade.CreateSurveyAnswer(surveyModel.SurveyId, ResponseID.ToString());
            Session[SessionKeys.RootFormId] = addNewFormId;
            var rootResponseId = responseId;
            Session[SessionKeys.RootResponseId] = rootResponseId;

            var responseContext = new ResponseContext
            {
                FormId = addNewFormId,
                ResponseId = responseId.ToString(),
                IsNewRecord = isNewRecord,
                UserId = userId,
                UserName = userName
            }.ResolveMetadataDependencies() as ResponseContext;

            int currentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());

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
                    Session[SessionKeys.RequiredList] = surveyResponseBuilder.RequiredList;
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


                    contextDetailList = Epi.Web.MVC.Utility.SurveyHelper.GetContextDetailList(functionObject_B);
                    form = Epi.Web.MVC.Utility.SurveyHelper.UpdateControlsValuesFromContext(form, contextDetailList);

                    _surveyFacade.UpdateSurveyResponse(surveyInfoModel,
                                                        responseId.ToString(),
                                                        form,
                                                        surveyAnswer,
                                                        false,
                                                        false,
                                                        0,
                                                        SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()), userName);
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
                Session[SessionKeys.RequiredList] = surveyResponseBuilder.RequiredList;
                form.RequiredFieldsList = _requiredList;
                //_surveyFacade.UpdateSurveyResponse(surveyInfoModel, surveyAnswer.ResponseId, form, surveyAnswer, false, false, 0, SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()));
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
            string userId = Session[SessionKeys.UserId].ToString();
            surveyAnswerRequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(userId);
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

            Session[SessionKeys.RootFormId] = formid;
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

            bool isMobileDevice = this.Request.Browser.IsMobileDevice;

            //Code added to retain Search Starts

            if (reset)
            {
                Session[SessionKeys.SortOrder] = "";
                Session[SessionKeys.SortField] = "";
			}

            if (Session[SessionKeys.ProjectId] == null)
            {
                // This will prime the cache if the project is not already loaded into cache.
                Session[SessionKeys.ProjectId] = _projectMetadataProvider.GetProjectId_RetrieveProjectIfNecessary();
            }

            Session[SessionKeys.SelectedOrgId] = orgId;
            if (Session[SessionKeys.RootFormId] != null && Session[SessionKeys.RootFormId].ToString() == formId)
            {
                if (Session[SessionKeys.SortOrder] != null &&
                    !string.IsNullOrEmpty(Session[SessionKeys.SortOrder].ToString()) &&
                    string.IsNullOrEmpty(sort))
                {
                    sort = Session[SessionKeys.SortOrder].ToString();
                }

                if (Session[SessionKeys.SortField] != null &&
                    !string.IsNullOrEmpty(Session[SessionKeys.SortField].ToString()) &&
                    string.IsNullOrEmpty(sortField))
                {
                    sortField = Session[SessionKeys.SortField].ToString();
                }

				Session[SessionKeys.SortOrder] = sort;
				Session[SessionKeys.SortField] = sortField;
				Session[SessionKeys.PageNumber] = page.Value;
			}
			else
			{
                ResponseContext responseContext = new ResponseContext { FormId = formId, RootFormId = formId };
                Session[SessionKeys.ResponseContext] = responseContext;

				Session.Remove(SessionKeys.SortOrder);
				Session.Remove(SessionKeys.SortField);
				Session[SessionKeys.RootFormId] = formId;
				Session[SessionKeys.PageNumber] = page.Value;

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
			Session[SessionKeys.SortOrder] = null;
			Session[SessionKeys.SortField] = null;
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
            var rootFormId = Session[SessionKeys.RootFormId].ToString();
            string encryptedUserId = Session[SessionKeys.UserId].ToString();
            int userId = SurveyHelper.GetDecryptUserId(encryptedUserId);

            var responseContext = new ResponseContext
            {
                ResponseId = responseId,
                RootResponseId = responseId,
                FormId = rootFormId,
                UserId = userId
            }.ResolveMetadataDependencies() as ResponseContext;

            SurveyAnswerRequest surveyAnswerRequest = responseContext.ToSurveyAnswerRequest();
            surveyAnswerRequest.SurveyAnswerList.Add(responseContext.ToSurveyAnswerDTO());
            surveyAnswerRequest.Criteria.UserId = userId;
            surveyAnswerRequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
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

            formReq.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());//Hard coded user for now.
            formReq.Criteria.CurrentOrgId = OrgID;

            List<FormInfoModel> listOfFormsInfoModel = _surveyFacade.GetFormsInfoModelList(formReq);

            return listOfFormsInfoModel;
        }

        public FormResponseInfoModel GetFormResponseInfoModel(string surveyId, int pageNumber, string sort = "", string sortfield = "", int orgid = -1)
        {
            // Initialize the Metadata Accessor
            MetadataAccessor.CurrentFormId = /*FromURL*/surveyId;

            FormResponseInfoModel formResponseInfoModel = null;

            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            string userName = Session[SessionKeys.UserName].ToString();
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
                    UserId = userId,
                    UserName = userName
                }.ResolveMetadataDependencies();

                SurveyAnswerRequest formResponseReq = new SurveyAnswerRequest { ResponseContext = responseContext };
                formResponseReq.Criteria.SurveyId = /*FromURL*/surveyId.ToString();
                formResponseReq.Criteria.PageNumber = pageNumber;
                formResponseReq.Criteria.UserId = userId;
                formResponseReq.Criteria.IsSqlProject = formSettingResponse.FormInfo.IsSQLProject;
                formResponseReq.Criteria.IsShareable = formSettingResponse.FormInfo.IsShareable;
                formResponseReq.Criteria.UserOrganizationId = orgid;

                Session[SessionKeys.IsSqlProject] = formSettingResponse.FormInfo.IsSQLProject;
                Session[SessionKeys.IsOwner] = formSettingResponse.FormInfo.IsOwner;

                // Following code retain search starts
                if (Session[SessionKeys.SearchCriteria] != null &&
                    !string.IsNullOrEmpty(Session[SessionKeys.SearchCriteria].ToString()) &&
                    (Request.QueryString["col1"] == null || Request.QueryString["col1"] == "undefined"))
                {
                    formResponseReq.Criteria.SearchCriteria = Session[SessionKeys.SearchCriteria].ToString();
                    formResponseInfoModel.SearchModel = (SearchBoxModel)Session[SessionKeys.SearchModel];
                }
                else
                {
                    formResponseReq.Criteria.SearchCriteria = CreateSearchCriteria(Request.QueryString, formResponseInfoModel.SearchModel, formResponseInfoModel);
                    Session[SessionKeys.SearchModel] = formResponseInfoModel.SearchModel;
                    Session[SessionKeys.SearchCriteria] = formResponseReq.Criteria.SearchCriteria;
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
                        if (column.Value == sortfield)
                        {
                            sortFieldcolumn = "Column" + column.Key;
                        }
                    }
                }

                var sortList = responseList;
                sortfield = sortfield.ToLower();
                if (!string.IsNullOrEmpty(sortfield))
                {
                    if(sort != "ASC")
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
                Session[SessionKeys.UsertRole] = userResponse.User[0].Role;
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
            this.Session.Clear();
            return RedirectToAction(ViewActions.Index, ControllerNames.Login);
        }

        [HttpGet]
        public ActionResult GetSettings(string formId)//List<FormInfoModel> ModelList, string formid)
        {
            var projectId = Session[SessionKeys.ProjectId].ToString();
            var userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            var currentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());

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

                _columns = formSettingResponse.FormSetting.AssignedUserList.ToList();
                if (_columns.Exists(col => col.Value == Session[SessionKeys.UserEmailAddress].ToString()))
                {
                    _columns.Remove(_columns.First(u => u.Value == Session[SessionKeys.UserEmailAddress].ToString()));
                }

                //Columns.Sort(Compare);

                Dictionary<int, string> dictionary2 = _columns.ToDictionary(pair => pair.Key, pair => pair.Value);

                model.AssignedUserList = dictionary2;

                _columns = formSettingResponse.FormSetting.UserList.ToList();

                if (_columns.Exists(col => col.Value == Session[SessionKeys.UserEmailAddress].ToString()))
                {
                    _columns.Remove(_columns.First(u => u.Value == Session[SessionKeys.UserEmailAddress].ToString()));
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
            surveyAnswerDTO.LoggedInUserId = responseContext.UserId;
            Session[SessionKeys.EditForm] = responseId;

            var json = Json(surveyAnswerDTO.ToSurveyAnswerDTO());
            return json;
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Notify(String responseId)
        {
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());

            //Get current user info
            int currentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());
            var userInfo = _securityFacade.GetUserInfo(userId);
            //Get Organization admin info 
            var surveyAnswerDTO = GetSurveyAnswer(responseId, Session[SessionKeys.RootFormId].ToString());
            SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswerDTO.SurveyId);

            var OwnerInfo = _securityFacade.GetUserInfo(surveyAnswerDTO.FormOwnerId);

            var email = new Email();

            email.To = new List<string>();
            email.To.Add(OwnerInfo.User.EmailAddress);
            email.From = ConfigurationManager.AppSettings["EMAIL_FROM"];
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
                Session[SessionKeys.RecoverLastRecordVersion] = recoverLastRecordVersion;
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
            FormSettingRequest formSettingReq = new FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
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

            int currentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());
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
            // FormsHierarchyRequest.SurveyResponseInfo.ResponseId = Session[SessionKeys.RootResponseId].ToString();
            formsHierarchyResponse = _surveyFacade.GetFormsHierarchy(formsHierarchyRequest);

            return formsHierarchyResponse.FormsHierarchy;
        }
    }
}
