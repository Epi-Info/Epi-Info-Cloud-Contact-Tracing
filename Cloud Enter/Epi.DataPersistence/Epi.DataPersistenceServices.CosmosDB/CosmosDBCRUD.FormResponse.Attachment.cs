using System;
using Epi.Cloud.Common;
using Epi.Common.Core.Interfaces;
using Epi.PersistenceServices.CosmosDB;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Epi.DataPersistenceServices.CosmosDB
{
    public partial class CosmosDBCRUD
    {
        private const int HResult_AttachmentAlreadyExists = -2146233088;

        public Attachment CreateResponseAttachment(string documentSelfLink, IResponseContext responseContext, string attachmentId, string surveyData)
        {
            int maxRetries = 2;
            TimeSpan interval = TimeSpan.FromMilliseconds(100);
            RetryStrategies retryStrategy = new RetryStrategies(maxRetries, interval);

            var attachment = retryStrategy.ExecuteWithRetry<Attachment>(() => 

                Client.CreateAttachmentAsync(documentSelfLink, new { id = attachmentId, contentType = "text/plain", media = "link to your media", RootResponseId = responseContext.RootResponseId, SurveyDocument = surveyData }).Result,
                       
                (ex, consumedRetries, remainingRetries) => RetryHandlerForCreateResponseAttachment(ex, consumedRetries, remainingRetries, responseContext, attachmentId)
            );
            return attachment;
        }

        private RetryResponse<Attachment> RetryHandlerForCreateResponseAttachment(Exception ex, int consumedRetries, int remainingRetries, IResponseContext responseContext, string attachmentId)
        {
            var baseException = ex.GetBaseException();
            if (baseException as DocumentClientException != null && baseException.HResult == HResult_AttachmentAlreadyExists)
            {
                var existingAttachment = ReadResponseAttachment(responseContext, attachmentId);
                DeleteAttachment(existingAttachment);
                return new RetryResponse<Attachment> { Action = RetryAction.ContinueRetrying };
            }
            else
            {
                return new RetryResponse<Attachment> { Action = RetryAction.ThrowException };
            }
        }

        public Attachment ReadResponseAttachment(IResponseContext responseContext, string attachmentId)
        {
            Attachment attachment = null;
            try
            {
                var rootFormName = responseContext.RootFormName;
                var rootResponseId = responseContext.RootResponseId;
                var attachmentUri = UriFactory.CreateAttachmentUri(DatabaseName, rootFormName, rootResponseId, attachmentId);
                attachment = Client.ReadAttachmentAsync(attachmentUri).Result;
                return attachment;
            }
            catch (Exception ex)
            {
                attachment = null;
                return attachment;
            }
        }
   
        public FormResponseResource RetrieveResponseAttachment(Attachment attachmentInfo)
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
