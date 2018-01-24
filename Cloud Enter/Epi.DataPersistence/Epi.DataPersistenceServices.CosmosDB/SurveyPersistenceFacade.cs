using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Core;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.ServiceBus;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Common;
using Epi.DataPersistence.Common.Interfaces;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistenceServices.CosmosDB;
using Microsoft.Azure.Documents;

namespace Epi.PersistenceServices.CosmosDB
{
    public partial class CosmosDB_SurveyPersistenceFacade : MetadataAccessor, ISurveyPersistenceFacade
    {
        private string AttachmentId = AppSettings.GetStringValue(AppSettings.Key.AttachmentId);

        public CosmosDB_SurveyPersistenceFacade()
        {
        }

        CosmosDBCRUD _formResponseCRUD = new CosmosDBCRUD();

        public FormResponseDetail GetFormResponseState(IResponseContext responseContext)
        {
            var formResponseProperties = _formResponseCRUD.GetFormResponseState(responseContext);
            return formResponseProperties != null ? formResponseProperties.ToFormResponseDetail() : null;
        }

        public bool DoChildResponsesExist(IResponseContext responseContext, bool includeDeletedRecords = false) // string childFormId, string parentResponseId)
        {
            return _formResponseCRUD.DoChildResponsesExist(responseContext, includeDeletedRecords);
        }

        public int GetFormResponseCount(string formId, bool includeDeletedRecords = false)
        {
            return _formResponseCRUD.GetFormResponseCount(formId, includeDeletedRecords);
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
                        var UpdateAttachmentresult = _formResponseCRUD.UpdateAttachment(responseContext, responseStatus);
                        break;
                    case RecordStatus.Deleted:
                        var UpdateSurveyResponseStatusToDeleteResult = _formResponseCRUD.DeleteResponse(responseContext, RecordStatus.Deleted);
                        //NotifyConsistencyService(responseId, RecordStatus.Deleted, RecordStatusChangeReason.DeleteResponse);
                        break;
                    case RecordStatus.RecoverLastRecordVersion:
                        attachment = _formResponseCRUD.ReadResponseAttachment(responseContext, AttachmentId);
                        if (attachment == null)
                        {
                            //Add new record Don't Save
                            var NewRecordDontSaveResult = _formResponseCRUD.DeleteResponse(responseContext, RecordStatus.PhysicalDelete);
                        }
                        else
                        {
                            //Edit Record Don't Save
                            FormResponseResource formResonseResource = _formResponseCRUD.RetrieveResponseAttachment(attachment);
                            var editRecordDontSaveResult = _formResponseCRUD.RestoreLastResponseSnapshot(formResonseResource);

                            //Delete Attachment                         
                            var deleteResponse = _formResponseCRUD.DeleteAttachment(attachment);
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
            var isSuccessful = _formResponseCRUD.ExecuteWithFollowOnAction(
                () => SaveFormResponseProperties(surveyResponseBO),
(Action)(() =>
{
    if (surveyResponseBO.RecStatus == RecordStatus.Saved)
    {
        NotifyConsistencyService(surveyResponseBO, (int)surveyResponseBO.RecStatus, RecordStatusChangeReason.SubmitOrClose);
    }
}));
            return isSuccessful;
        }

        //public bool SaveResponsexxx(SurveyResponseBO surveyResponseBO)
        //{
        //    using (ManualResetEvent completionEvent = new ManualResetEvent(false))
        //    {
        //        bool isSuccessful = false;
        //        Task<bool> isSuccessfulTask = null;

        //        var backgroundTask = Task.Run(() =>
        //        {
        //            isSuccessfulTask = SaveFormResponseProperties(surveyResponseBO);
        //        });

        //        var millisecondsToSleep = 100;
        //        var retries = (Int32)TimeSpan.FromSeconds(5).TotalMilliseconds / millisecondsToSleep;
        //        bool isCompleted = false;
        //        while (retries > 0)
        //        {
        //            if (isSuccessfulTask == null) { Thread.Sleep(10); continue; }
        //            isCompleted = isSuccessfulTask.IsCompleted;
        //            if (isCompleted) break;
        //            Thread.Sleep(millisecondsToSleep);
        //            retries -= 1;
        //        }
        //        isSuccessful = isCompleted;
        //        var awaiter = isSuccessfulTask.ContinueWith(t =>
        //        {
        //            if (surveyResponseBO.Status == RecordStatus.Saved)
        //            {
        //                NotifyConsistencyService(surveyResponseBO, surveyResponseBO.Status, RecordStatusChangeReason.SubmitOrClose);
        //            }
        //            completionEvent.Set();
        //        }, TaskContinuationOptions.AttachedToParent).ConfigureAwait(false);

        //        isSuccessful &= completionEvent.WaitOne(TimeSpan.FromSeconds(5));
        //        awaiter.GetAwaiter().GetResult();
        //        isSuccessful &= isSuccessfulTask.Result;
        //        isSuccessful &= backgroundTask.Wait(TimeSpan.FromSeconds(5));
        //        return isSuccessful;
        //    }
        //}

        #region Save Form Response Properties
        /// <summary>
        /// First time store ResponseId, RecStatus, and SurveyId in CosmosDB
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<bool> SaveFormResponseProperties(SurveyResponseBO response)
        {
            var now = DateTime.UtcNow;
            List<FormResponseProperties> formResponsePropertiesList = response.ResponseDetail.ToFormResponsePropertiesFlattenedList();

            bool isSuccessful = false;
            var result = await _formResponseCRUD.SaveFormResponsePropertiesAsync(formResponsePropertiesList).ConfigureAwait(false);
            isSuccessful = result.Resource != null;
            return isSuccessful;
        }

        #endregion

        #region DeleteSurveyByResponseId
        public SurveyAnswerResponse DeleteResponse(IResponseContext responseContext)
        {
            bool deleteStatus = UpdateResponseStatus(responseContext, RecordStatus.Deleted, RecordStatusChangeReason.DeleteResponse);
            SurveyAnswerResponse surveyAnsResponse = new SurveyAnswerResponse();
            return surveyAnsResponse;
        }

        #endregion

        #region Get All Responses With Criteria
        public ResponseGridQueryResult GetAllResponsesWithCriteria(ResponseGridQueryCriteria responseGridQueryCriteria)
        {
            ResponseGridQueryPropertiesResult responseGridQueryPropertiesResult = _formResponseCRUD.GetAllResponsesWithCriteria(responseGridQueryCriteria);
            var formResponseDetailList = responseGridQueryPropertiesResult.ResponsePropertiesList.ToFormResponseDetailList();
            var result = new ResponseGridQueryResult
            {
                FormResponseDetailList = responseGridQueryPropertiesResult.ResponsePropertiesList.ToFormResponseDetailList(),
                QuerySetToken = responseGridQueryPropertiesResult.QuerySetToken,
                NumberOfResponsesReturnedByQuery = responseGridQueryPropertiesResult.NumberOfResponsesReturnedByQuery,
                NumberOfResponsesPerPage = responseGridQueryPropertiesResult.NumberOfResponsesPerPage,
                NumberOfResponsesOnSelectedPage = responseGridQueryPropertiesResult.NumberOfResponsesOnSelectedPage,
                PageNumber = responseGridQueryPropertiesResult.PageNumber,
                NumberOfPages = responseGridQueryPropertiesResult.NumberOfPages,
                PostProcessingWasRequired = responseGridQueryPropertiesResult.IsPostProcessingRequired
            };
            return result;
        }
        #endregion

        public FormResponseDetail GetFormResponseByResponseId(IResponseContext responseContext)
        {
            var response = _formResponseCRUD.GetHierarchicalResponseListByResponseId(responseContext);
            var formResponseDetail = response.ToHierarchicalFormResponseDetail();
            //var formResponseDetail = response[0].ToFormResponseDetail();
            return formResponseDetail;
        }

        #region Get Hierarchical Responses for DataConsisitencyServiceAPI
        public FormResponseDetail GetHierarchicalResponsesByResponseId(IResponseContext responseContext, bool includeDeletedRecords = false)
        {
            var hierarchicalDocumentResponseProperties = _formResponseCRUD.GetHierarchicalResponseListByResponseId(responseContext, includeDeletedRecords);
            var hierarchicalFormResponseDetail = hierarchicalDocumentResponseProperties.ToHierarchicalFormResponseDetail();
            return hierarchicalFormResponseDetail;
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
                    var hierarchicalResponse = GetHierarchicalResponsesByResponseId(responseContext, includeDeletedRecords: true);
                    var messageHeader = string.Format("{0},{1},{2}", responseContext.RootFormName, responseContext.RootFormId, responseContext.RootResponseId);

                    var shouldNotify = ConsistencyServiceAttributeHelper.ShouldNotifyConsistencyService(reasonForStatusChange);
                    if (shouldNotify)
                    {
                        //send notification to ServiceBus
                        NotifyConsistencyService(hierarchicalResponse);
                        //ConsistencyHack(hierarchicalResponse);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public void NotifyConsistencyService(FormResponseDetail hierarchicalFormResponseDetail)
        {
            var serviceBusCRUD = new ServiceBusCRUD();
            //send notification to ServiceBus
            serviceBusCRUD.SendMessagesToTopic(hierarchicalFormResponseDetail);
            //ConsistencyHack(hierarchicalResponse);
        }

        #endregion NotifyConsistencyService  
    }
}