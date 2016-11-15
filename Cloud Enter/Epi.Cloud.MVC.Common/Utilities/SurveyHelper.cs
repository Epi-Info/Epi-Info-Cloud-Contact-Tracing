using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Security;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.SurveyInfoServices.Extensions;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.Model;

namespace Epi.Web.MVC.Utility
{
    public class SurveyHelper
	{
        /// <summary>
        /// Creates the first survey response in the response table
        /// </summary>
        /// <param name="surveyId"></param>
        /// <param name="responseId"></param>
        /// <param name="surveyAnswerRequest1"></param>
        /// <param name="surveyAnswerDTO"></param>
        /// <param name="surveyResponseHelper"></param>
        /// <param name="dataEntryService"></param>
        /// <param name="UserId"></param>
        /// <param name="IsChild"></param>
        /// <param name="RelateResponseId"></param>
        /// <param name="IsEditMode"></param>
        /// <param name="CurrentOrgId"></param>
        /// <returns></returns>
		public static SurveyAnswerDTO CreateSurveyResponse(string surveyId,
														string responseId, 
														SurveyAnswerRequest surveyAnswerRequest1,
														Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO,
														SurveyResponseHelper surveyResponseHelper,
														IDataEntryService dataEntryService,
														int UserId,
														bool IsChild = false,
														string RelateResponseId = "",
														bool IsEditMode = false,
														int CurrentOrgId = -1)


		{
			bool AddRoot = false;
			SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
			surveyAnswerRequest.Criteria.SurveyAnswerIdList.Add(responseId.ToString());
			surveyAnswerDTO.ResponseId = responseId.ToString();
			surveyAnswerDTO.DateCreated = DateTime.UtcNow;
			surveyAnswerDTO.SurveyId = surveyId;
			surveyAnswerDTO.Status = RecordStatus.InProcess;
			surveyAnswerDTO.RecordSourceId = RecordSource.WebEnter;
			if (IsEditMode)
			{
				surveyAnswerDTO.ParentRecordId = RelateResponseId;
			}
			//if (IsEditMode)
			//    {
			//    surveyAnswerDTO.Status = RecordStatus.Complete;
			//    }
			//else
			//    {
			//    surveyAnswerDTO.Status = RecordStatus.InProcess;
			//    }

			FormResponseDetail responseDetail = surveyResponseHelper.CreateResponseDetail(surveyId, AddRoot, 0, "");
			surveyAnswerDTO.ResponseDetail = responseDetail;

			surveyAnswerDTO.RelateParentId = RelateResponseId;
			surveyAnswerRequest.Criteria.UserId = UserId;
			surveyAnswerRequest.Criteria.UserOrganizationId = CurrentOrgId;
			surveyAnswerRequest.SurveyAnswerList.Add(surveyAnswerDTO);
			if (!IsChild)
			{
				surveyAnswerRequest.Action = Epi.Cloud.Common.Constants.Constant.CREATE;
			}
			else
			{
				if (IsEditMode)
				{

					surveyAnswerRequest.SurveyAnswerList[0].ParentRecordId = null;
				}

				surveyAnswerRequest.Action = Epi.Cloud.Common.Constants.Constant.CREATECHILD;

			}

			dataEntryService.SetSurveyAnswer(surveyAnswerRequest);

			return surveyAnswerDTO;
		}

		public static void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel,
												MvcDynamicForms.Form form,
												SurveyAnswerRequest surveyAnswerRequest,
												SurveyResponseHelper surveyResponseHelper,
												IDataEntryService dataEntryService,
												SurveyAnswerResponse surveyAnswerResponse,
												string responseId,
												SurveyAnswerDTO surveyAnswerDTO,
												bool IsSubmited,
												bool IsSaved,
												int PageNumber,
												int UserId)
		{
			// 1 Get the record for the current survey response
			// 2 update the current survey response
			// 3 save the current survey response

			var savedResponseDetail = surveyAnswerDTO.ResponseDetail;

			if (!IsSubmited)
			{
				// 2 a. update the current survey answer request
				surveyAnswerRequest.SurveyAnswerList = surveyAnswerResponse.SurveyResponseList;

				surveyResponseHelper.Add(form);
				bool addRoot = false;

				FormResponseDetail responseDetail = surveyResponseHelper.CreateResponseDetail(surveyInfoModel.SurveyId, addRoot, form.CurrentPage, form.PageId);

				surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail = responseDetail;
				// 2 b. save the current survey response
				surveyAnswerRequest.Action = Epi.Cloud.Common.Constants.Constant.UPDATE;

				var currentPageNumber = form.CurrentPage;
				FormResponseDetail currentFormResponseDetail = surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail;
				PageResponseDetail currentPageResponseDetail = currentFormResponseDetail.GetPageResponseDetailByPageNumber(currentPageNumber);
				if (addRoot == false)
				{
					var mergedResponseDetail = MergeResponseDetail(savedResponseDetail, currentPageResponseDetail);
					surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail.PageIds = mergedResponseDetail.PageIds;
					// keep only the pages that have updates
					var updatedPageResponseDetailList = mergedResponseDetail.PageResponseDetailList.Where(p => p.HasBeenUpdated).ToList();
					surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail.PageResponseDetailList.Clear();
					surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail.PageResponseDetailList.AddRange(updatedPageResponseDetailList);
				}
			}

			var updatedFromResponseDetail = surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail;

			////Update page number before saving response 
			if (surveyAnswerRequest.SurveyAnswerList[0].CurrentPageNumber != 0)
			{
				updatedFromResponseDetail.LastPageVisited = PageNumber;
			}
			if (form.HiddenFieldsList != null)
			{
				updatedFromResponseDetail.HiddenFieldsList = form.HiddenFieldsList;
			}
			if (form.HighlightedFieldsList != null)
			{
				updatedFromResponseDetail.HighlightedFieldsList = form.HighlightedFieldsList;
			}
			if (form.DisabledFieldsList != null)
			{
				updatedFromResponseDetail.DisabledFieldsList = form.DisabledFieldsList;
			}
			if (form.RequiredFieldsList != null)
			{
				updatedFromResponseDetail.RequiredFieldsList = form.RequiredFieldsList;
			}

			//  AssignList 
			List<KeyValuePair<string, String>> FieldsList = new List<KeyValuePair<string, string>>();

			FieldsList = GetHiddenFieldsList(form);
			// form.AssignList = FieldsList;
			if (FieldsList.Count > 0)
			{
				var pageList = form.SurveyInfo.PageDigests;
				for (var i = 0; i < pageList.Length; i++)
				{
					for (var j = 0; j < pageList[i].Length; j++)
					{
						var fields = pageList[i][j].Fields;
						foreach (var fieldx in fields)
						{
							foreach (var k in FieldsList)
							{
								if (fieldx.FieldName == k.Key)
								{
									if (k.Value != null)
										fieldx.Value = k.Value;
								}
							}
						}
					}
				}
			}

			if (IsSaved)
			{
				surveyAnswerRequest.SurveyAnswerList[0].Status = RecordStatus.Saved;
				surveyAnswerRequest.SurveyAnswerList[0].ReasonForStatusChange = RecordStatusChangeReason.Update;
			}

			/////Update Survey Mode ////////////////////
			surveyAnswerRequest.SurveyAnswerList[0].IsDraftMode = surveyAnswerDTO.IsDraftMode;
			surveyAnswerRequest.Criteria.UserId = UserId;
			dataEntryService.SetSurveyAnswer(surveyAnswerRequest);
		}

        /// <summary>
        /// Returns a SurveyInfoDTO object
        /// </summary>
        /// <param name="surveyInfoRequest"></param>
        /// <param name="surveyInfoService"></param>
        /// <param name="surveyId"></param>
        /// <returns></returns>
		public static SurveyInfoDTO GetSurveyInfoDTO(SurveyInfoRequest surveyInfoRequest,
													 ISurveyInfoService surveyInfoService,
													 string surveyId)
		{
			surveyInfoRequest.Criteria.SurveyIdList.Add(surveyId);
			try
			{
				SurveyInfoDTO result = surveyInfoService.GetSurveyInfoById(surveyId).ToSurveyInfoDTO();
				return result;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		///   This function will loop through the form controls and checks if any of the controls are found in the context detail list. 
		///   If any their values get updated from the context list.
		/// </summary>
		/// <param name="form"></param>
		/// <param name="ContextDetailList"></param>
		/// <returns>Returns a Form object</returns>
		public static MvcDynamicForms.Form UpdateControlsValuesFromContext(MvcDynamicForms.Form form, Dictionary<string, string> ContextDetailList)
		{



			Dictionary<string, string> formControlList = new Dictionary<string, string>();

			foreach (var field in form.InputFields)
			{
				string fieldName = field.Title;

				if (ContextDetailList.ContainsKey(fieldName))
				{
					field.Response = ContextDetailList[fieldName].ToString();
				}

			}



			return form;
		}
		public static MvcDynamicForms.Form UpdateControlsValues(MvcDynamicForms.Form form, string Name, string Value)
		{
			foreach (var field in form.InputFields)
			{
				string fieldName = field.Title;

				if (Name.ToLower() == fieldName.ToLower())
				{
					field.Response = Value.ToString();
				}

			}

			return form;
		}
		public static Dictionary<string, string> GetContextDetailList(Epi.Core.EnterInterpreter.EnterRule FunctionObject)
		{
			Dictionary<string, string> ContextDetailList = new Dictionary<string, string>();

			if (FunctionObject != null && !FunctionObject.IsNull())
			{
				foreach (KeyValuePair<string, EpiInfo.Plugin.IVariable> kvp in FunctionObject.Context.CurrentScope.SymbolList)
				{
					EpiInfo.Plugin.IVariable field = kvp.Value;

					if (!string.IsNullOrEmpty(field.Expression))
					{
						if (field.DataType == EpiInfo.Plugin.DataType.Date)
						{
							var datetemp = string.Format("{0:MM/dd/yyyy}", field.Expression);
							DateTime date = new DateTime();
							date = Convert.ToDateTime(datetemp);
							ContextDetailList[kvp.Key] = date.Date.ToString("MM/dd/yyyy");
						}
						else
						{
							ContextDetailList[kvp.Key] = field.Expression;
						}
					}
				}
			}

			return ContextDetailList;
		}

		public static List<KeyValuePair<string, string>> GetHiddenFieldsList(MvcDynamicForms.Form pForm)
		{
			List<KeyValuePair<string, String>> FieldsList = new List<KeyValuePair<string, string>>();

			foreach (var field in pForm.InputFields)
			{
				if (field.IsPlaceHolder)
				{
					FieldsList.Add(new KeyValuePair<string, string>(field.Title, field.Response));

				}
			}

			return FieldsList;
		}
		public static void UpdatePassCode(UserAuthenticationRequest AuthenticationRequest, ISecurityDataService securityDataService)
		{
			securityDataService.SetPassCode(AuthenticationRequest);
		}

		public static string GetPassCode()
		{
			Guid Guid = Guid.NewGuid();
			string Passcode = Guid.ToString().Substring(0, 4);
			return Passcode;
		}

		public static bool IsGuid(string expression)
		{
			if (expression != null)
			{
				Regex guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");

				return guidRegEx.IsMatch(expression);
			}
			return false;
		}

		public static bool IsMobileDevice(string RequestUserAgent)
		{


			if (RequestUserAgent.IndexOf("Opera Mobi", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Opera Mobi"))
			{
				return true;
			}
			else if (RequestUserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Android"))
			{
				return true;
			}
			else if (RequestUserAgent.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Mobile"))
			{
				return true;
			}
			else if (RequestUserAgent.IndexOf("Phone", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Phone"))
			{
				return true;
			}
			else if (RequestUserAgent.IndexOf("Opera Mini", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Opera Mini"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static FormResponseDetail MergeResponseDetail(FormResponseDetail savedResponseDetail, PageResponseDetail currentPageResponseDetail)
		{
			savedResponseDetail = savedResponseDetail ?? new FormResponseDetail
			{
				FormId = currentPageResponseDetail.FormId,
				FormName = currentPageResponseDetail.FormName
			};
			savedResponseDetail.LastPageVisited = currentPageResponseDetail.PageNumber;
			savedResponseDetail.MergePageResponseDetail(currentPageResponseDetail);
			return savedResponseDetail;
		}

		public static int GetDecryptUserId(string Id)
		{
			string DecryptedUserId = "";
			try
			{
				DecryptedUserId = Epi.Web.Enter.Common.Security.Cryptography.Decrypt(Id);
			}
			catch (Exception ex)
			{
				FormsAuthentication.SignOut();
				FormsAuthentication.RedirectToLoginPage();

			}
			int UserId = -1;
			int.TryParse(DecryptedUserId, out UserId);

			return UserId;
		}
	}
}

