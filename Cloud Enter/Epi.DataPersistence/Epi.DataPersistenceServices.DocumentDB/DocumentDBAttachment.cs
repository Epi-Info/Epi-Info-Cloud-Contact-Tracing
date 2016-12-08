using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Epi.PersistenceServices.DocumentDB.DataStructures;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class SurveyResponseCRUD
    {
        public Attachment CreateAttachment(string documentSelfLink, string attachmentId, string globalRecordId, string surveyData)
        {

            Attachment attachment = null;
            try
            {
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    if (attachment == null)
                    {
                        attachment = client.CreateAttachmentAsync(documentSelfLink, new { id = attachmentId, contentType = "text/plain", media = "link to your media", GlobalRecordID = globalRecordId, SurveyDocumnet = surveyData }).Result;
                        return attachment;
                    }
                    else
                    {
                        // return attachment.GetPropertyValue<string>(attachmentId);
                        return attachment;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public Attachment ReadAttachment(DocumentClient client, string globalRecordId, string attachmentId)
        {

            Attachment attachment = null;
            try
            {
                var attachLink = UriFactory.CreateAttachmentUri(DatabaseName, FormInfoCollectionName, globalRecordId, attachmentId);
                attachment = client.ReadAttachmentAsync(attachLink).Result;
                return attachment;
            }
            catch (Exception ex)
            {
                attachment = null;
                return attachment;
            }
        }

       
        public HierarchicalDocumentResponseProperties ConvertAttachmentToHierarchical(DocumentClient client, Attachment attachmentInfo)
        {
            var attachmentResponse = attachmentInfo.GetPropertyValue<string>("SurveyDocumnet");
            HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = JsonConvert.DeserializeObject<HierarchicalDocumentResponseProperties>(attachmentResponse);
            if (hierarchicalDocumentResponseProperties != null)
            {
                if (hierarchicalDocumentResponseProperties.FormResponseProperties != null && hierarchicalDocumentResponseProperties.PageResponsePropertiesList!=null)
                {
                    Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                    //var newformResponseProperties = ReadFormInfoByResponseId(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, client, formInfoCollectionUri);
                    return hierarchicalDocumentResponseProperties;
                }
                
            }
            return null;
        }
        public bool DeleteSurveyDataInDocumentDB(DocumentClient client, string globalId, string collectionName, List<int> pageIds)
        {
            bool deletestatus = false;
            try
            {
                foreach (var pageId in pageIds)
                {
                    //Create Survey Properties 
                    Uri pageCollectionUri = GetCollectionUri(client, collectionName + pageId);
                    var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, collectionName + pageId, globalId);
                    var response = client.DeleteDocumentAsync(docLink).Result;
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
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    var test = client.DeleteAttachmentAsync(attachment.AltLink, null).Result;
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }

        

    }

}
