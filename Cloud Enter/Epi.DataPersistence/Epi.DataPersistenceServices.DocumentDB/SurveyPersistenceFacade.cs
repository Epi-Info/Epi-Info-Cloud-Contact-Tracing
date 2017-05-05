using System;
using System.Collections.Generic;
using System.Configuration;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.ServiceBus;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Common.Interfaces;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistenceServices.DocumentDB;
using Epi.FormMetadata.DataStructures;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Epi.PersistenceServices.DocumentDB
{
	public partial class DocumentDBSurveyPersistenceFacade : MetadataAccessor, ISurveyPersistenceFacade
	{
		private string AttachmentId = ConfigurationManager.AppSettings[AppSettings.Key.AttachmentId];

		public DocumentDBSurveyPersistenceFacade()
		{
		}

		SurveyResponseCRUD _surveyResponseCRUD = new SurveyResponseCRUD();

		public FormResponseDetail GetFormResponseState(IResponseContext responseContext)
		{
			var formResponseProperties = _surveyResponseCRUD.GetFormResponseState(responseContext);
			return formResponseProperties != null ? formResponseProperties.ToFormResponseDetail() : null;
		}

        public bool DoChildResponsesExist(IResponseContext responseContext, bool includeDeletedRecords = false) // string childFormId, string parentResponseId)
        {
			return _surveyResponseCRUD.DoChildResponsesExist(responseContext, includeDeletedRecords);
		}

		public int GetFormResponseCount(string formId, bool includeDeletedRecords = false)
		{
			return _surveyResponseCRUD.GetFormResponseCount(formId, includeDeletedRecords);
		}

		public bool UpdateResponseStatus(IResponseContext responseContext, int responseStatus, RecordStatusChangeReason reasonForStatusChange)
		{
			var formId = responseContext.FormId;
			var responseId = responseContext.ResponseId;

			Attachment attachment = null;
			try
			{
				switch (responseStatus)
				{
					case RecordStatus.Saved:
						var UpdateAttachmentresult = _surveyResponseCRUD.UpdateAttachment(responseContext, responseStatus);
						break;
					case RecordStatus.Deleted:
						var UpdateSurveyResponseStatusToDeleteResult = _surveyResponseCRUD.Delete(responseContext, RecordStatus.Deleted);
						//NotifyConsistencyService(responseId, RecordStatus.Deleted, RecordStatusChangeReason.DeleteResponse);
						break;
					case RecordStatus.Restore:
						attachment = _surveyResponseCRUD.ReadAttachment(formId, responseId, AttachmentId);
						if (attachment == null)
						{
							//Add new record Don't Save
							var NewRecordDontSaveResult = _surveyResponseCRUD.Delete(responseContext, RecordStatus.PhysicalDelete);
						}
						else
						{
							//Edit Record Don't Save
							FormResponseResource formResonseResource = _surveyResponseCRUD.RetrieveAttachment(attachment);
							var editRecordDontSaveResult = _surveyResponseCRUD.RestoreLastResponseSnapshot(formResonseResource);

							//Delete Attachment                         
							var deleteResponse = _surveyResponseCRUD.DeleteAttachment(attachment);
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return true;
		}


		public bool SaveResponse(SurveyResponseBO surveyResponseBO)
		{
            var responseContext = ResponseContextExtensions.Clone(surveyResponseBO);

			bool saveFormPropertiesIsSuccessful = SaveFormResponseProperties(surveyResponseBO);
			if (surveyResponseBO.Status == RecordStatus.Saved)
			{
				NotifyConsistencyService(surveyResponseBO, surveyResponseBO.Status, RecordStatusChangeReason.SubmitOrClose);
			}
			return saveFormPropertiesIsSuccessful;
		}

        #region Save Form Response Properties
        /// <summary>
        /// First time store ResponseId, RecStatus, and SurveyId in DocumentDB
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool SaveFormResponseProperties(SurveyResponseBO request)
        {
            ResponseContext responseContext = request.ToResponseContext();
            var formName = responseContext.FormName;
            var now = DateTime.UtcNow;
            FormResponseProperties formResponseProperties = new FormResponseProperties
            {
                ResponseId = responseContext.ResponseId,
                FormId = responseContext.FormId,
                FormName = responseContext.FormName,

                ParentResponseId = responseContext.ParentResponseId,
                ParentFormId = responseContext.ParentFormId,
                ParentFormName = responseContext.ParentFormName,

                RootResponseId = responseContext.RootResponseId,
                RootFormId = responseContext.RootFormId,
                RootFormName = responseContext.RootFormName,

                UserId = responseContext.UserId,
                UserName = request.UserName,

                IsNewRecord = request.Status == RecordStatus.InProcess ? request.IsNewRecord : false,
                RecStatus = request.Status,
                FirstSaveTime = request.ResponseDetail.FirstSaveTime,
                LastSaveTime = now,
                FirstSaveLogonName = request.ResponseDetail.FirstSaveLogonName,
                IsDraftMode = request.IsDraftMode,
                IsLocked = request.IsLocked,
                RequiredFieldsList = request.ResponseDetail.RequiredFieldsList,
                HiddenFieldsList = request.ResponseDetail.HiddenFieldsList,
                HighlightedFieldsList = request.ResponseDetail.HighlightedFieldsList,
                DisabledFieldsList = request.ResponseDetail.DisabledFieldsList,
                ResponseQA = request.ResponseDetail.FlattenedResponseQA()
            };

            bool isSuccessful = false;
            // TODO: This must await to prevent the caller from running!!!!!!
            var result = _surveyResponseCRUD.SaveFormResponsePropertiesAsync(responseContext, formResponseProperties); // .ConfigureAwait(false).GetAwaiter().GetResult();
            return isSuccessful;
        }

		#endregion


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


		#region DeleteSurveyByResponseId
		public SurveyAnswerResponse DeleteResponse(IResponseContext responseContext)
		{
			bool deleteStatus = UpdateResponseStatus(responseContext, RecordStatus.Deleted, RecordStatusChangeReason.DeleteResponse);
			SurveyAnswerResponse surveyAnsResponse = new SurveyAnswerResponse();
			return surveyAnsResponse;
		}

		#endregion


		#region Read All Records By SurveyID
		//public IEnumerable<SurveyResponse> GetAllResponsesWithCriteria(IResponseContext responseContext, IDictionary<int, FieldDigest> gridFields, IDictionary<int, KeyValuePair<FieldDigest, string>> searchFields, int pageSize = 0, int pageNumber = 0)
		//{
		//	return _surveyResponseCRUD.GetAllResponsesWithCriteria(responseContext, gridFields, searchFields, null, pageSize, pageNumber);
		//}

        public IEnumerable<FormResponseDetail> GetAllResponsesWithCriteria(IResponseContext responseContext, IDictionary<int, FieldDigest> gridFields, IDictionary<int, KeyValuePair<FieldDigest, string>> searchFields, int pageSize = 0, int pageNumber = 0)
        {
            return _surveyResponseCRUD.GetAllResponsesWithCriteria(responseContext, gridFields, searchFields, null, pageSize, pageNumber);
        }

        public FormResponseDetail GetFormResponseByResponseId(IResponseContext responseContext)
		{
			var response = _surveyResponseCRUD.GetHierarchialResponseListByResponseId(responseContext);
			var formResponseDetail = response[0].ToFormResponseDetail();
			return formResponseDetail;
		}
		#endregion

		#region Get Hierarchial Responses for DataConsisitencyServiceAPI
		public FormResponseDetail GetHierarchialResponsesByResponseId(IResponseContext responseContext, bool includeDeletedRecords = false)
		{
			var hierarchicalDocumentResponseProperties = _surveyResponseCRUD.GetHierarchialResponseListByResponseId(responseContext, includeDeletedRecords);
			var hierarchialFormResponseDetail = hierarchicalDocumentResponseProperties.ToHierarchialFormResponseDetail();
			return hierarchialFormResponseDetail;
		}
		#endregion


		#region Create Resonse Document Info
		private DocumentResponseProperties CreateResponseDocumentInfo(string formId, int pageId)
		{
			DocumentResponseProperties documentResponseProperties = new DocumentResponseProperties();
			return documentResponseProperties;
		}
		#endregion

		#region Notify Consistency Service
		public void NotifyConsistencyService(IResponseContext responseContext, int responseStatus, RecordStatusChangeReason reasonForStatusChange)
		{
			if (responseStatus == RecordStatus.Deleted || responseStatus == RecordStatus.Saved)
			{
				try
				{
					var serviceBusCRUD = new ServiceBusCRUD();
					var hierarchialResponse = GetHierarchialResponsesByResponseId(responseContext, true);
                    var messageHeader = string.Format("{0},{1},{2}", responseContext.RootFormName, responseContext.RootFormId, responseContext.RootResponseId ?? responseContext.ResponseId);
					switch (reasonForStatusChange)
					{
						case RecordStatusChangeReason.SubmitOrClose:

                            //send notification to ServiceBus
                            var messageHeaderJson = JsonConvert.SerializeObject(messageHeader);
                            var hierarchialResponseJson = JsonConvert.SerializeObject(hierarchialResponse);
							serviceBusCRUD.SendMessagesToTopic(messageHeaderJson, hierarchialResponseJson);
							//ConsistencyHack(hierarchialResponse);
							break;
						case RecordStatusChangeReason.DeleteResponse:
							//Update status and send notification to ServiceBus 
							hierarchialResponse.RecStatus = RecordStatus.Deleted;
                            messageHeaderJson = JsonConvert.SerializeObject(messageHeader);
                            hierarchialResponseJson = JsonConvert.SerializeObject(hierarchialResponse);
							serviceBusCRUD.SendMessagesToTopic(messageHeaderJson, hierarchialResponseJson);
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
    }
}