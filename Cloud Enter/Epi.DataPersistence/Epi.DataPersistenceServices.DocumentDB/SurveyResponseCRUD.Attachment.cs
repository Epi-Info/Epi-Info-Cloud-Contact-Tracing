using System;
using System.Collections.Generic;
using Epi.Cloud.Common;
using Epi.PersistenceServices.DocumentDB;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class SurveyResponseCRUD
    {
        private const int HResult_AttachmentAlreadyExists = -2146233088;

        public Attachment CreateAttachment(string documentSelfLink, string attachmentId, string formName, string responseId, string surveyData)
        {
            int maxRetries = 2;
            TimeSpan interval = TimeSpan.FromMilliseconds(100);
            RetryStrategies retryStrategy = new RetryStrategies(maxRetries, interval);

            var attachment = retryStrategy.ExecuteWithRetry<Attachment>(() => 

                Client.CreateAttachmentAsync(documentSelfLink, new { id = attachmentId, contentType = "text/plain", media = "link to your media", GlobalRecordID = responseId, SurveyDocument = surveyData }).Result,
                       
                (ex, consumedRetries, remainingRetries) => RetryHandlerForCreateAttachment(ex, consumedRetries, remainingRetries, attachmentId, formName, responseId)
            );
            return attachment;
        }

        private RetryResponse<Attachment> RetryHandlerForCreateAttachment(Exception ex, int consumedRetries, int remainingRetries, string attachmentId, string formName, string responseId)
        {
            var baseException = ex.GetBaseException();
            if (baseException as DocumentClientException != null && baseException.HResult == HResult_AttachmentAlreadyExists)
            {
                var existingAttachment = ReadAttachment(formName, responseId, attachmentId);
                DeleteAttachment(existingAttachment);
                return new RetryResponse<Attachment> { Action = RetryAction.ContinueRetrying };
            }
            else
            {
                return new RetryResponse<Attachment> { Action = RetryAction.ThrowException };
            }
        }


        public Attachment ReadAttachment(string formName, string responseId, string attachmentId)
        {

            Attachment attachment = null;
            try
            {
                var attachmentUri = UriFactory.CreateAttachmentUri(DatabaseName, formName, responseId, attachmentId);
                attachment = Client.ReadAttachmentAsync(attachmentUri).Result;
                return attachment;
            }
            catch (Exception ex)
            {
                attachment = null;
                return attachment;
            }
        }
   
        public FormResponseResource RetrieveAttachment(Attachment attachmentInfo)
        {
            try
            {
                var attachmentResponse = attachmentInfo.GetPropertyValue<string>("SurveyDocument") ?? attachmentInfo.GetPropertyValue<string>("SurveyDocumnet");

                FormResponseResource formResponseResource = attachmentResponse != null
                    ? JsonConvert.DeserializeObject<FormResponseResource>(attachmentResponse)
                    : null;
                return formResponseResource;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool DeleteSurveyDataInDocumentDB(string globalId, string collectionName, List<int> pageIds)
        {
            try
            {
                foreach (var pageId in pageIds)
                {
                    //Create Survey Properties 
                    Uri pageCollectionUri = GetCollectionUri(collectionName + pageId);
                    var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, collectionName + pageId, globalId);
                    var response = Client.DeleteDocumentAsync(docLink).Result;
                }
                return true;
            }
            catch (Exception ex)
            {

            }
            return false;

        }

        public bool DeleteAttachment(Attachment attachment)
        {
            try
            {
              var DeleteResponse = Client.DeleteAttachmentAsync(attachment.AltLink, null).Result;
            }
            catch (Exception ex)
            {

            }

            return true;
        }
    }
}
