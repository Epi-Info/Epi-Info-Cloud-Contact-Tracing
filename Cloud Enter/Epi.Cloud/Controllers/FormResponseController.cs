using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Core.EnterInterpreter;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Model;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;

namespace Epi.Web.MVC.Controllers
{
    [Authorize]
    public class FormResponseController : BaseSurveyController
    {
        //
        // GET: /FormResponse/

        private IEnumerable<AbridgedFieldInfo> _pageFields;
        private string _requiredList = "";
        private string Sort, SortField;
        private bool IsNewRequest = true; //Added for retain search and sort


        public FormResponseController(ISurveyFacade isurveyFacade,
                                Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider projectMetadataProvider
)
        {
            _surveyFacade = isurveyFacade;
            _projectMetadataProvider = projectMetadataProvider;
        }



        [HttpGet]
        //string responseid,string SurveyId, int ViewId, string CurrentPage
        // View =0 Root form
        public ActionResult Index(string formid, string responseid, int Pagenumber = 1, int ViewId = 0)
        {
            bool Reset = false;
            bool.TryParse(Request.QueryString["reset"], out Reset);
            if (Reset)
            {
                Session[SessionKeys.SortOrder] = "";
                Session[SessionKeys.SortField] = "";
            }
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ViewBag.Version = version;
            bool IsAndroid = false;

            if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                IsAndroid = true;
            }
            if (ViewId == 0)
            {
                //Following code checks if request is for new or selected form.
                if (Session[SessionKeys.RootFormId] != null &&
                       Session[SessionKeys.RootFormId].ToString() == formid)
                {
                    IsNewRequest = false;
                }

                Session[SessionKeys.RootFormId] = formid;
                Session.Remove("RootResponseId");
                Session.Remove("FormValuesHasChanged");
                Session[SessionKeys.IsEditMode] = false;

                var model = new FormResponseInfoModel();
                model.ViewId = ViewId;
                model = GetSurveyResponseInfoModel(formid, Pagenumber, null, null, -1);
                Session[SessionKeys.SelectedOrgId] = model.FormInfoModel.OrganizationId;
                return View("Index", model);
            }
            else
            {
                List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();
                int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());

                bool IsMobileDevice = this.Request.Browser.IsMobileDevice;

                int RequestedViewId;
                RequestedViewId = ViewId;

                Session[SessionKeys.RequestedViewId] = RequestedViewId;
                SurveyModel SurveyModel = new SurveyModel();

                SurveyModel.RelateModel = FormsHierarchy.ToRelateModel(formid);
                SurveyModel.RequestedViewId = RequestedViewId;

                var RelateSurveyId = FormsHierarchy.Single(x => x.ViewId == ViewId);


                if (!string.IsNullOrEmpty(responseid))
                {
                    //SurveyModel.FormResponseInfoModel = GetFormResponseInfoModel(RelateSurveyId.FormId, responseid, Pagenumber, null, null);
                    SurveyModel.FormResponseInfoModel = GetFormResponseInfoModels(RelateSurveyId.FormId, responseid, FormsHierarchy);
                    SurveyModel.FormResponseInfoModel.NumberOfResponses = SurveyModel.FormResponseInfoModel.ResponsesList.Count();

                    SurveyModel.FormResponseInfoModel.ParentResponseId = responseid;
                }

                if (RelateSurveyId.ResponseIds.Count() > 0)
                {

                    SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(RelateSurveyId.ResponseIds[0].ResponseId);
                    var form = _surveyFacade.GetSurveyFormData(RelateSurveyId.ResponseIds[0].SurveyId, 1, surveyAnswerDTO, IsMobileDevice, null, null, IsAndroid);
                    SurveyModel.Form = form;
                    if (string.IsNullOrEmpty(responseid))
                    {
                        SurveyModel.FormResponseInfoModel = GetFormResponseInfoModels(RelateSurveyId.FormId, responseid, FormsHierarchy);
                        //SurveyModel.FormResponseInfoModel = GetFormResponseInfoModel(RelateSurveyId.FormId, RelateSurveyId.ResponseIds[0].RelateParentId, Pagenumber, null, null);
                        SurveyModel.FormResponseInfoModel.ParentResponseId = RelateSurveyId.ResponseIds[0].RelateParentId;

                    }

                    SurveyModel.FormResponseInfoModel.FormInfoModel.FormName = form.SurveyInfo.SurveyName;
                    SurveyModel.FormResponseInfoModel.FormInfoModel.FormId = form.SurveyInfo.SurveyId;
                    SurveyModel.FormResponseInfoModel.NumberOfResponses = SurveyModel.FormResponseInfoModel.ResponsesList.Count();


                }
                else
                {
                    FormResponseInfoModel ResponseInfoModel = new FormResponseInfoModel();
                    if (SurveyModel.FormResponseInfoModel.ResponsesList.Count() > 0)
                    {
                        SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(SurveyModel.FormResponseInfoModel.ResponsesList[0].Column0, RelateSurveyId.FormId);
                        ResponseInfoModel = GetFormResponseInfoModels(RelateSurveyId.FormId, responseid, FormsHierarchy);
                        //ResponseInfoModel = GetFormResponseInfoModel(RelateSurveyId.FormId, responseid, Pagenumber, null, null);
                        SurveyModel.Form = _surveyFacade.GetSurveyFormData(surveyAnswerDTO.SurveyId, 1, surveyAnswerDTO, IsMobileDevice, null, null, IsAndroid);
                        ResponseInfoModel.FormInfoModel.FormName = SurveyModel.Form.SurveyInfo.SurveyName.ToString();
                        ResponseInfoModel.FormInfoModel.FormId = SurveyModel.Form.SurveyInfo.SurveyId.ToString();
                        ResponseInfoModel.ParentResponseId = responseid;//SurveyModel.FormResponseInfoModel.ResponsesList[0].Column0;
                        ResponseInfoModel.NumberOfResponses = SurveyModel.FormResponseInfoModel.ResponsesList.Count();
                    }
                    else
                    {
                        var form1 = _surveyFacade.GetSurveyInfoModel(RelateSurveyId.FormId);
                        ResponseInfoModel.FormInfoModel.FormName = form1.SurveyName.ToString();
                        ResponseInfoModel.FormInfoModel.FormId = form1.SurveyId.ToString();
                        ResponseInfoModel.ParentResponseId = responseid;
                    }
                    SurveyModel.FormResponseInfoModel = ResponseInfoModel;

                }
                SurveyModel.FormResponseInfoModel.ViewId = ViewId;

                return View("Index", SurveyModel.FormResponseInfoModel);

            }
        }

        [HttpPost]
        public ActionResult Index(string surveyId, string AddNewFormId, string EditForm, string Cancel)
        {

            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            string UserName = Session[SessionKeys.UserName].ToString();
            bool isMobileDevice = this.Request.Browser.IsMobileDevice;
            FormsAuthentication.SetAuthCookie("BeginSurvey", false);
            bool isEditMode = false;
            if (isMobileDevice == true)
            {
                isMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
            }
            bool isAndroid = this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!string.IsNullOrEmpty(Cancel))
            {
                int pageNumber;
                int.TryParse(Cancel, out pageNumber);
                Dictionary<string, int> surveyPagesList = (Dictionary<string, int>)Session[SessionKeys.RelateButtonPageId];
                if (surveyPagesList != null)
                {
                    pageNumber = surveyPagesList[this.Request.Form["Parent_Response_Id"].ToString()];
                }

                return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = this.Request.Form["Parent_Response_Id"].ToString(), PageNumber = pageNumber });
            }
            if (string.IsNullOrEmpty(EditForm) && Session[SessionKeys.EditForm] != null && string.IsNullOrEmpty(AddNewFormId))
            {
                EditForm = Session[SessionKeys.EditForm].ToString();
            }

            var editFormResponseId = EditForm;

            if (!string.IsNullOrEmpty(editFormResponseId))
            {
                //Session[SessionKeys.RootFormId] = surveyid;
                if (Session[SessionKeys.RootResponseId] == null)
                {
                    Session[SessionKeys.RootResponseId] = editFormResponseId;
                }
                Session[SessionKeys.IsEditMode] = true;
                isEditMode = true;
                SurveyAnswerDTO surveyAnswer = GetSurveyAnswer(editFormResponseId);
                if (Session["RecoverLastRecordVersion"] != null)
                {
                    surveyAnswer.RecoverLastRecordVersion = bool.Parse(Session[SessionKeys.RecoverLastRecordVersion].ToString());
                }
                string ChildRecordId = GetChildRecordId(surveyAnswer);
                return RedirectToAction(Epi.Cloud.Common.Constants.Constant.INDEX, Epi.Cloud.Common.Constants.Constant.SURVEY_CONTROLLER, new { responseid = surveyAnswer.ParentRecordId, PageNumber = 1, Edit = "Edit" });
            }

            //create the responseid
            Guid responseID = Guid.NewGuid();
            if (Session[SessionKeys.RootResponseId] == null)
            {
                Session[SessionKeys.RootResponseId] = responseID;
            }
            TempData[Epi.Cloud.Common.Constants.Constant.RESPONSE_ID] = responseID.ToString();

            // create the first survey response
            SurveyAnswerDTO surveyAnswerDTO = _surveyFacade.CreateSurveyAnswer(AddNewFormId, responseID.ToString(), userId, true, this.Request.Form["Parent_Response_Id"].ToString(), isEditMode);
            List<FormsHierarchyDTO> formsHierarchy = GetFormsHierarchy();
            SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswerDTO.SurveyId, formsHierarchy);
            MetadataAccessor metadataAccessor = surveyInfoModel as MetadataAccessor;

            // set the survey answer to be production or test 
            surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;

            MvcDynamicForms.Form form = _surveyFacade.GetSurveyFormData(surveyAnswerDTO.SurveyId, 1, surveyAnswerDTO, isMobileDevice, null, formsHierarchy, isAndroid);

            TempData["Width"] = form.Width + 100;

            var formDigest = metadataAccessor.GetFormDigest(surveyAnswerDTO.SurveyId);
            string checkcode = formDigest.CheckCode;

            FormResponseDetail responseDetail = surveyAnswerDTO.ResponseDetail;

            form.FormCheckCodeObj = form.GetCheckCodeObj(MetadataAccessor.GetFieldDigests(surveyAnswerDTO.SurveyId), responseDetail, checkcode);

            ///////////////////////////// Execute - Record Before - start//////////////////////
            Dictionary<string, string> ContextDetailList = new Dictionary<string, string>();
            EnterRule FunctionObject_B = (EnterRule)form.FormCheckCodeObj.GetCommand("level=record&event=before&identifier=");
            SurveyResponseHelper surveyResponseDocDb = new SurveyResponseHelper(_pageFields, _requiredList);
            if (FunctionObject_B != null && !FunctionObject_B.IsNull())
            {
                try
                {
                    PageDigest[] pageDigests = form.MetadataAccessor.GetCurrentFormPageDigests();
                    responseDetail = surveyResponseDocDb.CreateResponseDocument(pageDigests);

                    Session[SessionKeys.RequiredList] = surveyResponseDocDb.RequiredList;
                    this._requiredList = surveyResponseDocDb.RequiredList;
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

                    _surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseID.ToString(), form, surveyAnswerDTO, false, false, 0, userId, UserName);
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

                surveyAnswerDTO.ResponseDetail = surveyResponseDocDb.CreateResponseDocument(pageDigestArray);

                this._requiredList = surveyResponseDocDb.RequiredList;
                Session[SessionKeys.RequiredList] = surveyResponseDocDb.RequiredList;
                form.RequiredFieldsList = _requiredList;
                // Session[SessionKeys.RequiredList] = _requiredList;
                _surveyFacade.UpdateSurveyResponse(surveyInfoModel, surveyAnswerDTO.ResponseId, form, surveyAnswerDTO, false, false, 0, userId, UserName);
            }

            surveyAnswerDTO = (SurveyAnswerDTO)formsHierarchy.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == surveyAnswerDTO.ResponseId);

            ///////////////////////////// Execute - Record Before - End//////////////////////
            return RedirectToAction(Epi.Cloud.Common.Constants.Constant.INDEX, Epi.Cloud.Common.Constants.Constant.SURVEY_CONTROLLER, new { responseid = responseID, PageNumber = 1 });
        }


        public FormResponseInfoModel GetSurveyResponseInfoModel(string surveyId, int pageNumber, string sort = "", string sortfield = "", int orgid = -1)
        {
            // Initialize the Metadata Accessor
            MetadataAccessor.CurrentFormId = surveyId;

            FormResponseInfoModel formResponseInfoModel = null;

            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
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

                SurveyAnswerRequest formResponseReq = new SurveyAnswerRequest();
                formResponseReq.Criteria.SurveyId = surveyId.ToString();
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

                if (sort != null && Sort.Length > 0)
                {
                    formResponseReq.Criteria.SortOrder = sort;
                }
                if (sortfield != null && SortField.Length > 0)
                {
                    formResponseReq.Criteria.Sortfield = sortfield;
                }
                formResponseReq.Criteria.SurveyQAList = Columns.ToDictionary(c => c.Key.ToString(), c => c.Value);
                formResponseReq.Criteria.FieldDigestList = formResponseInfoModel.ColumnDigests.ToDictionary(c => c.Key, c => c.Value);

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
                        ResponseList.Add(ConvertRowToModel(item, Columns, "GlobalRecordId"));
                    }
                    else
                    {
                        ResponseList.Add(item.ToResponseModel(Columns));
                    }
                }

                formResponseInfoModel.ResponsesList = ResponseList;
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

        private string CreateSearchCriteria(System.Collections.Specialized.NameValueCollection nameValueCollection, SearchBoxModel searchModel, FormResponseInfoModel model)
        {
            FormCollection Collection = new FormCollection(nameValueCollection);
            StringBuilder searchBuilder = new StringBuilder();
            //SortField = Collection[""];                   

            if (ValidateSearchFields(Collection))
            {

                if (Collection["col1"].Length > 0 && Collection["val1"].Length > 0)
                {
                    searchBuilder.Append(Collection["col1"] + "='" + Collection["val1"] + "'");
                    searchModel.SearchCol1 = Collection["col1"];
                    searchModel.Value1 = Collection["val1"];
                }
                if (Collection["col2"].Length > 0 && Collection["val2"].Length > 0)
                {
                    searchBuilder.Append(" AND " + Collection["col2"] + "='" + Collection["val2"] + "'");
                    searchModel.SearchCol2 = Collection["col2"];
                    searchModel.Value2 = Collection["val2"];
                }
                if (Collection["col3"].Length > 0 && Collection["val3"].Length > 0)
                {
                    searchBuilder.Append(" AND " + Collection["col3"] + "='" + Collection["val3"] + "'");
                    searchModel.SearchCol3 = Collection["col3"];
                    searchModel.Value3 = Collection["val3"];
                }
                if (Collection["col4"].Length > 0 && Collection["val4"].Length > 0)
                {
                    searchBuilder.Append(" AND " + Collection["col4"] + "='" + Collection["val4"] + "'");
                    searchModel.SearchCol4 = Collection["col4"];
                    searchModel.Value4 = Collection["val4"];
                }
                if (Collection["col5"].Length > 0 && Collection["val5"].Length > 0)
                {
                    searchBuilder.Append(" AND " + Collection["col5"] + "='" + Collection["val5"] + "'");
                    searchModel.SearchCol5 = Collection["col5"];
                    searchModel.Value5 = Collection["val5"];
                }
            }

            return searchBuilder.ToString();
        }

        private bool ValidateSearchFields(FormCollection collection)
        {
            if (string.IsNullOrEmpty(collection["col1"]) || collection["col1"] == "undefined" ||
               string.IsNullOrEmpty(collection["val1"]) || collection["val1"] == "undefined")
            {
                return false;
            }
            return true;
        }

        private void PopulateDropDownlist(out List<SelectListItem> searchColumns, string selectedValue, List<KeyValuePair<int, string>> columns)
        {
            searchColumns = new List<SelectListItem>();
            foreach (var item in columns)
            {
                SelectListItem newSelectListItem = new SelectListItem { Text = item.Value, Value = item.Value, Selected = item.Value == selectedValue };
                searchColumns.Add(newSelectListItem);
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
            surveyAnswerDTO.ParentRecordId = surveyAnswerDTO.ResponseId;
            surveyAnswerDTO.ResponseId = ChildId;
            surveyAnswerDTO.Status = RecordStatus.InProcess;
            surveyAnswerDTO.ReasonForStatusChange = RecordStatusChangeReason.CreateMulti;
            SurveyAnswerRequest.SurveyAnswerList.Add(surveyAnswerDTO);
            string result;

            //responseId = TempData[Epi.Web.MVC.Constants.Constant.RESPONSE_ID].ToString();
            string Id = Session[SessionKeys.UserId].ToString();
            SurveyAnswerRequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Id);//_UserId;
            SurveyAnswerRequest.RequestId = ChildId;
            SurveyAnswerRequest.Action = "CreateMulti";
            SurveyAnswerResponse = _surveyFacade.SetChildRecord(SurveyAnswerRequest);
            result = SurveyAnswerResponse.SurveyResponseList[0].ResponseId.ToString();
            return result;
        }

        private SurveyAnswerDTO GetSurveyAnswer(string responseId, string formid = "")
        {
            SurveyAnswerDTO result = null;

            string FormId = Session[SessionKeys.RootFormId].ToString();
            string Id = Session[SessionKeys.UserId].ToString();
            if (string.IsNullOrEmpty(formid))
            {
                result = _surveyFacade.GetSurveyAnswerResponse(responseId, FormId, SurveyHelper.GetDecryptUserId(Id)).SurveyResponseList[0];
            }
            else
            {
                result = _surveyFacade.GetSurveyAnswerResponse(responseId, formid, SurveyHelper.GetDecryptUserId(Id)).SurveyResponseList[0];
            }
            return result;
        }


        /// <summary>
        /// Following Action method takes ResponseId as a parameter and deletes the response.
        /// For now it returns nothing as a confirmation of deletion, we may add some error/success
        /// messages later. TBD
        /// </summary>
        /// <param name="ResponseId"></param>
        /// <returns></returns>
        [HttpPost]
        //public ActionResult Delete(string ResponseId, string surveyid)
        //{
        //    SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
        //    surveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = ResponseId });
        //    string Id = Session[SessionKeys.UserId].ToString();
        //    surveyAnswerRequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Id);
        //    surveyAnswerRequest.Criteria.IsEditMode = false;
        //    surveyAnswerRequest.Criteria.IsDeleteMode = false;
        //    surveyAnswerRequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
        //    surveyAnswerRequest.Criteria.SurveyId = Session[SessionKeys.RootFormId].ToString();
        //    surveyAnswerRequest.Action = "Delete";
        //    SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(surveyAnswerRequest);

        //    return Json(surveyid);
        //}

        //Mobile record Delete
        public ActionResult Delete(string responseId)
        {
            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
            surveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = responseId });
            string Id = Session[SessionKeys.UserId].ToString();
            surveyAnswerRequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Id);
            surveyAnswerRequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
            surveyAnswerRequest.Criteria.SurveyId = Session[SessionKeys.RootFormId].ToString();
            surveyAnswerRequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteResponse;
            surveyAnswerRequest.Criteria.StatusChangeReason = RecordStatusChangeReason.DeleteResponse;
            surveyAnswerRequest.Action = "Delete";
            SurveyAnswerResponse surveyAnswerResponse = _surveyFacade.DeleteResponse(surveyAnswerRequest);
            return Json(string.Empty);
        }

        [HttpPost]
        public ActionResult DeleteBranch(string ResponseId)//List<FormInfoModel> ModelList, string formid)
        {

            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
            surveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = ResponseId });
            surveyAnswerRequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            surveyAnswerRequest.Criteria.IsEditMode = false;
            surveyAnswerRequest.Criteria.IsDeleteMode = false;
            surveyAnswerRequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
            surveyAnswerRequest.Criteria.SurveyId = Session[SessionKeys.RootFormId].ToString();
            SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(surveyAnswerRequest);

            return Json(string.Empty);//string.Empty
                                      //return RedirectToAction("Index", "Home");
        }

        private List<FormsHierarchyDTO> GetFormsHierarchy()
        {
            FormsHierarchyResponse formsHierarchyResponse = new FormsHierarchyResponse();
            FormsHierarchyRequest formsHierarchyRequest = new FormsHierarchyRequest();
            if (Session[SessionKeys.RootFormId] != null && Session[SessionKeys.RootResponseId] != null)
            {
                formsHierarchyRequest.SurveyInfo.FormId = Session[SessionKeys.RootFormId].ToString();
                formsHierarchyRequest.SurveyResponseInfo.ResponseId = Session[SessionKeys.RootResponseId].ToString();
                formsHierarchyResponse = _surveyFacade.GetFormsHierarchy(formsHierarchyRequest);
            }
            return formsHierarchyResponse.FormsHierarchy;
        }

        private FormResponseInfoModel GetFormResponseInfoModels(string SurveyId, string ResponseId, List<FormsHierarchyDTO> FormsHierarchyDTOList = null)
        {
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            FormResponseInfoModel FormResponseInfoModel = new FormResponseInfoModel();

            var formHieratchyDTO = FormsHierarchyDTOList.FirstOrDefault(h => h.FormId == SurveyId);

            SurveyResponseHelper surveyResponseHelper = new SurveyResponseHelper();
            if (!string.IsNullOrEmpty(SurveyId))
            {
                SurveyAnswerRequest FormResponseReq = new SurveyAnswerRequest();
                FormSettingRequest FormSettingReq = new FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };

                //Populating the request

                FormSettingReq.FormInfo.FormId = SurveyId;
                FormSettingReq.FormInfo.UserId = UserId;
                //Getting Column Name  List
                FormSettingResponse FormSettingResponse = _surveyFacade.GetFormSettings(FormSettingReq);
                Columns = FormSettingResponse.FormSetting.ColumnNameList.ToList();
                Columns.Sort(Compare);

                // Setting  Column Name  List
                FormResponseInfoModel.Columns = Columns;

                //Getting Resposes
                var ResponseListDTO = FormsHierarchyDTOList.FirstOrDefault(x => x.FormId == SurveyId).ResponseIds;

                // If we don't have any data for this child form yet then create a response 
                if (ResponseListDTO.Count == 0)
                {
                    var surveyAnswerDTO = new SurveyAnswerDTO();
                    surveyAnswerDTO.CurrentPageNumber = 1;
                    surveyAnswerDTO.DateUpdated = DateTime.UtcNow;
                    surveyAnswerDTO.RelateParentId = ResponseId;
                    surveyAnswerDTO.ResponseId = Guid.NewGuid().ToString();
                    surveyAnswerDTO.ResponseDetail = new FormResponseDetail();
                    ResponseListDTO.Add(surveyAnswerDTO);
                }

                //Setting Resposes List
                List<ResponseModel> ResponseList = new List<ResponseModel>();
                foreach (var item in ResponseListDTO)
                {
                    if (item.RelateParentId == ResponseId)
                    {
                        if (item.SqlData != null)
                        {
                            ResponseList.Add(ConvertRowToModel(item, Columns, "ChildGlobalRecordID"));
                        }
                        else
                        {
                            ResponseList.Add(item.ToResponseModel(Columns));
                        }
                    }
                }

                FormResponseInfoModel.ResponsesList = ResponseList;

                FormResponseInfoModel.PageSize = ReadPageSize();

                FormResponseInfoModel.CurrentPage = 1;
            }
            return FormResponseInfoModel;
        }

        private FormResponseInfoModel GetFormResponseInfoModel(string surveyId, string responseId, int pageNumber, string sort = "", string sortfield = "", int orgid = -1)
        {
            // Initialize the Metadata Accessor
            MetadataAccessor.CurrentFormId = surveyId;

            FormResponseInfoModel formResponseInfoModel = null;

            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
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
                // SetUserRole(userId, orgid);

                SurveyAnswerRequest formResponseReq = new SurveyAnswerRequest();
                formResponseReq.Criteria.SurveyId = surveyId.ToString();
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

                //if (sort.Length > 0)
                //{
                //    formResponseReq.Criteria.SortOrder = sort;
                //}
                //if (sortfield.Length > 0)
                //{
                //    formResponseReq.Criteria.Sortfield = sortfield;
                //}

                if (Sort != null && Sort.Length > 0)
                {
                    formResponseReq.Criteria.SortOrder = Sort;
                }

                if (SortField != null && SortField.Length > 0)
                {
                    formResponseReq.Criteria.Sortfield = SortField;
                }

                formResponseReq.Criteria.SurveyQAList = Columns.ToDictionary(c => c.Key.ToString(), c => c.Value);
                formResponseReq.Criteria.FieldDigestList = formResponseInfoModel.ColumnDigests.ToDictionary(c => c.Key, c => c.Value);
                
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
                        ResponseList.Add(ConvertRowToModel(item, Columns, "GlobalRecordId"));
                    }
                    else
                    {
                        ResponseList.Add(item.ToResponseModel(Columns));
                    }
                }
                formResponseInfoModel.ResponsesList = ResponseList;
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

        private int ReadPageSize()
        {
            return Convert.ToInt16(WebConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"].ToString());
        }

        [HttpGet]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            this.Session.Clear();
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CheckForConcurrency(String responseId)
        {
            int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            var surveyAnswerStateDTO = GetSurveyAnswerState(responseId, Session[SessionKeys.RootFormId].ToString());
            surveyAnswerStateDTO.LoggedInUserId = userId;
            Session[SessionKeys.EditForm] = responseId;

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
                SurveyAnswerRequest.Criteria.StatusId = 2;
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

        public void SetRelateSession(string ResponseId, int CurrentPage)
        {
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


