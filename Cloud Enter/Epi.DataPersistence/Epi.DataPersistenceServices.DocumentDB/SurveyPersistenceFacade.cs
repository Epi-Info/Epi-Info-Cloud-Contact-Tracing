using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.ServiceBus;
using Epi.DataPersistence.Common.Interfaces;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistenceServices.DocumentDB;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using Newtonsoft.Json;
using static Epi.PersistenceServices.DocumentDB.DataStructures;

namespace Epi.PersistenceServices.DocumentDB
{
    public class DocumentDBSurveyPersistenceFacade : MetadataAccessor, ISurveyPersistenceFacade
	{
		public DocumentDBSurveyPersistenceFacade()
		{
		}

		SurveyResponseCRUD _surveyResponseCRUD = new SurveyResponseCRUD();

        public bool DoesResponseExist(string childFormId, string parentResponseId)
        {
            return _surveyResponseCRUD.DoesResponseExist(childFormId, parentResponseId);
        }

		public bool DoChildrenExistForResponseId(string parentResponseId)
		{
			return _surveyResponseCRUD.DoChildrenExistForResponseId(parentResponseId);
		}

		public int GetFormResponseCount(string formId, bool includeDeletedRecords = false)
		{
			return _surveyResponseCRUD.GetFormResponseCount(formId, includeDeletedRecords);
		}

		public FormResponseDetail GetFormResponseState(string responseId)
		{
			var formResponseProperties = _surveyResponseCRUD.GetFormResponseState(responseId);
			return formResponseProperties != null ? formResponseProperties.ToFormResponseDetail() : null;
		}

		public bool UpdateResponseStatus(string responseId, int responseStatus, RecordStatusChangeReason reasonForStatusChange)
		{
			var result = _surveyResponseCRUD.UpdateResponseStatus(responseId, responseStatus);
            //NotifyConsistencyService(responseId, responseStatus, reasonForStatusChange);
            return true;
			//return result;
		}

		public async Task<bool> InsertResponse(SurveyResponseBO surveyResponseBO)
		{
            bool isSuccessful = false;
            var formId = surveyResponseBO.SurveyId;
            int pageId = 0;
            PageResponseProperties pageResponseProperties = new PageResponseProperties();
            if (surveyResponseBO.ResponseDetail.PageResponseDetailList.FirstOrDefault() != null)
            {
                pageId = Convert.ToInt32(surveyResponseBO.ResponseDetail.PageResponseDetailList[0].PageId);
            }
            var formDigest = GetFormDigest(formId);
            DocumentResponseProperties documentResponseProperties = CreateResponseDocumentInfo(formId, pageId);
            documentResponseProperties.GlobalRecordID = surveyResponseBO.ResponseId;
            documentResponseProperties.UserId = surveyResponseBO.UserId;
			documentResponseProperties.FormResponseProperties = surveyResponseBO.ResponseDetail.ToFormResponseProperties(); ;

			if (surveyResponseBO.ResponseDetail.PageResponseDetailList.Count >= 1 && surveyResponseBO.ResponseDetail.PageResponseDetailList[0] != null)
            {
                pageResponseProperties.ResponseQA = surveyResponseBO.ResponseDetail.PageResponseDetailList[0].ResponseQA;
                pageResponseProperties.GlobalRecordID = surveyResponseBO.ResponseId;
                pageResponseProperties.Id = surveyResponseBO.ResponseId;
                if (surveyResponseBO.ResponseDetail.PageResponseDetailList[0].PageId != 0)
                {
                    pageResponseProperties.PageId = surveyResponseBO.ResponseDetail.PageResponseDetailList[0].PageId;
                }

                documentResponseProperties.PageResponsePropertiesList = new List<PageResponseProperties>();
                documentResponseProperties.PageResponsePropertiesList.Add(pageResponseProperties);
                isSuccessful = await _surveyResponseCRUD.InsertResponseAsync(documentResponseProperties);
            }
            return isSuccessful;
		}


        #region Insert Survey Response
#if false
        public bool InsertResponse(Form form, SurveyResponseBO surveyResponseBO)
		{
				bool isSuccessful = false;
				var formId = form.SurveyInfo.SurveyId;
				var pageId = Convert.ToInt32(form.PageId);

				DocumentResponseProperties documentResponseProperties = CreateResponseDocumentInfo(formId, pageId);
				documentResponseProperties.GlobalRecordID = surveyResponseBO.ResponseId;
				documentResponseProperties.UserId = surveyResponseBO.UserId;
				documentResponseProperties.PageResponsePropertiesList.Add(form.ToPageResponseProperties(surveyResponseBO.ResponseId));
				//isSuccessful = _surveyResponseCRUD.InsertResponseAsync(documentResponseProperties).Result;
				return isSuccessful;
		}
#endif
#endregion

#region Insert Survey Response // Garry
#if false
		public async Task<bool> InsertResponseAsync(SurveyInfoModel surveyInfoModel, string responseId, Form form, SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int userId)
		{
			var formId = form.SurveyInfo.SurveyId;
			var pageId = Convert.ToInt32(form.PageId);
			var formDigest = GetFormDigest(formId);

			DocumentResponseProperties documentResponseProperties = CreateResponseDocumentInfo(formId, pageId);
			documentResponseProperties.GlobalRecordID = responseId;
            documentResponseProperties.UserId = surveyResponseBO.UserId;
			documentResponseProperties.PageResponsePropertiesList.Add(form.ToPageResponseProperties(responseId));

			bool isSuccessful = await _surveyResponse.InsertResponseAsync(documentResponseProperties, formDigest, userId);
			return isSuccessful;
		}
#endif
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
			var pageResponseProperties = _surveyResponseCRUD.GetPageResponsePropertiesByResponseId(responseId, formName, pageId);
			var pageResponseDetail = pageResponseProperties != null
								   ? pageResponseProperties.ToPageResponseDetail(formId, formName)
								   : null;
			return pageResponseDetail;
		}

#endregion

#region DeleteSurveyByResponseId
		public SurveyAnswerResponse DeleteResponse(string responseId, int userId)
		{
			bool deleteStatus=_surveyResponseCRUD.UpdateResponseStatus(responseId, RecordStatus.Deleted, userId).Result;

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
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task<bool> SaveFormProperties(SurveyResponseBO request)
		{
			//if(request.IsDeleteMode)
			//{
			//     request.SurveyAnswerList[0].Status = RecordStatus.Deleted;
			//}

			//var formName = GetFormDigest(request.Criteria.SurveyId).FormName;
			var formName = GetFormDigest(request.SurveyId).FormName;
			var now = DateTime.UtcNow;
			FormResponseProperties formResponseProperties = new FormResponseProperties
			{
				Id = request.ResponseId,
				GlobalRecordID = request.ResponseId,
                IsNewRecord = request.Status == RecordStatus.InProcess ? request.IsNewRecord : false,
				RecStatus = request.Status,
				FormId = request.SurveyId,
				FormName = formName,
				RelateParentId = request.RelateParentId,
				IsRelatedView = request.RelateParentId != null,
				FirstSaveTime = request.ResponseDetail.FirstSaveTime,
				LastSaveTime = now,
				FirstSaveLogonName = request.ResponseDetail.FirstSaveLogonName,
				UserId = request.UserId,
				IsDraftMode = request.IsDraftMode,
				PageIds = request.ResponseDetail.PageIds != null ? request.ResponseDetail.PageIds.Where(pid => pid != 0).ToList() : new List<int>(),
                RequiredFieldsList = request.ResponseDetail.RequiredFieldsList,
                HiddenFieldsList = request.ResponseDetail.HiddenFieldsList,
                HighlightedFieldsList = request.ResponseDetail.HighlightedFieldsList,
                DisabledFieldsList = request.ResponseDetail.DisabledFieldsList
            };

			var saveTask = await _surveyResponseCRUD.UpsertFormResponseProperties(formResponseProperties);
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

			var formDocumentDbEntity = _surveyResponseCRUD.GetAllPageResponsesByResponseId(responseId);
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
			surveyAnswerDTO.ReasonForStatusChange = RecordStatusChangeReason.Unknown;

			surveyResponseList.Add(surveyAnswerDTO);
			_surveyAnswerResponse.SurveyResponseList = surveyResponseList;
			return _surveyAnswerResponse;
		}
#endregion

#region Read All Records By SurveyID
		public IEnumerable<SurveyResponse> GetAllResponsesContainingFields(IDictionary<int, FieldDigest> gridFields)
		{
			return _surveyResponseCRUD.GetAllResponsesWithFieldNames(gridFields);
		}

		public FormResponseDetail GetFormResponseByResponseId(string responseId)
		{
			var response = _surveyResponseCRUD.GetAllPageResponsesByResponseId(responseId);
			var formResponseDetail = response.ToFormResponseDetail();
			return formResponseDetail;
		}
#endregion

#region Get Hierarchial ResponsesIds
        public FormResponseDetail GetHierarchialResponseIdsByResponseId(string responseId, bool includeDeletedRecords = false)
        {
            var hierarchicalDocumentResponseProperties = _surveyResponseCRUD.GetHierarchialResponseIdsByResponseId(responseId, includeDeletedRecords);
            var hierarchialFormResponseDetail = hierarchicalDocumentResponseProperties.ToFormResponseDetail();
            return hierarchialFormResponseDetail;
        }
#endregion

#region Get Hierarchial Responses for DataConsisitencyServiceAPI
        public FormResponseDetail GetHierarchialResponsesByResponseId(string responseId, bool includeDeletedRecords = false)
		{
			var hierarchicalDocumentResponseProperties = _surveyResponseCRUD.GetHierarchialResponsesByResponseId(responseId, includeDeletedRecords);
			var hierarchialFormResponseDetail = hierarchicalDocumentResponseProperties.ToFormResponseDetail();
			return hierarchialFormResponseDetail;
		}
#endregion


#region Create Resonse Document Info
        private DocumentResponseProperties CreateResponseDocumentInfo(string formId, int pageId)
		{
			var pageDigest = GetPageDigestByPageId(formId, pageId);

			DocumentResponseProperties documentResponseProperties = new DocumentResponseProperties();
			documentResponseProperties.FormName = pageDigest.FormName;
			documentResponseProperties.IsChildForm = pageDigest.IsRelatedView;
			return documentResponseProperties;
		}
#endregion

#region Notify Consistency Service
		public void NotifyConsistencyService(string responseId, int responseStatus, RecordStatusChangeReason reasonForStatusChange)
		{
			if (responseStatus == RecordStatus.Deleted || responseStatus == RecordStatus.Saved)
			{
				try
				{
					switch (reasonForStatusChange)
					{
						case RecordStatusChangeReason.SubmitOrClose:

							var serviceBusCRUD = new ServiceBusCRUD();
							//send notification to ServiceBus
							var hierarchialResponse = GetHierarchialResponsesByResponseId(responseId, true);
							var hierarchialResponseJson = JsonConvert.SerializeObject(hierarchialResponse);
							serviceBusCRUD.SendMessagesToTopic(responseId, hierarchialResponseJson);
							//ConsistencyHack(hierarchialResponse);
							break;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}

			}
		}
#endregion NotifyConsistencyService

		private void ConsistencyHack(FormResponseDetail formResponseDetail)
		{
			//var hack = new DataPersistence.ConsistencyServiceHack();
			//hack.PersistToSqlServer(formResponseDetail);
		}

	}
}