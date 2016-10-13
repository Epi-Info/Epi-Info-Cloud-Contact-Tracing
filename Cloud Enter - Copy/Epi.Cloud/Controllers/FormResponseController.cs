using System;
using System.Web.Mvc;
using Epi.Web.MVC.Models;
using System.Linq;
using Epi.Core.EnterInterpreter;
using System.Collections.Generic;
using System.Web.Security;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Utility;
using Epi.Web.Enter.Common.DTO;
using System.Web.Configuration;
using System.Text;
using Epi.Web.MVC.Constants;
using System.Reflection;
using Epi.Cloud.DataEntryServices.Model;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.MVC.Extensions;
using Epi.FormMetadata.DataStructures;
using Epi.DataPersistence.DataStructures;

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


		public FormResponseController(Epi.Web.MVC.Facade.ISurveyFacade isurveyFacade,
                                      Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider projectMetadataProvider
)
		{
			_isurveyFacade = isurveyFacade;
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
				model = GetSurveyResponseInfoModel(formid, Pagenumber);
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

				SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, formid);
				SurveyModel.RequestedViewId = RequestedViewId;

				var RelateSurveyId = FormsHierarchy.Single(x => x.ViewId == ViewId);


				if (!string.IsNullOrEmpty(responseid))
				{
					SurveyModel.FormResponseInfoModel = GetFormResponseInfoModel(RelateSurveyId.FormId, responseid);
					SurveyModel.FormResponseInfoModel.NumberOfResponses = SurveyModel.FormResponseInfoModel.ResponsesList.Count();

					SurveyModel.FormResponseInfoModel.ParentResponseId = responseid;
				}

				if (RelateSurveyId.ResponseIds.Count() > 0)
				{

					Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(RelateSurveyId.ResponseIds[0].ResponseId);
					var form = _isurveyFacade.GetSurveyFormData(RelateSurveyId.ResponseIds[0].SurveyId, 1, surveyAnswerDTO, IsMobileDevice, null,null,IsAndroid);
					SurveyModel.Form = form;
					if (string.IsNullOrEmpty(responseid))
					{
						SurveyModel.FormResponseInfoModel = GetFormResponseInfoModel(RelateSurveyId.FormId, RelateSurveyId.ResponseIds[0].RelateParentId);
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
						Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(SurveyModel.FormResponseInfoModel.ResponsesList[0].Column0, RelateSurveyId.FormId);
						ResponseInfoModel = GetFormResponseInfoModel(RelateSurveyId.FormId, responseid);
						SurveyModel.Form = _isurveyFacade.GetSurveyFormData(surveyAnswerDTO.SurveyId, 1, surveyAnswerDTO, IsMobileDevice, null,null,IsAndroid );
						ResponseInfoModel.FormInfoModel.FormName = SurveyModel.Form.SurveyInfo.SurveyName.ToString();
						ResponseInfoModel.FormInfoModel.FormId = SurveyModel.Form.SurveyInfo.SurveyId.ToString();
						ResponseInfoModel.ParentResponseId = responseid;//SurveyModel.FormResponseInfoModel.ResponsesList[0].Column0;
						ResponseInfoModel.NumberOfResponses = SurveyModel.FormResponseInfoModel.ResponsesList.Count();
					}
					else
					{
						var form1 = _isurveyFacade.GetSurveyInfoModel(RelateSurveyId.FormId);
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
		public ActionResult Index(string surveyid, string AddNewFormId, string EditForm, string Cancel)
		{

			int userId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			bool isMobileDevice = this.Request.Browser.IsMobileDevice;
			FormsAuthentication.SetAuthCookie("BeginSurvey", false);
			bool isEditMode = false;
			if (isMobileDevice == false)
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
				Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswer = GetSurveyAnswer(editFormResponseId);
				if (Session["RecoverLastRecordVersion"] != null)
				{
					surveyAnswer.RecoverLastRecordVersion = bool.Parse(Session[SessionKeys.RecoverLastRecordVersion].ToString());
				}
				string ChildRecordId = GetChildRecordId(surveyAnswer);
				return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, Epi.Web.MVC.Constants.Constant.SURVEY_CONTROLLER, new { responseid = surveyAnswer.ParentRecordId, PageNumber = 1, Edit = "Edit" });
			}
			
			//create the responseid
			Guid responseID = Guid.NewGuid();
			if (Session[SessionKeys.RootResponseId] == null)
			{
				Session[SessionKeys.RootResponseId] = responseID;
			}
			TempData[Epi.Web.MVC.Constants.Constant.RESPONSE_ID] = responseID.ToString();

			// create the first survey response
			Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO = _isurveyFacade.CreateSurveyAnswer(AddNewFormId, responseID.ToString(), userId, true, this.Request.Form["Parent_Response_Id"].ToString(), isEditMode);
			List<FormsHierarchyDTO> formsHierarchy = GetFormsHierarchy();
			SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswerDTO.SurveyId, formsHierarchy);
            MetadataAccessor metadataAccessor = surveyInfoModel as MetadataAccessor;

            // set the survey answer to be production or test 
            surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;

            MvcDynamicForms.Form form = _isurveyFacade.GetSurveyFormData(surveyAnswerDTO.SurveyId, 1, surveyAnswerDTO, isMobileDevice, null, formsHierarchy,isAndroid );

			TempData["Width"] = form.Width + 100;

            var formDigest = metadataAccessor.GetFormDigest(surveyAnswerDTO.SurveyId);
            string checkcode = formDigest.CheckCode;

            FormResponseDetail responseDetail = surveyAnswerDTO.ResponseDetail;

            form.FormCheckCodeObj = form.GetCheckCodeObj(MetadataAccessor.CurrentFormFieldDigests, responseDetail, checkcode);

			///////////////////////////// Execute - Record Before - start//////////////////////
			Dictionary<string, string> ContextDetailList = new Dictionary<string, string>();
			EnterRule FunctionObject_B = (EnterRule)form.FormCheckCodeObj.GetCommand("level=record&event=before&identifier=");
			SurveyResponseDocDb surveyResponseDocDb = new SurveyResponseDocDb(_pageFields, _requiredList);
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

					_isurveyFacade.UpdateSurveyResponse(surveyInfoModel, responseID.ToString(), form, surveyAnswerDTO, false, false, 0, userId);
				}
				catch (Exception ex)
				{
					// do nothing so that processing
					// can continue
				}
			}
			else
			{
                form.RequiredFieldsList = _requiredList;
				Session[SessionKeys.RequiredList] = _requiredList;
				_isurveyFacade.UpdateSurveyResponse(surveyInfoModel, surveyAnswerDTO.ResponseId, form, surveyAnswerDTO, false, false, 0, userId);
			}

			surveyAnswerDTO = (SurveyAnswerDTO)formsHierarchy.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == surveyAnswerDTO.ResponseId);

			///////////////////////////// Execute - Record Before - End//////////////////////
			return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, Epi.Web.MVC.Constants.Constant.SURVEY_CONTROLLER, new { responseid = responseID, PageNumber = 1 });
		}


		public FormResponseInfoModel GetSurveyResponseInfoModel(string surveyId, int pageNumber)
		{
			int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			FormResponseInfoModel FormResponseInfoModel = new FormResponseInfoModel();
			SurveyAnswerRequest FormResponseReq = new SurveyAnswerRequest();
			FormSettingRequest FormSettingReq = new Enter.Common.Message.FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };

			//Populating the request
			FormResponseReq.Criteria.SurveyId = surveyId.ToString();
			FormResponseReq.Criteria.PageNumber = pageNumber;
			FormResponseReq.Criteria.IsMobile = true;
			FormSettingReq.FormInfo.FormId = new Guid(surveyId).ToString();

			//Getting Column Name  List
			FormSettingResponse FormSettingResponse = _isurveyFacade.GetFormSettings(FormSettingReq);
			Columns = FormSettingResponse.FormSetting.ColumnNameList.ToList();
			Columns.Sort(Compare);
			FormResponseInfoModel.SearchModel = new SearchBoxModel();
			// Setting  Column Name  List
			FormResponseInfoModel.Columns = Columns;
			FormResponseReq.Criteria.IsSqlProject = FormSettingResponse.FormInfo.IsSQLProject;
			FormResponseReq.Criteria.UserId = UserId;
			Session[SessionKeys.IsSqlProject] = FormSettingResponse.FormInfo.IsSQLProject;
			//if (Session[SessionKeys.SearchCriteria] != null)
			//{
			//    FormResponseInfoModel.SearchModel = (SeachBoxModel)Session[SessionKeys.SearchCriteria];
			//}
			//FormResponseReq.Criteria.SearchCriteria = CreateSearchCriteria(Request.QueryString, FormResponseInfoModel.SearchModel, FormResponseInfoModel);

			//Following code retains the search and sort criteria for already selected form. 
			if (!IsNewRequest)
			{
				if (Session[SessionKeys.SortOrder] != null &&
						!string.IsNullOrEmpty(Session[SessionKeys.SortOrder].ToString()) &&
					string.IsNullOrEmpty(Request.QueryString["sort"]))
				{
					Sort = Session[SessionKeys.SortOrder].ToString();
				}
				else
				{
					Sort = Request.QueryString["sort"];
					Session[SessionKeys.SortOrder] = Sort;
				}

				if (Session[SessionKeys.SortField] != null &&
					   !string.IsNullOrEmpty(Session[SessionKeys.SortField].ToString()) &&
				   string.IsNullOrEmpty(Request.QueryString["sortfield"]))
				{
					SortField = Session[SessionKeys.SortField].ToString();
				}
				else
				{
					SortField = Request.QueryString["sortfield"];
					Session[SessionKeys.SortField] = SortField;
				}
			}

			if (!IsNewRequest &&
					Session[SessionKeys.SearchCriteria] != null && !string.IsNullOrEmpty(Session[SessionKeys.SearchCriteria].ToString()) &&
					(Request.QueryString["reset"] == null) && Request.QueryString["col1"] == null

				)
			//(Request.QueryString["col1"] == null || Request.QueryString["col1"] == "undefined") &&
			{
				FormResponseReq.Criteria.SearchCriteria = Session[SessionKeys.SearchCriteria].ToString();
				FormResponseInfoModel.SearchModel = (SearchBoxModel)Session[SessionKeys.SearchModel];
			}
			else
			{
				FormResponseReq.Criteria.SearchCriteria = CreateSearchCriteria(Request.QueryString, FormResponseInfoModel.SearchModel, FormResponseInfoModel);
				Session[SessionKeys.SearchModel] = FormResponseInfoModel.SearchModel;
				Session[SessionKeys.SearchCriteria] = FormResponseReq.Criteria.SearchCriteria;
			}
			Session[SessionKeys.PageNumber] = pageNumber;
			// Session[SessionKeys.SearchCriteria] = FormResponseInfoModel.SearchModel;
			PopulateDropDownlists(FormResponseInfoModel, FormSettingResponse.FormSetting.FormControlNameList.ToList());

			if (Sort != null && Sort.Length > 0)
			{
				FormResponseReq.Criteria.SortOrder = Sort;
			}

			if (SortField != null && SortField.Length > 0)
			{
				FormResponseReq.Criteria.Sortfield = SortField;
			}

            FormResponseReq.Criteria.SurveyQAList = new Dictionary<string, string>();
            foreach (var sqlParam in Columns)
            {
                FormResponseReq.Criteria.SurveyQAList.Add(sqlParam.Key.ToString(), sqlParam.Value.ToString());
            }

            //Getting Resposes
            SurveyAnswerResponse FormResponseList = _isurveyFacade.GetFormResponseList(FormResponseReq);

			//Setting Resposes List
			List<ResponseModel> ResponseList = new List<ResponseModel>();
			foreach (var item in FormResponseList.SurveyResponseList)
			{
					ResponseList.Add(ConvertRowToModel(item, Columns, "GlobalRecordId"));
			}

			FormResponseInfoModel.ResponsesList = ResponseList;
			//Setting Form Info 
			FormResponseInfoModel.FormInfoModel = Mapper.ToFormInfoModel(FormResponseList.FormInfo);
			//Setting Additional Data

			FormResponseInfoModel.NumberOfPages = FormResponseList.NumberOfPages;
			FormResponseInfoModel.PageSize = FormResponseList.PageSize;
			FormResponseInfoModel.NumberOfResponses = FormResponseList.NumberOfResponses;
			FormResponseInfoModel.sortfield = SortField;
			FormResponseInfoModel.sortOrder = Sort;
			FormResponseInfoModel.CurrentPage = pageNumber;
			return FormResponseInfoModel;
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

		public SurveyInfoModel GetSurveyInfo(string surveyId, List<Epi.Web.Enter.Common.DTO.FormsHierarchyDTO> formsHierarchyDTOList = null)
		{
			SurveyInfoModel surveyInfoModel = new SurveyInfoModel();
			if (formsHierarchyDTOList != null)
			{
				surveyInfoModel = Mapper.ToSurveyInfoModel(formsHierarchyDTOList.FirstOrDefault(x => x.FormId == surveyId).SurveyInfo);
			}
			else
			{
				surveyInfoModel = _isurveyFacade.GetSurveyInfoModel(surveyId);
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
			SurveyAnswerResponse = _isurveyFacade.SetChildRecord(SurveyAnswerRequest);
			result = SurveyAnswerResponse.SurveyResponseList[0].ResponseId.ToString();
			return result;
		}

		private Epi.Web.Enter.Common.DTO.SurveyAnswerDTO GetSurveyAnswer(string responseId, string formid = "")
		{
			Epi.Web.Enter.Common.DTO.SurveyAnswerDTO result = null;

			string FormId = Session[SessionKeys.RootFormId].ToString();
			string Id = Session[SessionKeys.UserId].ToString();
			if (string.IsNullOrEmpty(formid))
			{
				result = _isurveyFacade.GetSurveyAnswerResponse(responseId, FormId, SurveyHelper.GetDecryptUserId(Id)).SurveyResponseList[0];
			}
			else
			{
				result = _isurveyFacade.GetSurveyAnswerResponse(responseId, formid, SurveyHelper.GetDecryptUserId(Id)).SurveyResponseList[0];
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
		public ActionResult Delete(string ResponseId, string surveyid)
		{
			SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
			surveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = ResponseId });
			string Id = Session[SessionKeys.UserId].ToString();
			surveyAnswerRequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Id);
			surveyAnswerRequest.Criteria.IsEditMode = false;
			surveyAnswerRequest.Criteria.IsDeleteMode = false;
			surveyAnswerRequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
			surveyAnswerRequest.Criteria.SurveyId = Session[SessionKeys.RootFormId].ToString();

			SurveyAnswerResponse SAResponse = _isurveyFacade.DeleteResponse(surveyAnswerRequest);

			return Json(surveyid);
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
			SurveyAnswerResponse SAResponse = _isurveyFacade.DeleteResponse(surveyAnswerRequest);

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
				formsHierarchyResponse = _isurveyFacade.GetFormsHierarchy(formsHierarchyRequest);
			}
			return formsHierarchyResponse.FormsHierarchy;
		}

		private FormResponseInfoModel GetFormResponseInfoModel(string surveyId, string responseId)
		{
			int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			FormResponseInfoModel formResponseInfoModel = new FormResponseInfoModel();

			SurveyResponseDocDb surveyResponseHelper = new SurveyResponseDocDb();
			if (!string.IsNullOrEmpty(surveyId))
			{
				SurveyAnswerRequest formResponseReq = new SurveyAnswerRequest();
				FormSettingRequest formSettingReq = new Enter.Common.Message.FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };

				//Populating the request
				formSettingReq.FormInfo.FormId = surveyId;
				formSettingReq.FormInfo.UserId = UserId;

				//Getting Column Name  List
				FormSettingResponse FormSettingResponse = _isurveyFacade.GetFormSettings(formSettingReq);
				Columns = FormSettingResponse.FormSetting.ColumnNameList.ToList();
				Columns.Sort(Compare);

				// Setting  Column Name  List
				formResponseInfoModel.Columns = Columns;

				//Getting Resposes
				formResponseReq.Criteria.SurveyId = surveyId.ToString();
				formResponseReq.Criteria.SurveyAnswerIdList.Add(responseId);

				formResponseReq.Criteria.PageNumber = 1;
				formResponseReq.Criteria.UserId = UserId;
				formResponseReq.Criteria.IsMobile = true;
                formResponseReq.Criteria.SurveyQAList = formResponseReq.Criteria.SurveyQAList;
                SurveyAnswerResponse formResponseList = _isurveyFacade.GetResponsesByRelatedFormId(formResponseReq);

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

				//Setting Additional Data
				formResponseInfoModel.NumberOfPages = formResponseList.NumberOfPages;
				formResponseInfoModel.PageSize = ReadPageSize();
				formResponseInfoModel.NumberOfResponses = formResponseList.SurveyResponseList.Count();//FormResponseList.NumberOfResponses;
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
