using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Security;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.SurveyInfoServices.Extensions;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Model;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Common.Constants;
using Epi.Common.Core.DataStructures;
using Epi.Cloud.Common.Extensions;
using Epi.Common.Core.Interfaces;

namespace Epi.Web.MVC.Utility
{
    public class SurveyHelper
	{
        /// <summary>
        /// Creates the first survey response in the response table
        /// </summary>
        /// <param name="responseContext"></param>
        /// <param name="surveyAnswerRequest1"></param>
        /// <param name="surveyAnswerDTO"></param>
        /// <param name="surveyResponseBuilder"></param>
        /// <param name="dataEntryService"></param>
        /// <param name="isEditMode"></param>
        /// <param name="currentOrgId"></param>
        /// <returns></returns>
		public static SurveyAnswerDTO CreateSurveyResponse(IResponseContext responseContext,
														SurveyAnswerRequest surveyAnswerRequest1,
														SurveyAnswerDTO surveyAnswerDTO,
														SurveyResponseBuilder surveyResponseBuilder,
														IDataEntryService dataEntryService,
														bool isEditMode = false,
														int currentOrgId = -1)
		{
			bool AddRoot = false;
			SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
			surveyAnswerRequest.ResponseContext = responseContext;

            responseContext.ToSurveyAnswerDTO(surveyAnswerDTO);
			surveyAnswerDTO.DateCreated = DateTime.UtcNow;
			surveyAnswerDTO.SurveyId = responseContext.FormId;
			surveyAnswerDTO.Status = RecordStatus.InProcess;
            surveyAnswerDTO.LastActiveUserId = responseContext.UserId;
            surveyAnswerDTO.LoggedInUserId = responseContext.UserId;
			surveyAnswerDTO.RecordSourceId = RecordSource.CloudEnter;
			//if (isEditMode)
			//{
			//	surveyAnswerDTO.ParentResponseId = responseContext.ParentResponseId;
			//}


            FormResponseDetail responseDetail = responseContext.ToFormResponseDetail();
            surveyAnswerDTO.ResponseDetail = responseDetail;

			surveyAnswerDTO.ParentResponseId = responseContext.ParentResponseId;
			surveyAnswerRequest.Criteria.UserId = responseContext.UserId;
            surveyAnswerRequest.Criteria.UserName = responseContext.UserName;

            surveyAnswerRequest.Criteria.UserOrganizationId = currentOrgId;
			surveyAnswerRequest.SurveyAnswerList.Add(surveyAnswerDTO);

			if (!responseContext.IsChildResponse)
			{
				surveyAnswerRequest.Action = RequestAction.Create;
			}
			else
			{
				//if (isEditMode)
				//{

				//	surveyAnswerRequest.SurveyAnswerList[0].ParentResponseId = null;
				//}

				surveyAnswerRequest.Action = RequestAction.CreateChild;

			}

			dataEntryService.SetSurveyAnswer(surveyAnswerRequest);

			return surveyAnswerDTO;
		}

		public static void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel,
												MvcDynamicForms.Form form,
												SurveyAnswerRequest surveyAnswerRequest,
												SurveyResponseBuilder surveyResponseBuilder,
												IDataEntryService dataEntryService,
												SurveyAnswerResponse surveyAnswerResponse,
												string responseId,
												SurveyAnswerDTO surveyAnswerDTO,
												bool isSubmited,
												bool isSaved,
												int pageNumber,
												int userId)
		{
			// 1 Get the record for the current survey response
			// 2 update the current survey response
			// 3 save the current survey response

			var savedResponseDetail = surveyAnswerDTO.ResponseDetail;

			if (!isSubmited)
			{
				// 2 a. update the current survey answer request
				surveyAnswerRequest.SurveyAnswerList = surveyAnswerResponse.SurveyResponseList;

				surveyResponseBuilder.Add(form);

				FormResponseDetail formResponseDetail = surveyResponseBuilder.UpdateResponseDetail(savedResponseDetail, form.CurrentPage, form.PageId);

                surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail = formResponseDetail;
				// 2 b. save the current survey response
				surveyAnswerRequest.Action = RequestAction.Update;

				var currentPageNumber = form.CurrentPage;
				FormResponseDetail currentFormResponseDetail = surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail;
				PageResponseDetail currentPageResponseDetail = currentFormResponseDetail.GetPageResponseDetailByPageNumber(currentPageNumber);

				var mergedResponseDetail = MergeResponseDetail(savedResponseDetail, currentPageResponseDetail);
				surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail.PageIds = mergedResponseDetail.PageIds;
				// keep only the pages that have updates
				var updatedPageResponseDetailList = mergedResponseDetail.PageResponseDetailList.ToList();    
                surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail.PageResponseDetailList.Clear();
				surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail.PageResponseDetailList.AddRange(updatedPageResponseDetailList);
			}

			var updatedFromResponseDetail = surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail;
            updatedFromResponseDetail.UserId = userId;

			////Update page number before saving response 
			if (surveyAnswerRequest.SurveyAnswerList[0].CurrentPageNumber != 0)
			{
				updatedFromResponseDetail.LastPageVisited = pageNumber;
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
            List<KeyValuePair<string, string>> fieldsList = GetHiddenFieldsList(form).Where(kvp => kvp.Value != null).ToList();

            if (fieldsList.Count > 0)
            {
                var formId = form.SurveyInfo.SurveyId;
                var metadataAccessor = form.SurveyInfo as MetadataAccessor;
                var formDigest = metadataAccessor.GetFormDigest(formId);
                foreach (var fieldsListKvp in fieldsList)
                {
                    var fieldName = fieldsListKvp.Key.ToLower();
                    var pageId = formDigest.FieldNameToPageId(fieldName);
                    var pageResponseDetail = updatedFromResponseDetail.PageResponseDetailList.SingleOrDefault(p => p.PageId == pageId);
                    if (pageResponseDetail == null)
                    {
                        var pageDigest = metadataAccessor.GetPageDigestByPageId(formId, pageId);
                        pageResponseDetail = new PageResponseDetail { PageId = pageId, PageNumber = pageDigest.PageNumber };
                        updatedFromResponseDetail.AddPageResponseDetail(pageResponseDetail);
                    }
                    pageResponseDetail.ResponseQA[fieldName] = fieldsListKvp.Value;
                    pageResponseDetail.HasBeenUpdated = true;
                }
            }

			if (isSaved)
			{
				surveyAnswerRequest.SurveyAnswerList[0].Status = RecordStatus.Saved;
				surveyAnswerRequest.SurveyAnswerList[0].ReasonForStatusChange = RecordStatusChangeReason.Update;
			}

			/////Update Survey Mode ////////////////////
			surveyAnswerRequest.SurveyAnswerList[0].IsDraftMode = surveyAnswerDTO.IsDraftMode;
            //surveyAnswerRequest.Criteria.UserId = UserId;
            ResponseContext responseContext = ((IResponseContext)updatedFromResponseDetail).Clone();
            responseContext.UserId = userId;

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
				SurveyInfoDTO result = surveyInfoService.GetSurveyInfoByFormId(surveyId).ToSurveyInfoDTO();
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

		public static List<KeyValuePair<string, string>> GetHiddenFieldsList(MvcDynamicForms.Form form)
		{
            List<KeyValuePair<string, String>> hiddenFields = form.InputFields.Where(field => field.IsPlaceHolder).Select(field => new KeyValuePair<string, string>(field.Title, field.Response)).ToList();
			return hiddenFields;
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
				DecryptedUserId = Epi.Common.Security.Cryptography.Decrypt(Id);
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

