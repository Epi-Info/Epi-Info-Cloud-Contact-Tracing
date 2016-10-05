using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Extensions;
using Epi.Cloud.DataEntryServices.Model;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistenceServices.DocumentDB;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using MvcDynamicForms;
using static Epi.PersistenceServices.DocumentDB.DataStructures;
using Epi.Cloud.ServiceBus;
using Newtonsoft.Json;

namespace Epi.Cloud.DataEntryServices.Facade
{
	public class DocumentDBSurveyPersistenceFacade : MetadataAccessor, ISurveyPersistenceFacade
	{
		public DocumentDBSurveyPersistenceFacade(IProjectMetadataProvider projectMetadataProvider)
		{
			ProjectMetadataProvider = projectMetadataProvider;
		}


		SurveyResponseCRUD _surveyResponse = new SurveyResponseCRUD();

		/// <summary>
		/// Insert survey question and answer to Document Db
		/// </summary>

		public bool DoChildrenExistForResponseId(string responseId)
		{
			return _surveyResponse.DoChildrenExistForResponseId(responseId);
		}

		public int GetFormResponseCount(string formId, bool includeDeletedRecords = false)
		{
			return _surveyResponse.GetFormResponseCount(formId, includeDeletedRecords);
		}

		public FormResponseDetail GetFormResponseState(string responseId)
		{
			var formResponseProperties = _surveyResponse.GetFormResponseState(responseId);
			return formResponseProperties != null ? formResponseProperties.ToFormResponseDetail() : null;
		}

		public bool UpdateResponseStatus(string responseId, int responseStatus, RecordStatusChangeReason reasonForStatusChange)
		{
			var result = _surveyResponse.UpdateResponseStatus(responseId, responseStatus);
			NotifyConsistencyService(responseId, responseStatus, reasonForStatusChange);
			return result;
		}

		#region Insert Survey Response
		public async Task<bool> InsertResponseAsync(SurveyInfoModel surveyInfoModel, string responseId, Form form, SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int userId)
		{
			var formId = form.SurveyInfo.SurveyId;
			var pageId = Convert.ToInt32(form.PageId);
			var formDigest = GetFormDigest(formId);

			DocumentResponseProperties documentResponseProperties = CreateResponseDocumentInfo(formId, pageId);
			documentResponseProperties.GlobalRecordID = responseId;
			documentResponseProperties.PageResponsePropertiesList.Add(form.ToPageResponseProperties(responseId));

			bool isSuccessful = await _surveyResponse.InsertResponseAsync(documentResponseProperties, formDigest, userId);
			return isSuccessful;
		}
		#endregion

		#region Insert Child Survey Response
		public Task<bool> InsertChildResponseAsync(SurveyResponseBO surveyResponseBO)
		{
			var pageResponseDetail = surveyResponseBO.ResponseDetail.PageResponseDetailList.SingleOrDefault();
			var formId = surveyResponseBO.SurveyId;
			var pageId = pageResponseDetail != null ? pageResponseDetail.PageId : 0;

			var parentRecordId = surveyResponseBO.ParentRecordId;
			var relateParentId = surveyResponseBO.RelateParentId;

			DocumentResponseProperties documentResponseProperties = CreateResponseDocumentInfo(formId, pageId);
			documentResponseProperties.GlobalRecordID = surveyResponseBO.ResponseId;
			if (pageResponseDetail != null && pageResponseDetail.PageId > 0)
			{
				documentResponseProperties.PageResponsePropertiesList.Add(pageResponseDetail.ToPageResponseProperties());
			}
			return Task.FromResult(true);
		}
		#endregion

		#region ReadSurveyAnswerByResponseID,PageId 
		public PageResponseDetail ReadSurveyAnswerByResponseID(string formId, string responseId, int pageId)
		{
			var formDigest = GetFormDigest(formId);
			var formName = formDigest.FormName;
			var pageResponseProperties = _surveyResponse.GetPageResponsePropertiesByResponseId(responseId, formDigest, pageId);
			var pageResponseDetail = pageResponseProperties != null
								   ? pageResponseProperties.ToPageResponseDetail(formId, formName)
								   : null;
			return pageResponseDetail;
		}

		#endregion

		#region DeleteSurveyByResponseId
		public SurveyAnswerResponse DeleteResponse(string responseId, int userId)
		{
			_surveyResponse.UpdateResponseStatus(responseId, RecordStatus.Deleted, userId);

			SurveyAnswerResponse surveyAnsResponse = new SurveyAnswerResponse();
			//var tasks = _surveyResponse.DeleteDocumentByIdAsync(SARequest);
			//var result = tasks.Result;
			return surveyAnsResponse;
		}

		#endregion

		//public FormsHierarchyDTO GetChildRecordByChildFormId(string childFormId, string relateParentId, IDictionary<int, FieldDigest> fields)
		//{
		//	FormsHierarchyDTO formHierarchyDTO = new FormsHierarchyDTO();
		//	var childRecords = _surveyResponse.GetAllResponsesWithFieldNames(fields, relateParentId);
		//	formHierarchyDTO.ResponseIds = new List<SurveyAnswerDTO>();
		//	foreach (var item in childRecords)
		//	{
		//		SurveyAnswerDTO surveyAnswer = new SurveyAnswerDTO();
		//		surveyAnswer.ResponseId = item.ResponseId.ToString();
		//		surveyAnswer.SqlData = item.ResponseDetail.FlattenedResponseQA(key => key.ToLower());
		//		formHierarchyDTO.ResponseIds.Add(surveyAnswer);
		//	}
		  
		//	return formHierarchyDTO;
		//}

		#region Save FormParentProperties
		/// <summary>
		/// First time store ResonseId,RecStatus, and SurveyId in DocumentDB
		/// </summary>
		/// <param name="ProjectMetaData"></param>
		/// <param name="Status"></param>
		/// <param name="UserId"></param>
		/// <param name="ResponseId"></param>
		/// <returns></returns>
		public bool SaveFormProperties(SurveyAnswerRequest request)
		{

			DocumentResponseProperties documentResponseProperties = new DocumentResponseProperties(); 
			if(request.Criteria.IsDeleteMode)
			{
				request.SurveyAnswerList[0].Status = RecordStatus.Deleted;
			}

			var formName = GetFormDigest(request.Criteria.SurveyId).FormName;
			var now = DateTime.UtcNow;
			FormResponseProperties formResponseProperties = new FormResponseProperties
			{
				RecStatus = request.SurveyAnswerList[0].Status,
				FormId = request.Criteria.SurveyId,
				FormName = formName,
				GlobalRecordID = request.SurveyAnswerList[0].ResponseId,
				RelateParentId = request.SurveyAnswerList[0].RelateParentId,
				IsRelatedView = request.SurveyAnswerList[0].RelateParentId != null,
				FirstSaveTime = now,
				LastSaveTime = now,
				UserId = request.Criteria.UserId,
				IsDraftMode = request.Criteria.IsDraftMode
			};

			documentResponseProperties.FormResponseProperties = new FormResponseProperties();
			documentResponseProperties.FormResponseProperties = formResponseProperties;
			documentResponseProperties.GlobalRecordID = request.RequestId; 
			var saveTask = _surveyResponse.SaveResponseAsync(documentResponseProperties);
			return true;
		}

		#endregion

		#region Get the Record by GlobalRecordID
		public SurveyAnswerResponse GetSurveyAnswerResponse(string responseId)
		{
			// TODO Implement GetSurveyAnswerResponse
			return null;
		}
		#endregion

		#region Get Survey Answer Response
		public SurveyAnswerResponse GetSurveyAnswerResponse(string responseId, int userId)
		{ 
			SurveyAnswerResponse _surveyAnswerResponse = new SurveyAnswerResponse();
			_surveyAnswerResponse.SurveyResponseList = new List<SurveyAnswerDTO>();

			var formDocumentDbEntity = _surveyResponse.GetAllPageResponsesByResponseId(responseId);
			List<SurveyAnswerDTO> surveyResponseList = new List<SurveyAnswerDTO>();
			SurveyAnswerDTO surveyAnswerDTO = new SurveyAnswerDTO();
			surveyAnswerDTO.ResponseId = responseId;
			surveyAnswerDTO.SurveyId = formDocumentDbEntity.FormResponseProperties.FormId;

			surveyAnswerDTO.ResponseDetail = formDocumentDbEntity.ToFormResponseDetail();

			surveyAnswerDTO.DateCreated = formDocumentDbEntity.FormResponseProperties.FirstSaveTime;
			surveyAnswerDTO.DateUpdated = formDocumentDbEntity.FormResponseProperties.LastSaveTime;
			surveyAnswerDTO.RelateParentId = formDocumentDbEntity.FormResponseProperties.RelateParentId;
			surveyAnswerDTO.LastActiveUserId = formDocumentDbEntity.FormResponseProperties.UserId;
			surveyAnswerDTO.Status = RecordStatus.Saved;
			surveyResponseList.Add(surveyAnswerDTO); 
			_surveyAnswerResponse.SurveyResponseList = surveyResponseList;
			return _surveyAnswerResponse;
		}
		#endregion

		#region Read All Records By SurveyID
		public IEnumerable<SurveyResponse> GetAllResponsesContainingFields(IDictionary<int, FieldDigest> gridFields)
		{
			return _surveyResponse.GetAllResponsesWithFieldNames(gridFields);
		}

		public FormResponseDetail GetFormResponseByResponseId(string responseId)
		{
			var response = _surveyResponse.GetAllPageResponsesByResponseId(responseId);
			var formResponseDetail = response.ToFormResponseDetail();
			return formResponseDetail;
		}
		#endregion


		#region Get Forminfo for DataConsisitencyServiceAPI
		public FormResponseDetail GetHierarchialResponsesByResponseId(string responseId, bool includeDeletedRecords = false)
		{
			var hierarchicalDocumentResponseProperties = _surveyResponse.GetHierarchialResponsesByResponseId(responseId, includeDeletedRecords);
			var hierarchialFormResponseDetail = hierarchicalDocumentResponseProperties.ToFormResponseDetail();
			return hierarchialFormResponseDetail;
		}
		#endregion


		#region Create Resonse Document Info
		private Epi.PersistenceServices.DocumentDB.DataStructures.DocumentResponseProperties CreateResponseDocumentInfo(string formId, int pageId)
		{
			var pageDigest = GetPageDigestByPageId(formId, pageId);

			Epi.PersistenceServices.DocumentDB.DataStructures.DocumentResponseProperties survey = new Epi.PersistenceServices.DocumentDB.DataStructures.DocumentResponseProperties();
			survey.FormName = pageDigest.FormName;
			survey.IsChildForm = pageDigest.IsRelatedView;
			survey.CollectionName = pageDigest.FormName + pageDigest.PageId;
			return survey;
		}
		#endregion

		#region Notify Consistency Service
		public void NotifyConsistencyService(string responseId, int responseStatus, RecordStatusChangeReason reasonForStatusChange)
		{
			if (responseStatus == RecordStatus.Deleted || responseStatus == RecordStatus.Saved)
			{
				try
				{
					var serviceBusCRUD = new ServiceBusCRUD();
					//send notification to ServiceBus
					serviceBusCRUD.SendMessagesToTopic(responseId, responseId);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}

		#endregion NotifyConsistencyService

	}
}