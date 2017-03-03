using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Core.EnterInterpreter;
using Epi.DataPersistence.Constants;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Model;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;
using Epi.Common.EmailServices;

namespace Epi.Web.MVC.Controllers
{
    [Authorize]
    public class HomeController : BaseSurveyController
    {
        private readonly ISecurityFacade _securityFacade;
        private readonly Epi.Cloud.CacheServices.IEpiCloudCache _cacheServices;

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

                string surveyMode = "";
                Omniture omnitureObj = Epi.Web.MVC.Utility.OmnitureHelper.GetSettings(surveyMode, isMobileDevice);
                ViewBag.Omniture = omnitureObj;

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
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            string userName = Session[SessionKeys.UserName].ToString();
            Session[SessionKeys.FormValuesHasChanged] = "";

            if (string.IsNullOrEmpty(editForm) && Session[SessionKeys.EditForm] != null)
            {
                editForm = Session[SessionKeys.EditForm].ToString();
            }

            if (!string.IsNullOrEmpty(editForm) && string.IsNullOrEmpty(addNewFormId))
            {
                Session[SessionKeys.RootResponseId] = editForm;

                Session[SessionKeys.IsEditMode] = true;
                SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(editForm, Session[SessionKeys.RootFormId].ToString());


                Session[SessionKeys.RequestedViewId] = surveyAnswerDTO.ViewId;
                if (Session[SessionKeys.RecoverLastRecordVersion] != null)
                {
                    surveyAnswerDTO.RecoverLastRecordVersion = bool.Parse(Session[SessionKeys.RecoverLastRecordVersion].ToString());
                }
                string childRecordId = GetChildRecordId(surveyAnswerDTO);
                Session[SessionKeys.RecoverLastRecordVersion] = false;
                return RedirectToAction(ViewActions.Index, ControllerNames.Survey, new { responseid = childRecordId, PageNumber = 1, surveyid = surveyAnswerDTO.SurveyId, Edit = "Edit" });
            }
            else
            {
                Session[SessionKeys.IsEditMode] = false;
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
            Session[SessionKeys.RootResponseId] = responseId;

            int currentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());

            SurveyAnswerDTO surveyAnswer = _surveyFacade.CreateSurveyAnswer(addNewFormId, responseId.ToString(), userId, false, "", false, currentOrgId);
            surveyId = /*FromURL*/surveyId ?? surveyAnswer.SurveyId;

            // Initialize the Metadata Accessor
            MetadataAccessor.CurrentFormId = surveyId;

            MvcDynamicForms.Form form = _surveyFacade.GetSurveyFormData(surveyAnswer.SurveyId, 1, surveyAnswer, isMobileDevice);
            SurveyInfoModel surveyInfoModel = form.SurveyInfo.ToFormInfoModel();

            MetadataAccessor metadataAccessor = form.SurveyInfo as MetadataAccessor;

            // set the survey answer to be production or test 
            surveyAnswer.IsDraftMode = form.SurveyInfo.IsDraftMode;

            TempData[TempDataKeys.Width] = form.Width + 100;

            string checkcode = metadataAccessor.GetFormDigest(surveyId).CheckCode;
            form.FormCheckCodeObj = form.GetCheckCodeObj(metadataAccessor.GetFieldDigests(surveyId), surveyAnswer.ResponseDetail, checkcode);

            ///////////////////////////// Execute - Record Before - start//////////////////////
            Dictionary<string, string> contextDetailList = new Dictionary<string, string>();
            EnterRule functionObject_B = (EnterRule)form.FormCheckCodeObj.GetCommand("level=record&event=before&identifier=");
            SurveyResponseHelper surveyResponseHelper = new SurveyResponseHelper(_pageFields, _requiredList);
            if (functionObject_B != null && !functionObject_B.IsNull())
            {
                try
                {
                    PageDigest[] pageDigests = form.MetadataAccessor.GetCurrentFormPageDigests();
                    var responseDetail = surveyAnswer.ResponseDetail;

                    responseDetail = surveyResponseHelper.CreateResponseDocument(pageDigests);
                    Session[SessionKeys.RequiredList] = surveyResponseHelper.RequiredList;
                    _requiredList = surveyResponseHelper.RequiredList;
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

                surveyAnswer.ResponseDetail = surveyResponseHelper.CreateResponseDocument(pageDigestArray);

                _requiredList = surveyResponseHelper.RequiredList;
                Session[SessionKeys.RequiredList] = surveyResponseHelper.RequiredList;
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
            surveyAnswerDTO.ParentRecordId = surveyAnswerDTO.ResponseId;
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
				Session.Remove(SessionKeys.SortOrder);
				Session.Remove(SessionKeys.SortField);
				Session[SessionKeys.RootFormId] = formId;
				Session[SessionKeys.PageNumber] = page.Value;

			}

			//Code added to retain Search Ends. 

            var model = new FormResponseInfoModel();
            model = GetFormResponseInfoModel(formId, page.Value, sort, sortField, orgId);

            if (isMobileDevice == false)
            {
                return PartialView("ListResponses", model);
            }
            else
            {
                return View("ListResponses", model);
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
            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
            surveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = responseId });
            string Id = Session[SessionKeys.UserId].ToString();
            surveyAnswerRequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Id);
            surveyAnswerRequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
            surveyAnswerRequest.Criteria.SurveyId = Session[SessionKeys.RootFormId].ToString();
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
            // formReq.Criteria.UserId = UserId;
            //define filter criteria here.
            //define sorting criteria here.

            List<FormInfoModel> listOfFormsInfoModel = _surveyFacade.GetFormsInfoModelList(formReq);

            // return listOfFormsInfoModel.Where(x=>x.OrganizationId== OrgID).ToList();
            return listOfFormsInfoModel;
        }

        public FormResponseInfoModel GetFormResponseInfoModel(string surveyId, int pageNumber, string sort = "", string sortfield = "", int orgid = -1)
        {
            // Initialize the Metadata Accessor
            MetadataAccessor.CurrentFormId = /*FromURL*/surveyId;

            FormResponseInfoModel formResponseInfoModel = null;

            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            if (!string.IsNullOrEmpty(/*FromURL*/surveyId))
            {
                formResponseInfoModel = GetFormResponseInfoModel(/*FromURL*/surveyId, orgid, userId);
                FormSettingResponse formSettingResponse = formResponseInfoModel.FormSettingResponse;

                var surveyResponseHelper = new SurveyResponseHelper();

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
                SetUserRole(userId, orgid);

                SurveyAnswerRequest formResponseReq = new SurveyAnswerRequest();
                formResponseReq.Criteria.SurveyId = /*FromURL*/surveyId.ToString();
                formResponseReq.Criteria.PageNumber = pageNumber;
                formResponseReq.Criteria.UserId = userId;
                formResponseReq.Criteria.IsSqlProject = formSettingResponse.FormInfo.IsSQLProject;
                formResponseReq.Criteria.IsShareable = formSettingResponse.FormInfo.IsShareable;
                formResponseReq.Criteria.UserOrganizationId = orgid;

                Session[SessionKeys.IsSqlProject] = formSettingResponse.FormInfo.IsSQLProject;
                Session[SessionKeys.IsOwner] = formSettingResponse.FormInfo.IsOwner;
                //if (Session[SessionKeys.SearchCriteria] != null)
                //{
                //    formResponseInfoModel.SearchModel = (SearchBoxModel)Session[SessionKeys.SearchCriteria];
                //}
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
                //ResponseList = ResponseList.Skip((pageNumber - 1) * 20).Take(20).ToList();
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

                        // formResponseInfoModel.ResponsesList = ResponseListModel;
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

                        // formResponseInfoModel.ResponsesList = ResponseListModel.Skip((pageNumber - 1) * 20).Take(20).ToList();
                    }
                    formResponseInfoModel.ResponsesList = responseListModel.Skip((pageNumber - 1) * 20).Take(20).ToList();

                }
                if (string.IsNullOrEmpty(sort))
                {
                    formResponseInfoModel.ResponsesList = responseList.Skip((pageNumber - 1) * 20).Take(20).ToList();

                    //formResponseInfoModel.ResponsesList = ResponseList;
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

        private SurveyAnswerDTO GetSurveyAnswer(string responseId, string formId)
        {
            SurveyAnswerDTO result = null;
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            var surveyAnswerResponse = _surveyFacade.GetSurveyAnswerResponse(responseId, formId, userId);
            result = surveyAnswerResponse.SurveyResponseList[0];
            result.FormOwnerId = surveyAnswerResponse.FormInfo.OwnerId;
            return result;
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
            FormSettingRequest formSettingReq = new FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };
            List<KeyValuePair<int, string>> tempColumns = new List<KeyValuePair<int, string>>();
            //Get All forms
            List<FormsHierarchyDTO> formsHierarchy = GetFormsHierarchy(/*FromURL*/formId);
            List<SettingsInfoModel> modelList = new List<SettingsInfoModel>();
            foreach (var Item in formsHierarchy)
            {
                formSettingReq.GetMetadata = true;
                formSettingReq.FormInfo.FormId = new Guid(Item.FormId).ToString();
                formSettingReq.FormInfo.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
                formSettingReq.CurrentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());
                //Getting Column Name  List

                FormSettingResponse formSettingResponse = _surveyFacade.GetFormSettings(formSettingReq);
                //  FormSettingResponseList.Add(FormSettingResponse);

                // FormSettingResponse FormSettingResponse = _isurveyFacade.GetFormSettings(FormSettingReq);
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
                model.FormId = Item.FormId;
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
            return RedirectToActionPermanent("Index", "MetadataAdmin");
        }


        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CheckForConcurrency(String responseId)
        {
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            var surveyAnswerStateDTO = GetSurveyAnswerState(responseId);
            if (surveyAnswerStateDTO.DateCreated == DateTime.MinValue) surveyAnswerStateDTO.DateCreated = surveyAnswerStateDTO.DateUpdated;
            surveyAnswerStateDTO.LoggedInUserId = userId;
            Session[SessionKeys.EditForm] = responseId;

            // Minimize the amount of Json data by serializing only pertinent state information
            var json = Json(surveyAnswerStateDTO.ToSurveyAnswerDTO());
            return json;
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Notify(String responseId)
        {
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());

            //Get current user info
            int CurrentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());
            var UserInfo = _securityFacade.GetUserInfo(userId);
            //Get Organization admin info 
            var surveyAnswerDTO = GetSurveyAnswer(responseId, Session[SessionKeys.RootFormId].ToString());
            SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswerDTO.SurveyId);

            var OwnerInfo = _securityFacade.GetUserInfo(surveyAnswerDTO.FormOwnerId);

            var email = new Email();
            //ResponseId;

            email.Subject = "Record locked notification.";
            email.Body = " A user was unable to edit/delete a Epi Info™ Cloud Enter recored. \n \n Please login to Epi Info™ Cloud Enter system to Unlock this record.\n \n Below are the needed info to unlock the record.\n \n Response id: " + responseId + "\n\n User email: " + UserInfo.User.EmailAddress + "\n\n";
            email.From = ConfigurationManager.AppSettings["EMAIL_FROM"];
            email.To = new List<string>();
            email.To.Add(OwnerInfo.User.EmailAddress);

            var success = EmailHandler.SendMessage(email);

            return Json(1);
        }

        //Unlock
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Unlock(String ResponseId, bool RecoverLastRecordVersion = false)
        {
            try
            {
                SurveyAnswerRequest SurveyAnswerRequest = new SurveyAnswerRequest();
                SurveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = ResponseId });
                SurveyAnswerRequest.Criteria.StatusId = RecordStatus.Saved;
                SurveyAnswerRequest.Criteria.SurveyAnswerIdList.Add(ResponseId);
                Session[SessionKeys.RecoverLastRecordVersion] = RecoverLastRecordVersion;
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
            List<FormsHierarchyDTO> FormList = GetFormsHierarchy(formid);
            FormSettingRequest FormSettingReq = new FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            foreach (var Form in FormList)
            {
                FormSettingReq.GetMetadata = true;
                FormSettingReq.FormInfo.FormId = new Guid(formid).ToString();
                FormSettingReq.FormInfo.UserId = UserId;
                FormSettingDTO FormSetting = new FormSettingDTO();
                FormSetting.FormId = Form.FormId;
                FormSetting.ColumnNameList = GetDictionary(this.Request.Form["SelectedColumns_" + Form.FormId]);
                FormSetting.AssignedUserList = GetDictionary(this.Request.Form["SelectedUser"]);
                FormSetting.SelectedOrgList = GetDictionary(this.Request.Form["SelectedOrg"]);
                FormSetting.IsShareable = GetBoolValue(this.Request.Form["IsShareable"]);
                FormSetting.SelectedDataAccessRule = int.Parse(this.Request.Form["DataAccessRuleId"]);

                if (!string.IsNullOrEmpty(this.Request.Form["SoftDeleteForm"]) && this.Request.Form["SoftDeleteForm"].ToUpper() == "ON")
                {
                    FormSetting.IsDisabled = true;
                }
                if (!string.IsNullOrEmpty(this.Request.Form["RemoveTestData"]) && this.Request.Form["RemoveTestData"].ToUpper() == "ON")
                {
                    FormSetting.DeleteDraftData = true;
                }
                FormSettingReq.FormSetting.Add(FormSetting);
                FormSettingReq.FormInfo.IsDraftMode = GetBoolValue(this.Request.Form["Mode"]);

            }
            FormSettingResponse FormSettingResponse = _surveyFacade.SaveSettings(FormSettingReq);

            bool IsMobileDevice = this.Request.Browser.IsMobileDevice;

            var model = new FormResponseInfoModel();

            int CurrentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());
            model = GetFormResponseInfoModel(formid, 1, "", "", CurrentOrgId);

            if (IsMobileDevice == false)
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

        private Dictionary<int, string> GetDictionary(string List)
        {
            Dictionary<int, string> Dictionary = new Dictionary<int, string>();
            if (!string.IsNullOrEmpty(List))
            {
                Dictionary = List.Split(',').ToList().Select((s, i) => new { s, i }).ToDictionary(x => x.i, x => x.s);
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


        private List<FormsHierarchyDTO> GetFormsHierarchy(string formid)
        {
            FormsHierarchyResponse FormsHierarchyResponse = new FormsHierarchyResponse();
            FormsHierarchyRequest FormsHierarchyRequest = new FormsHierarchyRequest();

            FormsHierarchyRequest.SurveyInfo.FormId = formid;
            // FormsHierarchyRequest.SurveyResponseInfo.ResponseId = Session[SessionKeys.RootResponseId].ToString();
            FormsHierarchyResponse = _surveyFacade.GetFormsHierarchy(FormsHierarchyRequest);

            return FormsHierarchyResponse.FormsHierarchy;
        }
    }
}
