using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Interfaces;
using Epi.Cloud.DataEntryServices.Model;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Core.EnterInterpreter;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Cloud.Common.DTO;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Constants;
using Epi.Web.MVC.Facade;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;
using Epi.Web.Enter.Common.DTO;

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
				return View(Epi.Web.MVC.Constants.Constant.INDEX_PAGE, surveyModel);
			}
			catch (Exception ex)
			{
				Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);

				ExceptionModel ExModel = new ExceptionModel();
				ExModel.ExceptionDetail = "Stack Trace : " + ex.StackTrace;
				ExModel.Message = ex.Message;

				return View(Epi.Web.MVC.Constants.Constant.EXCEPTION_PAGE, ExModel);
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
#region Summa
			//if (surveyAnswerDTO == null)
			//{
				//surveyAnswerDTO = new SurveyAnswerDTO { SurveyId = surveyId, ResponseId = responseId };
				//ValidationTest = PreValidationResultEnum.Success;
			//}
			//else
			//{
				//ValidationTest = PreValidateResponse(surveyAnswerDTO.ToSurveyAnswerModel());
			//}
            //  PreValidateResponse(Mapper.ToSurveyAnswerModel(SurveyAnswer));
            //  ValidationTest = PreValidateResponse(surveyAnswerDTO.ToSurveyAnswerModel());

            ValidationTest = PreValidateResponse(Mapper.ToSurveyAnswerModel(surveyAnswerDTO));
#endregion Summa
            string formId = surveyId;
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
					TempData["Width"] = form.Width + 5;
					// if redirect then perform server validation before displaying
					if (TempData.ContainsKey("isredirect") && !string.IsNullOrWhiteSpace(TempData["isredirect"].ToString()))
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
					SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, form.SurveyInfo.SurveyId);
					//return View(Epi.Web.MVC.Constants.Constant.INDEX_PAGE, SurveyModel);
					return SurveyModel;
			}
		}

		private void UpdateStatus(string ResponseId, string SurveyId, int StatusId, RecordStatusChangeReason statusChangeReason)
		{
			SurveyAnswerRequest SurveyAnswerRequest = new SurveyAnswerRequest();
			SurveyAnswerRequest.Criteria.SurveyAnswerIdList.Add(ResponseId);

			SurveyAnswerRequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = ResponseId });
			SurveyAnswerRequest.Criteria.StatusId = StatusId;
			SurveyAnswerRequest.Criteria.StatusChangeReason = statusChangeReason;

			SurveyAnswerRequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			if (!string.IsNullOrEmpty(SurveyId))
			{
				SurveyAnswerRequest.Criteria.SurveyId = SurveyId;
			}

			_surveyFacade.UpdateResponseStatus(SurveyAnswerRequest);
		}





		[HttpPost]
		//  [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
		[ValidateAntiForgeryToken]
		//public ActionResult Index(SurveyInfoModel surveyInfoModel, string Submitbutton, string Savebutton, string ContinueButton, string PreviousButton, int PageNumber = 1)
		public ActionResult Index(SurveyAnswerModel surveyAnswerModel, 
			string Submitbutton, 
			string Savebutton, 
			string ContinueButton, 
			string PreviousButton, 
			string Close, 
			string CloseButton, 
			int PageNumber = 0, 
			string Form_Has_Changed = "", 
			string Requested_View_Id = "", 
			bool Log_Out = false
			)
		{

			SetGlobalVariable();
			ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			string responseId = surveyAnswerModel.ResponseId;
			Session[SessionKeys.FormValuesHasChanged] = Form_Has_Changed;

			bool IsAndroid = false;

			if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				IsAndroid = true;
			}
			if (Session[SessionKeys.RootResponseId] != null && Session[SessionKeys.RootResponseId].ToString() == responseId)
			{
				Session[SessionKeys.RelateButtonPageId] = null;
			}
			List<FormsHierarchyDTO> FormsHierarchy = GetFormsHierarchy();

			var response = FormsHierarchy.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == responseId);
			string SurveyId = null;
			if (response != null)
			{
				SurveyId = response.SurveyId;
				// Initialize the Metadata Accessor
				MetadataAccessor.CurrentFormId = SurveyId;

				bool IsMobileDevice = false;
				IsMobileDevice = this.Request.Browser.IsMobileDevice;
				if (IsMobileDevice == false)
				{
					IsMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
				}
				try
				{
					string FormValuesHasChanged = Form_Has_Changed;

					SurveyAnswerDTO SurveyAnswer = new SurveyAnswerDTO();
					SurveyAnswer = (SurveyAnswerDTO)FormsHierarchy.SelectMany(x => x.ResponseIds).First(z => z.ResponseId == responseId);
					SurveyAnswer.RequestedViewId = Requested_View_Id;
					SurveyAnswer.CurrentPageNumber = PageNumber != 0 ? PageNumber : 1;

					//object temp = System.Web.HttpContext.Current.Cache;
					SurveyInfoModel surveyInfoModel = GetSurveyInfo(SurveyAnswer.SurveyId, FormsHierarchy);

					//////////////////////UpDate Survey Mode//////////////////////////
					SurveyAnswer.IsDraftMode = surveyInfoModel.IsDraftMode;
					PreValidationResultEnum ValidationTest = PreValidateResponse(Mapper.ToSurveyAnswerModel(SurveyAnswer));

					switch (ValidationTest)
					{
						case PreValidationResultEnum.SurveyIsPastClosingDate:
							return View("SurveyClosedError");
						case PreValidationResultEnum.SurveyIsAlreadyCompleted:
							return View("IsSubmitedError");
						case PreValidationResultEnum.Success:
						default:


							//Update Survey Model Start
							MvcDynamicForms.Form form = UpdateSurveyModel(surveyInfoModel, IsMobileDevice, FormValuesHasChanged, SurveyAnswer);
							//Update Survey Model End

							//PassCode start
							if (IsMobileDevice)
							{

								form = SetFormPassCode(form, responseId);
							}
							//passCode end
							form.StatusId = SurveyAnswer.Status;
							bool IsSubmited = false;
							bool IsSaved = false;

							form = SetLists(form);

							_surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, SurveyAnswer, IsSubmited, IsSaved, PageNumber, UserId);



							if (!string.IsNullOrEmpty(this.Request.Form["is_save_action"]) && this.Request.Form["is_save_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
							{
								form = SaveCurrentForm(form, surveyInfoModel, SurveyAnswer, responseId, UserId, IsSubmited, IsSaved, IsMobileDevice, FormValuesHasChanged, PageNumber, FormsHierarchy);
								form = SetLists(form);
								TempData["Width"] = form.Width + 5;
								SurveyModel SurveyModel = new SurveyModel();
								SurveyModel.Form = form;
								SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, form.SurveyInfo.SurveyId);

								return View(Epi.Web.MVC.Constants.Constant.INDEX_PAGE, SurveyModel);

							}
							else if (!string.IsNullOrEmpty(this.Request.Form["Go_Home_action"]) && this.Request.Form["Go_Home_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
							{

								IsSaved = true;
								form = SaveCurrentForm(form, surveyInfoModel, SurveyAnswer, responseId, UserId, IsSubmited, IsSaved, IsMobileDevice, FormValuesHasChanged, PageNumber, FormsHierarchy);
								form = SetLists(form);
								TempData["Width"] = form.Width + 5;
								SurveyModel SurveyModel = new SurveyModel();
								SurveyModel.Form = form;
								SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, form.SurveyInfo.SurveyId);


								return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = RootResponseId, PageNumber = 1 });

							}
							else if (!string.IsNullOrEmpty(this.Request.Form["Go_One_Level_Up_action"]) && this.Request.Form["Go_One_Level_Up_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
							{
								IsSaved = true;

								string RelateParentId = "";
								form = SaveCurrentForm(form, surveyInfoModel, SurveyAnswer, responseId, UserId, IsSubmited, IsSaved, IsMobileDevice, FormValuesHasChanged, PageNumber, FormsHierarchy);
								form = SetLists(form);
								TempData["Width"] = form.Width + 5;
								SurveyModel SurveyModel = new SurveyModel();
								SurveyModel.Form = form;
								SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, form.SurveyInfo.SurveyId);

								var CurentRecordParent = FormsHierarchy.Single(x => x.FormId == surveyInfoModel.SurveyId);
								foreach (var item in CurentRecordParent.ResponseIds)
								{
									if (item.ResponseId == responseId && !string.IsNullOrEmpty(item.RelateParentId))
									{

										RelateParentId = item.RelateParentId;
										break;
									}


								}
								Dictionary<string, int> SurveyPagesList = (Dictionary<string, int>)Session[SessionKeys.RelateButtonPageId];
								if (SurveyPagesList != null)
								{
									PageNumber = SurveyPagesList[RelateParentId];
								}
								if (!string.IsNullOrEmpty(RelateParentId))
								{
									return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = RelateParentId, PageNumber = PageNumber });
								}
								else
								{
									return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = RootResponseId, PageNumber = PageNumber });
								}


							}
							else if (!string.IsNullOrEmpty(this.Request.Form["Get_Child_action"]) && this.Request.Form["Get_Child_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
							{
								int RequestedViewId;

								SetRelateSession(responseId, PageNumber);
								RequestedViewId = int.Parse(this.Request.Form["Requested_View_Id"]);
								SurveyAnswer.RelateParentId = responseId; // TODO: GEL varify this
								form = SaveCurrentForm(form, surveyInfoModel, SurveyAnswer, responseId, UserId, IsSubmited, IsSaved, IsMobileDevice, FormValuesHasChanged, PageNumber, FormsHierarchy);
								form = SetLists(form);
								TempData["Width"] = form.Width + 5;
								Session[SessionKeys.RequestedViewId] = RequestedViewId;
								SurveyModel SurveyModel = new SurveyModel();
								SurveyModel.Form = form;
								SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, form.SurveyInfo.SurveyId);
								SurveyModel.RequestedViewId = RequestedViewId;
								int.TryParse(this.Request.Form["Requested_View_Id"].ToString(), out RequestedViewId);
								var RelateSurveyId = FormsHierarchy.Single(x => x.ViewId == RequestedViewId);

								int ViewId = int.Parse(Requested_View_Id);

								string ChildResponseId = AddNewChild(RelateSurveyId.FormId, ViewId, responseId, FormValuesHasChanged, "1");
								return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = ChildResponseId, PageNumber = 1 });

							}
							//Read_Response_action
							else if (!string.IsNullOrEmpty(this.Request.Form["Read_Response_action"]) && this.Request.Form["Read_Response_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
							{
								SetRelateSession(responseId, PageNumber);

								this.UpdateStatus(surveyAnswerModel.ResponseId, surveyAnswerModel.SurveyId, RecordStatus.Saved, RecordStatusChangeReason.ReadResponse);

								int RequestedViewId = int.Parse(this.Request.Form["Requested_View_Id"]);
								// return RedirectToRoute(new { Controller = "RelatedResponse", Action = "Index", SurveyId = form.SurveyInfo.SurveyId, ViewId = RequestedViewId, ResponseId = responseId, CurrentPage = 1 });

								return RedirectToRoute(new { Controller = "FormResponse", Action = "Index", formid = form.SurveyInfo.SurveyId, ViewId = RequestedViewId, responseid = responseId, Pagenumber = 1 });

							}

							else if (!string.IsNullOrEmpty(this.Request.Form["Do_Not_Save_action"]) && this.Request.Form["Do_Not_Save_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
							{


								bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);

								SurveyAnswerRequest SARequest = new SurveyAnswerRequest();
								SARequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = Session[SessionKeys.RootResponseId].ToString() });
								SARequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
								SARequest.Criteria.IsEditMode = this.IsEditMode;
								SARequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
								SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);
								return RedirectToRoute(new { Controller = "FormResponse", Action = "Index", formid = Session[SessionKeys.RootFormId].ToString(), ViewId = 0, PageNumber = Convert.ToInt32(Session[SessionKeys.PageNumber].ToString()) });

							}

							else if (!string.IsNullOrEmpty(this.Request.Form["is_goto_action"]) && this.Request.Form["is_goto_action"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
							{
								//This is a Navigation to a url


								form = SetLists(form);

								_surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, SurveyAnswer, IsSubmited, IsSaved, PageNumber, UserId);

								SurveyAnswer = _surveyFacade.GetSurveyAnswerResponse(responseId, surveyInfoModel.SurveyId).SurveyResponseList[0];
								form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, PageNumber, SurveyAnswer, IsMobileDevice, null, FormsHierarchy, IsAndroid);
								form.FormValuesHasChanged = FormValuesHasChanged;
								TempData["Width"] = form.Width + 5;
								SurveyModel SurveyModel = new SurveyModel();
								SurveyModel.Form = form;
								SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, form.SurveyInfo.SurveyId);

								return View(Epi.Web.MVC.Constants.Constant.INDEX_PAGE, SurveyModel);

							}

							else if (form.Validate(form.RequiredFieldsList))
							{
								if (!string.IsNullOrEmpty(Submitbutton) || !string.IsNullOrEmpty(CloseButton) || (!string.IsNullOrEmpty(this.Request.Form["is_save_action_Mobile"]) && this.Request.Form["is_save_action_Mobile"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase)))
								{

									KeyValuePair<string, int> ValidateValues = ValidateAll(form, UserId, IsSubmited, IsSaved, IsMobileDevice, FormValuesHasChanged);

									if (!string.IsNullOrEmpty(FormValuesHasChanged))
									{
										if (!string.IsNullOrEmpty(ValidateValues.Key) && !string.IsNullOrEmpty(ValidateValues.Value.ToString()))
										{
											return RedirectToRoute(new { Controller = "Survey", Action = "Index", responseid = ValidateValues.Key, PageNumber = ValidateValues.Value.ToString() });
										}
									}
									this.UpdateStatus(form.ResponseId, form.SurveyInfo.SurveyId, RecordStatus.Saved, RecordStatusChangeReason.SubmitOrClose);

									SurveyAnswerRequest SurveyAnswerRequest1 = new SurveyAnswerRequest();
									SurveyAnswerRequest1.Action = "DeleteResponse";

									var List = FormsHierarchy.SelectMany(x => x.ResponseIds).OrderByDescending(x => x.DateCreated);
									SurveyAnswerRequest1.SurveyAnswerList = List.ToList();

									if (this.IsEditMode)
									{
										_surveyFacade.DeleteResponseNR(SurveyAnswerRequest1);
									}

									if (!string.IsNullOrEmpty(CloseButton))
									{
										if (!Log_Out)
										{
											return RedirectToAction("Index", "Home", new { surveyid = this.RootFormId, orgid = (int)Session[SessionKeys.SelectedOrgId] });
										}
										else
										{
											return RedirectToAction("Index", "Post");

										}
									}
									else
									{
										if (!IsMobileDevice)
										{
											if (string.IsNullOrEmpty(this.RootFormId))
											{
												return RedirectToAction("Index", "Home", new { surveyid = surveyInfoModel.SurveyId, orgid = (int)Session[SessionKeys.SelectedOrgId] });
											}
											else
											{
												return RedirectToAction("Index", "Home", new { surveyid = this.RootFormId, orgid = (int)Session[SessionKeys.SelectedOrgId] });

											}
										}
										else
										{
											return RedirectToAction("Index", "FormResponse", new { formid = this.RootFormId, Pagenumber = Convert.ToInt32(Session[SessionKeys.PageNumber]) });

										}

									}
								}
								else
								{
									//This is a Navigation to a url

									//////////////////////UpDate Survey Mode//////////////////////////

#region Ananth
#if false
									if (responseId != null )
                                    {                 //Survey Info
                                        SurveyResponseBO surveyResponseBO = new SurveyResponseBO();
                                        surveyResponseBO.ResponseId = responseId;
                                        surveyResponseBO.IsDraftMode = surveyInfoModel.IsDraftMode;
                                        surveyResponseBO.UserId = UserId;
                                        _isurveyDocumentDBStoreFacade.InsertResponseAsync(form, surveyResponseBO);
                                    }
#endif
#endregion Ananth
									form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, PageNumber, SurveyAnswer, IsMobileDevice, null, FormsHierarchy, IsAndroid);
									form.FormValuesHasChanged = FormValuesHasChanged;
									TempData["Width"] = form.Width + 5;
									//PassCode start
									if (IsMobileDevice)
									{
										form = SetFormPassCode(form, responseId);
									}
									//passCode end
									form.StatusId = SurveyAnswer.Status;
									SurveyModel SurveyModel = new SurveyModel();
									SurveyModel.Form = form;


									SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, form.SurveyInfo.SurveyId);
									if (!string.IsNullOrEmpty(this.Request.Form["Click_Related_Form"]))
									{
										bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);
										string Edit = "";
										if (IsEditMode)
										{
											ViewBag.Edit = "Edit";
										}
										//SurveyModel = GetIndex(form.ResponseId, form.CurrentPage, Edit, form.SurveyInfo.SurveyId);
										SurveyModel.RelatedButtonWasClicked = this.Request.Form["Click_Related_Form"].ToString();

										return View(Epi.Web.MVC.Constants.Constant.INDEX_PAGE, SurveyModel);
										// return RedirectToAction("Index", "Survey", new { RequestId = form.ResponseId, PageNumber = form.CurrentPage });
									}
									else
									{
										return RedirectToAction("Index", "Survey", new { RequestId = form.ResponseId, PageNumber = form.CurrentPage });
									}
									//  return View(Epi.Web.MVC.Constants.Constant.INDEX_PAGE, SurveyModel);

								}

							}
							else
							{
								//Invalid Data - stay on same page
								int CurrentPageNum = SurveyAnswer.CurrentPageNumber;



								if (CurrentPageNum != PageNumber) // failed validation and navigating to different page// must keep url the same 
								{
									TempData["isredirect"] = "true";
									TempData["Width"] = form.Width + 5;
									return RedirectToAction("Index", "Survey", new { RequestId = form.ResponseId, PageNumber = CurrentPageNum });

								}
								else
								{
									TempData["Width"] = form.Width + 5;
									SurveyModel SurveyModel = new SurveyModel();
									SurveyModel.Form = form;
									SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, form.SurveyInfo.SurveyId);

									return View(Epi.Web.MVC.Constants.Constant.INDEX_PAGE, SurveyModel);
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

                    return View(Epi.Web.MVC.Constants.Constant.EXCEPTION_PAGE, ExModel);
				}
			}
			else
			{
                ExceptionModel ExModel = new ExceptionModel();
                ExModel.ExceptionDetail = "Survey Save";
                ExModel.Message = "Response Not Found";

                return View(Epi.Web.MVC.Constants.Constant.EXCEPTION_PAGE, ExModel);
			}
		}


		private int GetResponseCount(List<FormsHierarchyDTO> FormsHierarchy, int RequestedViewId, string responseId)
		{
			int ResponseCount = 0;
			var ViewResponses = FormsHierarchy.Where(x => x.ViewId == RequestedViewId);

			foreach (var item in ViewResponses)
			{
				if (item.ResponseIds.Count > 0)
				{
					var list = item.ResponseIds.Any(x => x.RelateParentId == responseId);
					if (list == true)
					{

						ResponseCount++;
						break;
					}
				}
			}

			return ResponseCount;
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
				formsHierarchyRequest.SurveyResponseInfo.ResponseId = rootResponseId.ToString();
				formsHierarchyResponse = _surveyFacade.GetFormsHierarchy(formsHierarchyRequest);
			}

			return formsHierarchyResponse.FormsHierarchy;
		}

		private int GetCurrentPage()
		{
			int CurrentPage = 1;

			string PageNum = this.Request.UrlReferrer.ToString().Substring(this.Request.UrlReferrer.ToString().LastIndexOf('/') + 1);

			int.TryParse(PageNum, out CurrentPage);
			return CurrentPage;
		}

		private void SetCurrentPage(SurveyAnswerDTO surveyAnswerDTO, int viewPageNumber)
		{
			//surveyAnswerDTO.ResponseDetail.LastPageVisited = viewPageNumber;

			Epi.Web.Enter.Common.Message.SurveyAnswerRequest sar = new Enter.Common.Message.SurveyAnswerRequest();
			sar.Action = "Update";
			sar.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			sar.SurveyAnswerList.Add(surveyAnswerDTO);

			this._surveyFacade.GetSurveyAnswerRepository().SaveSurveyAnswer(sar);
		}

		private SurveyAnswerDTO GetSurveyAnswer(string responseId, string currentFormId = "")
		{
			SurveyAnswerDTO result = null;

			//responseId = TempData[Epi.Web.MVC.Constants.Constant.RESPONSE_ID].ToString();
			result = _surveyFacade.GetSurveyAnswerResponse(responseId, currentFormId, SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString())).SurveyResponseList[0];

			return result;
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
			int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			try
			{
				if (!string.IsNullOrEmpty(NameList))
				{
					string[] nameList = null;


					nameList = NameList.Split(',');

					bool IsMobileDevice = false;

					IsMobileDevice = this.Request.Browser.IsMobileDevice;
					SurveyAnswerDTO SurveyAnswer = _surveyFacade.GetSurveyAnswerResponse(responseId).SurveyResponseList[0];

					var surveyId = SurveyAnswer.SurveyId;
					SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyId);

					int numberOfPages = surveyInfoModel.GetFormDigest(surveyId).NumberOfPages;

					foreach (string Name in nameList)
					{
						for (int i = numberOfPages; i > 0; i--)
						{
							SurveyAnswer = _surveyFacade.GetSurveyAnswerResponse(SurveyAnswer.ResponseId, SurveyAnswer.SurveyId).SurveyResponseList[0];

							MvcDynamicForms.Form formRs = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, i, SurveyAnswer, IsMobileDevice, null, null, IsAndroid);

							formRs = Epi.Web.MVC.Utility.SurveyHelper.UpdateControlsValues(formRs, Name, Value);

							_surveyFacade.UpdateSurveyResponse(surveyInfoModel, SurveyAnswer.ResponseId, formRs, SurveyAnswer, false, false, i, UserId);
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
			int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			try
			{
				bool IsMobileDevice = false;
				int PageNumber = Value;
				IsMobileDevice = this.Request.Browser.IsMobileDevice;


				SurveyAnswerDTO SurveyAnswer = _surveyFacade.GetSurveyAnswerResponse(responseId).SurveyResponseList[0];
				bool IsAndroid = false;

				if (this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					IsAndroid = true;
				}


				//SurveyInfoModel surveyInfoModel = _isurveyFacade.GetSurveyInfoModel(SurveyAnswer.SurveyId);
				SurveyInfoModel surveyInfoModel = GetSurveyInfo(SurveyAnswer.SurveyId);
				PreValidationResultEnum ValidationTest = PreValidateResponse(Mapper.ToSurveyAnswerModel(SurveyAnswer));
				var form = _surveyFacade.GetSurveyFormData(SurveyAnswer.SurveyId, PageNumber, SurveyAnswer, IsMobileDevice,null,null,IsAndroid);

				form.StatusId = SurveyAnswer.Status;
				var IsSaved = false;
				form.IsSaved = true;
				SurveyAnswer = _surveyFacade.GetSurveyAnswerResponse(responseId, SurveyAnswer.SurveyId).SurveyResponseList[0];

                var pageNumber = GetSurveyPageNumber(SurveyAnswer.ResponseDetail);
                form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, pageNumber == 0 ? 1 : pageNumber, SurveyAnswer, IsMobileDevice ,null ,null ,IsAndroid );
				//Update the model
				UpdateModel(form);
				//Save the child form
				_surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, SurveyAnswer, false, IsSaved, PageNumber, UserId);
				//  SetCurrentPage(SurveyAnswer, PageNumber);
				//Save the parent form 
				IsSaved = form.IsSaved = true;
				_surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, SurveyAnswer, false, IsSaved, PageNumber, UserId);
				return Json(true);

			}
			catch (Exception ex)
			{
				return Json(false);
			}
		}


		//[OutputCache(Duration = int.MaxValue, VaryByParam = "SurveyId", Location = OutputCacheLocation.Server)]
		public SurveyInfoModel GetSurveyInfo(string SurveyId, List<FormsHierarchyDTO> FormsHierarchyDTOList = null)
		{

			/* var CacheObj = HttpRuntime.Cache.Get(SurveyId);
			if (CacheObj ==null)
			{

					   SurveyInfoModel surveyInfoModel = _isurveyFacade.GetSurveyInfoModel(SurveyId);
					   HttpRuntime.Cache.Insert(SurveyId, surveyInfoModel, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1));

					return surveyInfoModel;
			   }
			   else

			   {
				   return (SurveyInfoModel)CacheObj;

			   }*/

			SurveyInfoModel surveyInfoModel = new SurveyInfoModel();
			if (FormsHierarchyDTOList != null)
			{
				surveyInfoModel = Mapper.ToSurveyInfoModel(FormsHierarchyDTOList.FirstOrDefault(x => x.FormId == SurveyId).SurveyInfo);
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
			bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);

			SurveyAnswerRequest SARequest = new SurveyAnswerRequest();
			SARequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = Session[SessionKeys.RootResponseId].ToString() });
			SARequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			SARequest.Criteria.IsEditMode = this.IsEditMode;
			SARequest.Criteria.IsDeleteMode = true;
			SARequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
			// TODO: GEL - Delete from DocumentDB
			SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);

			return Json(Session[SessionKeys.RootFormId]);//string.Empty
			//return RedirectToAction("Index", "Home");
		}
		[HttpPost]

		public ActionResult DeleteBranch(string ResponseId)//List<FormInfoModel> ModelList, string formid)
		{

			SurveyAnswerRequest SARequest = new SurveyAnswerRequest();
			SARequest.SurveyAnswerList.Add(new SurveyAnswerDTO() { ResponseId = ResponseId });
			SARequest.Criteria.UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			SARequest.Criteria.IsEditMode = false;
			SARequest.Criteria.IsDeleteMode = true;
			SARequest.Criteria.IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];
			SARequest.Criteria.SurveyId = Session[SessionKeys.RootFormId].ToString();
			SurveyAnswerResponse SAResponse = _surveyFacade.DeleteResponse(SARequest);

			return Json(Session[SessionKeys.RootFormId]);//string.Empty
			//return RedirectToAction("Index", "Home");
		}
		[HttpGet]
		public ActionResult LogOut()
		{
			this.UpdateStatus(Session[SessionKeys.RootResponseId].ToString(), null, RecordStatus.InProcess, RecordStatusChangeReason.Logout);
			FormsAuthentication.SignOut();
			this.Session.Clear();
			return RedirectToAction("Index", "Login");
		}

		[HttpPost]
		public JsonResult AddChild(string SurveyId, int ViewId, string ResponseId, string FormValuesHasChanged, string CurrentPage)
		{
			Session[SessionKeys.RequestedViewId] = ViewId;
			//1-Get the child Id

			SurveyInfoRequest SurveyInfoRequest = new Enter.Common.Message.SurveyInfoRequest();
			SurveyInfoResponse SurveyInfoResponse = new Enter.Common.Message.SurveyInfoResponse();
			SurveyInfoDTO SurveyInfoDTO = new Enter.Common.DTO.SurveyInfoDTO();
			SurveyInfoDTO.SurveyId = SurveyId;
			SurveyInfoDTO.ViewId = ViewId;
			SurveyInfoRequest.SurveyInfoList.Add(SurveyInfoDTO);
			SurveyInfoResponse = _surveyFacade.GetChildFormInfo(SurveyInfoRequest);



			//3-Create a new response for the child 
			//string ChildResponseId = CreateResponse(SurveyInfoResponse.SurveyInfoList[0].SurveyId, ResponseId);
			string ChildResponseId = AddNewChild(SurveyInfoResponse.SurveyInfoList[0].SurveyId, ViewId, ResponseId, FormValuesHasChanged, CurrentPage);

			return Json(ChildResponseId);

		}
		private string AddNewChild(string surveyId, int viewId, string responseId, string formValuesHasChanged, string currentPage)
		{
			Session[SessionKeys.RequestedViewId] = viewId;
			bool IsMobileDevice = this.Request.Browser.IsMobileDevice;
			if (IsMobileDevice == false)
			{
				IsMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
			}
			int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());

			string ChildResponseId = CreateResponse(surveyId, responseId);
			this.UpdateStatus(responseId, surveyId, RecordStatus.Saved, RecordStatusChangeReason.NewChild);

			return ChildResponseId;
		}
		[HttpPost]
		public JsonResult HasResponse(string SurveyId, int ViewId, string ResponseId)
		{

			bool IsSqlProject = (bool)Session[SessionKeys.IsSqlProject];

			bool HasResponse = false;
			List<FormsHierarchyDTO> formsHierarchy = new List<FormsHierarchyDTO>();
			formsHierarchy = GetFormsHierarchy();
			var RelateSurveyId = formsHierarchy.Single(x => x.ViewId == ViewId);
			if (!IsSqlProject)
			{

				bool IsMobileDevice = this.Request.Browser.IsMobileDevice;
				if (IsMobileDevice == false)
				{
					IsMobileDevice = Epi.Web.MVC.Utility.SurveyHelper.IsMobileDevice(this.Request.UserAgent.ToString());
				}
				int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());

				int ResponseCount = GetResponseCount(formsHierarchy, ViewId, ResponseId);

				if (ResponseCount > 0)
				{


					HasResponse = true;
				}
			}
			else
			{
				// Get child count from Sql
				//1-Get the child Id
				//SurveyInfoResponse GetChildFormInfo(SurveyInfoRequest SurveyInfoRequest)

				HasResponse = _surveyFacade.HasResponse(RelateSurveyId.FormId.ToString(), ResponseId);
			}


			return Json(HasResponse);

		}
		public string CreateResponse(string surveyId, string relateResponseId)
		{
			int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
			bool.TryParse(Session[SessionKeys.IsEditMode].ToString(), out this.IsEditMode);
			//if (!string.IsNullOrEmpty(EditForm))
			//    {
			//    Epi.Cloud.Common.DTO.SurveyAnswerDTO surveyAnswerDTO = GetSurveyAnswer(EditForm);
			//    string ChildRecordId = GetChildRecordId(surveyAnswerDTO);

			//    }
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
			Guid ResponseID = Guid.NewGuid();
			TempData[Epi.Web.MVC.Constants.Constant.RESPONSE_ID] = ResponseID.ToString();

			// create the first survey response
			// SurveyAnswerDTO SurveyAnswer = _isurveyFacade.CreateSurveyAnswer(surveyModel.SurveyId, ResponseID.ToString());
			int CuurentOrgId = int.Parse(Session[SessionKeys.SelectedOrgId].ToString());
			SurveyAnswerDTO SurveyAnswer = _surveyFacade.CreateSurveyAnswer(surveyId, ResponseID.ToString(), UserId, true, relateResponseId, this.IsEditMode, CuurentOrgId);
			SurveyInfoModel surveyInfoModel = GetSurveyInfo(SurveyAnswer.SurveyId);

			// set the survey answer to be production or test 
			SurveyAnswer.IsDraftMode = surveyInfoModel.IsDraftMode;

			MvcDynamicForms.Form form = _surveyFacade.GetSurveyFormData(SurveyAnswer.SurveyId, 1, SurveyAnswer, IsMobileDevice,null,null,IsAndroid);

			TempData["Width"] = form.Width + 100;

            string checkcode = MetadataAccessor.GetFormDigest(surveyId).CheckCode;
            form.FormCheckCodeObj = form.GetCheckCodeObj(MetadataAccessor.GetFieldDigests(surveyId), SurveyAnswer.ResponseDetail, checkcode);

            ///////////////////////////// Execute - Record Before - start//////////////////////
            Dictionary<string, string> ContextDetailList = new Dictionary<string, string>();
			EnterRule FunctionObject_B = (EnterRule)form.FormCheckCodeObj.GetCommand("level=record&event=before&identifier=");
			SurveyResponseDocDb surveyResponseDocDb = new SurveyResponseDocDb(_pageFields, _requiredList);
			if (FunctionObject_B != null && !FunctionObject_B.IsNull())
			{
				try
				{

					PageDigest[] pageDigests = form.MetadataAccessor.GetCurrentFormPageDigests();
					var responseDetail = SurveyAnswer.ResponseDetail;

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

					_surveyFacade.UpdateSurveyResponse(surveyInfoModel, ResponseID.ToString(), form, SurveyAnswer, false, false, 0, SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()));
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

				SurveyAnswer.ResponseDetail = surveyResponseDocDb.CreateResponseDocument(pageDigestArray);

				this._requiredList = surveyResponseDocDb.RequiredList;
				Session[SessionKeys.RequiredList] = surveyResponseDocDb.RequiredList;
				form.RequiredFieldsList = _requiredList;
				_surveyFacade.UpdateSurveyResponse(surveyInfoModel, SurveyAnswer.ResponseId, form, SurveyAnswer, false, false, 0, SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString()));
			}

			var surveyAnswerResponse = _surveyFacade.GetSurveyAnswerResponse(SurveyAnswer.ResponseId, SurveyAnswer.SurveyId);
			if (surveyAnswerResponse != null && surveyAnswerResponse.SurveyResponseList.Count > 0)
			{
				SurveyAnswer = _surveyFacade.GetSurveyAnswerResponse(SurveyAnswer.ResponseId, SurveyAnswer.SurveyId).SurveyResponseList[0];
			}

			return ResponseID.ToString();
		}


		private MvcDynamicForms.Form SetFormPassCode(MvcDynamicForms.Form form, string responseId)
		{

			Epi.Web.Enter.Common.Message.UserAuthenticationResponse AuthenticationResponse = _securityFacade.GetAuthenticationResponse(responseId);

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
					form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, referrerPageNum, surveyAnswer, IsMobileDevice, null,   formsHierarchy,isAndroid);
					form.FormValuesHasChanged = formValuesHasChanged;
					formProvider.UpdateHiddenFields(referrerPageNum, form, this.ControllerContext.RequestContext.HttpContext.Request.Form);
				}
				else
				{
					form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, currentPageNumber, surveyAnswer, IsMobileDevice, null,   formsHierarchy,isAndroid );
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
				form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, GetSurveyPageNumber(surveyAnswer.ResponseDetail), surveyAnswer, IsMobileDevice, null, formsHierarchy,isAndroid);
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

		private void ExecuteRecordAfterCheckCode(MvcDynamicForms.Form form, SurveyInfoModel surveyInfoModel, SurveyAnswerDTO surveyAnswerDTO, string responseId, int pageNumber, int userId)
		{

			EnterRule functionObject_A = (EnterRule)form.FormCheckCodeObj.GetCommand("level=record&event=after&identifier=");
			if (functionObject_A != null && !functionObject_A.IsNull())
			{
				try
				{
					functionObject_A.Execute();
				}
				catch (Exception ex)
				{
					// do nothing so that processing can 
					// continue
				}
			}
			Dictionary<string, string> contextDetailList = new Dictionary<string, string>();
			contextDetailList = Epi.Web.MVC.Utility.SurveyHelper.GetContextDetailList(functionObject_A);
			form = Epi.Web.MVC.Utility.SurveyHelper.UpdateControlsValuesFromContext(form, contextDetailList);
			_surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswerDTO, false, false, pageNumber, userId);
		}

		private KeyValuePair<string, int> ValidateAll(MvcDynamicForms.Form form, int userId, bool isSubmited, bool isSaved, bool isMobileDevice, string formValuesHasChanged)
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
					SurveyAnswerDTO surveyAnswer = _surveyFacade.GetSurveyAnswerResponse(responseId, surveyAnswerDTO.SurveyId).SurveyResponseList[0];

					SurveyInfoModel surveyInfoModel = GetSurveyInfo(surveyAnswer.SurveyId, formsHierarchy);
					surveyAnswer.IsDraftMode = surveyInfoModel.IsDraftMode;
					form = UpdateSurveyModel(surveyInfoModel, isMobileDevice, formValuesHasChanged, surveyAnswer, true, formsHierarchy);
					var formProvider = new FormProvider(surveyId);
					for (int i = 1; i < form.NumberOfPages + 1; i++)
					{
						form = formProvider.GetForm(form.SurveyInfo, i, surveyAnswer);
						if (!form.Validate(form.RequiredFieldsList))
						{
							TempData["isredirect"] = "true";
							TempData["Width"] = form.Width + 5;
							_surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswer, isSubmited, isSaved, i, userId);

							result = new KeyValuePair<string, int>(responseId, i);
                            return result;
						}
					}
				}
			}

			return result;
		}

		private MvcDynamicForms.Form SaveCurrentForm(MvcDynamicForms.Form form, SurveyInfoModel surveyInfoModel, SurveyAnswerDTO surveyAnswerDTO, string responseId, int userId, bool isSubmited, bool isSaved,
			bool isMobileDevice, string formValuesHasChanged, int pageNumber, List<FormsHierarchyDTO> formsHierarchyDTOList = null
		)
		{
			surveyAnswerDTO = formsHierarchyDTOList.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == responseId);
#region Ananth
			surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;
			if (responseId != null && formValuesHasChanged == "True")
			{                 //Survey Info
                SurveyResponseBO surveyResponseBO = new SurveyResponseBO();  
                surveyResponseBO.ResponseId = responseId;
                surveyResponseBO.IsDraftMode = surveyInfoModel.IsDraftMode;
                surveyResponseBO.UserId = userId;
               // var isSuccesful = _surveyPersistenceFacade.InsertResponse(form, surveyResponseBO);              
            }

			bool IsAndroid = this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0;

			//SurveyAnswer = _isurveyFacade.GetSurveyAnswerResponse(responseId, surveyInfoModel.SurveyId).SurveyResponseList[0];
			surveyAnswerDTO = formsHierarchyDTOList.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == responseId);
#endregion Ananth

			surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;

			var lastPageNumber = GetSurveyPageNumber(surveyAnswerDTO.ResponseDetail);
			form = _surveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, lastPageNumber == 0 ? 1 : lastPageNumber, surveyAnswerDTO, isMobileDevice, null, formsHierarchyDTOList,IsAndroid );
			form.FormValuesHasChanged = formValuesHasChanged;

			UpdateModel(form);

			form.IsSaved = true;
			form.StatusId = surveyAnswerDTO.Status;

			// Pass Code Logic  start 
			form = SetFormPassCode(form, responseId);
			// Pass Code Logic  end
			 
			_surveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswerDTO, isSubmited, isSaved, pageNumber, userId);

			return form;
		}

		//private MvcDynamicForms.Form SaveCurrentForm(MvcDynamicForms.Form form, SurveyInfoModel surveyInfoModel, SurveyAnswerDTO surveyAnswerDTO, string responseId, int userId, bool isSubmited, bool isSaved,
		//	bool isMobileDevice, string formValuesHasChanged, int pageNumber, List<FormsHierarchyDTO> formsHierarchyDTOList = null
		//)
		//{
		//	surveyAnswerDTO = formsHierarchyDTOList.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == responseId);

		//	surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;
		//	surveyAnswerDTO.ResponseDetail.IsDraftMode = true;

		//	if (responseId != null && formValuesHasChanged == "True")
		//	{
		//		_isurveyDocumentDBStoreFacade.InsertResponseAsync(surveyInfoModel, responseId, form, surveyAnswerDTO, isSubmited, isSaved, pageNumber, userId);
		//	}

		//	bool IsAndroid = this.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0;

		//	//SurveyAnswer = _isurveyFacade.GetSurveyAnswerResponse(responseId, surveyInfoModel.SurveyId).SurveyResponseList[0];
		//	surveyAnswerDTO = formsHierarchyDTOList.SelectMany(x => x.ResponseIds).FirstOrDefault(z => z.ResponseId == responseId);

		//	surveyAnswerDTO.IsDraftMode = surveyInfoModel.IsDraftMode;

		//          var lastPageNumber = GetSurveyPageNumber(surveyAnswerDTO.ResponseDetail);
		//	form = _isurveyFacade.GetSurveyFormData(surveyInfoModel.SurveyId, lastPageNumber == 0 ? 1 : lastPageNumber, surveyAnswerDTO, isMobileDevice, null, formsHierarchyDTOList,IsAndroid );
		//	form.FormValuesHasChanged = formValuesHasChanged;

		//	UpdateModel(form);

		//	form.IsSaved = true;
		//	form.StatusId = surveyAnswerDTO.Status;

		//	// Pass Code Logic  start 
		//	form = SetFormPassCode(form, responseId);
		//	// Pass Code Logic  end 
		//	_isurveyFacade.UpdateSurveyResponse(surveyInfoModel, responseId, form, surveyAnswerDTO, isSubmited, isSaved, pageNumber, userId);

		//	return form;
		//}

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


            SurveyResponseDocDb surveyResponseHelper = new SurveyResponseDocDb();
			if (!string.IsNullOrEmpty(SurveyId))
			{
				SurveyAnswerRequest FormResponseReq = new SurveyAnswerRequest();
				FormSettingRequest FormSettingReq = new Enter.Common.Message.FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };

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

					if (item.SqlData != null)
					{
						ResponseList.Add(ConvertRowToModel(item, Columns, "ChildGlobalRecordID"));
					}
					else
					{
						ResponseList.Add(item.ToResponseModel(Columns));
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
			return Convert.ToInt16(WebConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"].ToString());
		}



		[HttpPost]

		public ActionResult ReadResponseInfo(string SurveyId, int ViewId, string ResponseId, string CurrentPage)//List<FormInfoModel> ModelList, string formid)
		// public ActionResult ReadResponseInfo( string ResponseId)//List<FormInfoModel> ModelList, string formid)
		{
			//var temp = SurveyModel;
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
				SurveyModel.RelateModel = Mapper.ToRelateModel(FormsHierarchy, SurveyId);
				SurveyModel.RequestedViewId = RequestedViewId;


				var RelateSurveyId = FormsHierarchy.Single(x => x.ViewId == ViewId);

				SurveyAnswerRequest FormResponseReq = new SurveyAnswerRequest();


				SurveyModel.FormResponseInfoModel = GetFormResponseInfoModel(RelateSurveyId.FormId, ResponseId, FormsHierarchy);
				SurveyModel.FormResponseInfoModel.NumberOfResponses = SurveyModel.FormResponseInfoModel.ResponsesList.Count();

				SurveyAnswerDTO surveyAnswerDTO = new SurveyAnswerDTO();

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

                return PartialView("ListResponses", SurveyModel);
			}
			else
			{
				return RedirectToAction("Index", "RelatedResponse", new { SurveyId = SurveyId, ViewId = ViewId, ResponseId = ResponseId, CurrentPage = CurrentPage });
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




