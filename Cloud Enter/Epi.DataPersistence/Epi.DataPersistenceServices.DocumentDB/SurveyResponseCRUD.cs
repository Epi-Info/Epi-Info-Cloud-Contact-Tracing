#define ConfigureIndexing

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.Common.Configuration;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using static Epi.PersistenceServices.DocumentDB.DataStructures;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class SurveyResponseCRUD
    {
        public string serviceEndpoint;
        public string authKey;
        public string DatabaseName = ConfigurationManager.AppSettings["DatabaseName"];
        public const string FormInfoCollectionName = "FormInfo";
        public string attachmentId = ConfigurationManager.AppSettings["AttachmentId"];
        Microsoft.Azure.Documents.Database _database;
        Dictionary<string, DocumentCollection> _documentCollections = new Dictionary<string, DocumentCollection>();

        public SurveyResponseCRUD()
        {
            ParseConnectionString();

            //Getting reference to Database 
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                _database = GetOrCreateDatabase(client, DatabaseName);
            }
        }

        private void ParseConnectionString()
        {
            var connectionStringName = ConfigurationHelper.GetEnvironmentResourceKey("CollectedDataConnectionString");
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            var parts = connectionString.Split(',');
            serviceEndpoint = parts[0].Trim();
            for (var i = 1; i < parts.Length; ++i)
            {
                var nvp = parts[i];
                var eqSignIndex = nvp.IndexOf('=');
                var name = nvp.Substring(0, eqSignIndex).Trim().ToLowerInvariant();
                var value = nvp.Substring(eqSignIndex + 1).Trim();
                switch (name)
                {
                    case "authkey":
                        authKey = value;
                        break;
                    case "dbname":
                        DatabaseName = value;
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(serviceEndpoint) || string.IsNullOrWhiteSpace(authKey))
            {
                throw new ConfigurationException("SurveyResponse ConnectionString is invalid. Service Endpoint and AuthKey must be specified.");
            }
        }

        #region GetOrCreateDabase
        /// <summary>
        ///If DB is not avaliable in Document Db create DB
        /// </summary>
        private Microsoft.Azure.Documents.Database GetOrCreateDatabase(DocumentClient client, string databaseName)
        {
            if (_database == null)
            {
                _database = client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
                if (_database == null)
                {
                    client.CreateDatabaseAsync(new Microsoft.Azure.Documents.Database { Id = databaseName }, null)
                           .ContinueWith(t => _database = t.Result); ;
                }
            }
            return _database;
        }

        #endregion



        #region GetOrCreateCollection 
        /// <summary>
        /// Get or Create Collection in Document DB
        /// </summary>
        private DocumentCollection GetOrCreateCollection(DocumentClient client, string databaseLink, string collectionId)
        {
            DocumentCollection documentCollection;
            if (!_documentCollections.TryGetValue(collectionId, out documentCollection))
            {
                documentCollection = client.CreateDocumentCollectionQuery(databaseLink).Where(c => c.Id == collectionId).AsEnumerable().FirstOrDefault();
                if (documentCollection == null)
                {
#if ConfigureIndexing
                    DocumentCollection collectionSpec = ConfigureIndexing(collectionId);
#else
                                  var collectionSpec = new DocumentCollection
                                  {
                                         Id = collectionId,
                                  };
#endif //ConfigureIndexing
                    documentCollection = client.CreateDocumentCollectionAsync(databaseLink, collectionSpec).Result;
                    //              client.CreateDocumentCollectionAsync(databaseLink, collectionSpec)
                    //.ContinueWith(t =>
                    //{
                    //     documentCollection = t.Result;
                    //     _documentCollections[collectionId] = documentCollection;
                    //});
                }
            }
            return documentCollection;
        }
        #endregion

        private DocumentCollection GetCollectionReference(DocumentClient client, string collectionId)
        {
            //Get a reference to the DocumentDB Database 
            var database = GetOrCreateDatabase(client, DatabaseName);

            //Get a reference to the DocumentDB Collection
            var collection = GetOrCreateCollection(client, database.SelfLink, collectionId);
            return collection;
        }

        private Uri GetCollectionUri(DocumentClient client, string collectionId)
        {
            GetCollectionReference(client, collectionId);
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionId);
            return collectionUri;
        }

        /// <summary>
        /// UpdateResponseStatus
        /// </summary>
        /// <param name="responseId"></param>
        /// <param name="newResponseStatus"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> UpdateResponseStatus(string responseId, int responseStatus, int userId = 0, int newResponseStatus = 0)
        {
            newResponseStatus = responseStatus;

            Attachment attachment = null;
            bool isSuccessful = false;
            bool deleteResponse = false;
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                try
                {
                    Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                    var formResponseProperties = ReadFormInfoByResponseId(responseId, client, formInfoCollectionUri);
                    //formResponseProperties.RecStatus= responseStatus;
                    //var formResponsePropertieks = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);



                    if (formResponseProperties != null)
                    {
                        //Is status is Saved and check if attachment is existed or not.If attachment is null and delete attachment
                        if (newResponseStatus == RecordStatus.Saved)
                        {
                            attachment = ReadAttachment(client, responseId, attachmentId);
                            // formResponseProperties.RecStatus = responseStatus;
                            if (attachment != null)
                            {
                                deleteResponse = DeleteAttachment(attachment);
                            }
                        }

                        if (newResponseStatus != formResponseProperties.RecStatus)
                        {
                            switch (newResponseStatus)
                            {
                                case RecordStatus.Saved:
                                    formResponseProperties.RecStatus = RecordStatus.Saved;
                                    var formResponseSave = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
                                    break;
                                case RecordStatus.Deleted:
                                    var existFormResponseProperties = ReadFormInfoByResponseId(responseId, client, formInfoCollectionUri);

                                    //Restore Attachment to FormInfo
                                    if (existFormResponseProperties != null)
                                    {
                                        if (existFormResponseProperties.GlobalRecordID == formResponseProperties.GlobalRecordID)
                                        {
                                            existFormResponseProperties.RecStatus = 0;
                                            var formResponse = await client.UpsertDocumentAsync(formInfoCollectionUri, existFormResponseProperties);
                                        }
                                    }
                                    break;
                                case RecordStatus.DontSave:
                                    attachment = ReadAttachment(client, responseId, attachmentId);
                                    if (attachment == null)
                                    {
                                        //formResponseProperties.RecStatus = RecordStatus.Deleted;
                                        //var formResponse = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
                                    }
                                    else
                                    {
                                        HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = ConvertAttachmentToHierarchical(client, attachment);
                                        var existingFormResponseProperties = ReadFormInfoByResponseId(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, client, formInfoCollectionUri);

                                        //Restore Attachment to FormInfo
                                        if (hierarchicalDocumentResponseProperties.FormResponseProperties != null)
                                        {
                                            if (existingFormResponseProperties.GlobalRecordID == formResponseProperties.GlobalRecordID)
                                            {
                                                formResponseProperties.RecStatus = 2;
                                                var formResponse = await client.UpsertDocumentAsync(formInfoCollectionUri, hierarchicalDocumentResponseProperties.FormResponseProperties);
                                            }
                                        }
                                        //Restore Attachment to Parent
                                        if (hierarchicalDocumentResponseProperties.PageResponsePropertiesList != null)
                                        {
                                            foreach (var pageAttachmentResponseProperties in hierarchicalDocumentResponseProperties.PageResponsePropertiesList)
                                            {
                                                var collectionName = hierarchicalDocumentResponseProperties.FormResponseProperties.FormName + pageAttachmentResponseProperties.PageId;
                                                Uri pageCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                                                try
                                                {
                                                    var ParentResponse = await client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties);
                                                }
                                                catch (Exception ex)
                                                {

                                                }
                                            }
                                        }
                                        //Restore Attachment to Child
                                        if (hierarchicalDocumentResponseProperties.ChildResponseList != null && hierarchicalDocumentResponseProperties.ChildResponseList.Count > 0)
                                        {
                                            foreach (var pageAttachmentResponseProperties in hierarchicalDocumentResponseProperties.ChildResponseList)
                                            {
                                                var collectionName = hierarchicalDocumentResponseProperties.FormResponseProperties.FormName;
                                                Uri pageCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                                                var forminfoResponse = await client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties);
                                            }
                                        }
                                        //Delete new survey data in Document DB
                                        if (hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds != null && hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds.Count > 0)
                                        {
                                            var RemovePages = existingFormResponseProperties.PageIds.Except(hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds).ToList();
                                            if (RemovePages.Count > 0)
                                            {
                                                deleteResponse = DeleteSurveyDataInDocumentDB(client, hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, hierarchicalDocumentResponseProperties.FormResponseProperties.FormName, RemovePages);
                                            }
                                        }

                                        //Delete Attachment
                                        Uri collectionUri = UriFactory.CreateAttachmentUri(DatabaseName, FormInfoCollectionName, hierarchicalDocumentResponseProperties.FormResponseProperties.Id, attachmentId);
                                        deleteResponse = DeleteAttachment(attachment);
                                    }

                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return isSuccessful;
        }
        #region Attachment
        public Attachment GetAttachmentInfo(DocumentClient client, string globalRecordID, string attachmentId, string documentSelfLink, string hierarchialResponseJson)
        {

            Attachment attachment = null;
            //Check Attachment is exist or not 
            attachment = ReadAttachment(client, globalRecordID, attachmentId);
            if (attachment == null && hierarchialResponseJson != null)
            {

                attachment = CreateAttachment(documentSelfLink, attachmentId, globalRecordID, hierarchialResponseJson);
            }
            return attachment;
        }
        #endregion
        public async Task<bool> RestoreSurveyDataFromAttachment(Attachment attachmentInfo, string attachmentId, FormResponseProperties existingFormResponseProperties)
        {

            Attachment attachment = null;
            bool tasksRanToCompletion = false;
            bool deleteResponse = false;
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                bool isSuccessful = false;
                try
                {
                    var formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                    var formInfoResponse = await client.UpsertDocumentAsync(formInfoCollectionUri, existingFormResponseProperties);
                    HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = ConvertAttachmentToHierarchical(client, attachmentInfo);
                    //Restore Attachment to FormInfo
                    if (hierarchicalDocumentResponseProperties.FormResponseProperties != null)
                    {
                        var existingFormRespodgnseProperties = ReadFormInfoByResponseId(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, client, formInfoCollectionUri);

                    }
                    //Restore Attachment to Parent
                    if (hierarchicalDocumentResponseProperties.PageResponsePropertiesList != null)
                    {
                        foreach (var pageAttachmentResponseProperties in hierarchicalDocumentResponseProperties.PageResponsePropertiesList)
                        {
                            var collectionName = hierarchicalDocumentResponseProperties.FormResponseProperties.FormName + pageAttachmentResponseProperties.PageId;
                            Uri pageCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                            try
                            {
                                var ParentResponse = client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties).Result;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    //Restore Attachment to Child
                    if (hierarchicalDocumentResponseProperties.ChildResponseList != null)
                    {
                        foreach (var pageAttachmentResponseProperties in hierarchicalDocumentResponseProperties.ChildResponseList)
                        {
                            var collectionName = hierarchicalDocumentResponseProperties.FormResponseProperties.FormName;
                            Uri pageCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                            client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties);
                        }
                    }
                    //Delete new survey data
                    if (hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds != null)
                    {
                        var newFormResponseProperties = ReadFormInfoByResponseId(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, client, formInfoCollectionUri);
                        hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds = hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds.Except(newFormResponseProperties.PageIds).ToList();
                        deleteResponse = DeleteSurveyDataInDocumentDB(client, hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, hierarchicalDocumentResponseProperties.FormResponseProperties.FormName, hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds);
                    }
                    if (deleteResponse)
                    {
                        Uri collectionUri = UriFactory.CreateAttachmentUri(DatabaseName, FormInfoCollectionName, hierarchicalDocumentResponseProperties.FormResponseProperties.Id, attachmentId);
                        deleteResponse = DeleteAttachment(attachment);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return tasksRanToCompletion;
        }

        #region InsertToSurveyToDocumentDB
        /// <summary>
        /// Created instance of DocumentClient and Getting reference to database and Document collections
        /// </summary>
        ///
        public async Task<bool> InsertResponseAsync(DocumentResponseProperties documentResponseProperties)
        {
            bool tasksRanToCompletion = false;
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);

                var formResponseProperties = ReadFormInfoByResponseId(documentResponseProperties.GlobalRecordID, client, formInfoCollectionUri);

                foreach (var newPageResponseProperties in documentResponseProperties.PageResponsePropertiesList)
                {
                    var pageId = newPageResponseProperties.PageId;
                    Uri pageCollectionUri = GetCollectionUri(client, newPageResponseProperties.ToColectionName(documentResponseProperties.FormResponseProperties.FormName));
                    var pageResponse = await client.UpsertDocumentAsync(pageCollectionUri, newPageResponseProperties);

                    if (!formResponseProperties.PageIds.Contains(pageId))
                    {
                        formResponseProperties.PageIds.Add(pageId);
                    }
                }

                formResponseProperties.PageIds.Sort();

                var formResponse = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties); //UpsertDocumentAsync(client, formInfoCollectionUri, formResponseProperties);
                tasksRanToCompletion = true;
            }
            return tasksRanToCompletion;
        }

        private async Task<ResourceResponse<Document>> UpsertDocumentAsync(DocumentClient client, Uri formInfoCollectionUri, FormResponseProperties formResponseProperties)
        {
            formResponseProperties = new FormResponseProperties();
            formResponseProperties.Id = "ATL10";
            try
            {
                var result = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private Task<ResourceResponse<Document>> UpsertDocumentAsync(DocumentClient client, PageResponseProperties newPageResponseProperties, Uri pageCollectionUri)
        {
            return client.UpsertDocumentAsync(pageCollectionUri, newPageResponseProperties);
        }

        #endregion

        #region SaveQuestionInDocumentDB

        /// <summary>
        /// This method help to save form properties 
        /// and also used for delete operation.Ex:RecStatus=0
        /// </summary>
        /// <param name="formResponseProperties"></param>
        /// <returns></returns>
        public async Task<bool> UpsertFormResponseProperties(FormResponseProperties formResponseProperties)
        {
            bool isSuccessful = false;
            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    var formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);

                    //Verify Response Id is exist or Not
                    var existingFormResponseProperties = ReadFormInfoByResponseId(formResponseProperties.GlobalRecordID, client, formInfoCollectionUri);
                    if (existingFormResponseProperties == null)
                    {
                        formResponseProperties.Id = formResponseProperties.GlobalRecordID;
                        formResponseProperties.PageIds = formResponseProperties.PageIds ?? new List<int>();
                        var result = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
                    }
                    else
                    {
                        //Create attachment
                        if (existingFormResponseProperties.RecStatus == RecordStatus.Saved || true)
                        {
                            Attachment attachment = null;
                            attachment = GetAttachmentInfo(client, existingFormResponseProperties.GlobalRecordID, attachmentId, existingFormResponseProperties.SelfLink, null);
                            if (attachment == null)
                            {
                                var hierarchialResponse = GetHierarchialResponsesByResponseId(existingFormResponseProperties.GlobalRecordID, true);
                                var hierarchialResponseJson = JsonConvert.SerializeObject(hierarchialResponse);
                                //Check Attachment is exist or not ,If not create attachment                             
                                attachment = GetAttachmentInfo(client, existingFormResponseProperties.GlobalRecordID, attachmentId, existingFormResponseProperties.SelfLink, hierarchialResponseJson);
                            }
                        }

                        var pageIdsUpdated = false;
                        formResponseProperties.RelateParentId = existingFormResponseProperties.RelateParentId;
                        if (formResponseProperties.PageIds != null && formResponseProperties.PageIds.Count > 0)
                        {
                            var newPageIds = formResponseProperties.PageIds.ToList();
                            formResponseProperties.PageIds = existingFormResponseProperties.PageIds;
                            foreach (var pageId in newPageIds)
                            {
                                if (!existingFormResponseProperties.PageIds.Contains(pageId))
                                {
                                    formResponseProperties.PageIds.Add(pageId);
                                    pageIdsUpdated = true;
                                }
                            }
                            formResponseProperties.PageIds.Sort();
                        }

                        if (pageIdsUpdated
                               || existingFormResponseProperties.RecStatus != formResponseProperties.RecStatus
                               || existingFormResponseProperties.HiddenFieldsList != formResponseProperties.HiddenFieldsList
                               || existingFormResponseProperties.DisabledFieldsList != formResponseProperties.DisabledFieldsList
                               || existingFormResponseProperties.HighlightedFieldsList != formResponseProperties.HighlightedFieldsList
                               || existingFormResponseProperties.RequiredFieldsList != formResponseProperties.RequiredFieldsList)
                        {
                            var result = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return isSuccessful;
        }
        #endregion

        #region Get PageResponseProperties By ResponseId, FormId, and PageId
        public PageResponseProperties GetPageResponsePropertiesByResponseId(string responseId, FormDigest formDigest, int pageId)
        {
            PageResponseProperties pageResponseProperties = null;
            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    var collectionName = formDigest.FormName + pageId;
                    Uri pageCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);

                    //Read collection and store data  
                    pageResponseProperties = ReadPageResponsePropertiesByResponseId(responseId, client, pageCollectionUri);
                    return pageResponseProperties;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        #endregion Get PageResponseProperties By ResponseId, FormId, and PageId


        #region Get All Responses With FieldNames 
        public List<SurveyResponse> GetAllResponsesWithFieldNames(IDictionary<int, FieldDigest> fields, string relateParentId = null)
        {

            List<SurveyResponse> surveyResponse = null;

            try
            {
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    var surveyResponses = ReadAllResponsesWithFieldNames(fields, relateParentId, client);
                    return surveyResponses;
                }

            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return surveyResponse;
        }

        private List<SurveyResponse> ReadAllResponsesWithFieldNames(IDictionary<int, FieldDigest> fieldDigestList, string relateParentId, DocumentClient client)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                // One SurveyResponse per GlobalRecordId
                Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

                List<FormResponseProperties> parentGlobalIdList = ReadAllResponsesByRelateParentResponseId(client, relateParentId, fieldDigestList.FirstOrDefault().Value.FormId);
                string parentGlobalRecordIds = string.Empty;

                if (parentGlobalIdList != null)
                {
                    parentGlobalRecordIds = "'" + string.Join("','", parentGlobalIdList.Select(p => p.GlobalRecordID)) + "'";
                }

                // Query DocumentDB one page at a time. Only query pages that contain a specified field.
                var pageGroups = fieldDigestList.Values.GroupBy(d => d.PageId);
                foreach (var pageGroup in pageGroups)
                {
                    var pageId = pageGroup.Key;
                    var firstPageGroup = pageGroup.First();
                    var formId = firstPageGroup.FormId;
                    var formName = firstPageGroup.FormName;
                    var collectionName = formName + pageId;
                    var columnList = AssembleSelect(collectionName, pageGroup.Select(g => "ResponseQA." + g.FieldName.ToLower()).ToArray())
                           + ","
                           + AssembleSelect(collectionName, "GlobalRecordID", "_ts");
                    Uri pageCollectionUri = GetCollectionUri(client, collectionName);
                    var pageQuery = client.CreateDocumentQuery(pageCollectionUri,
                           SELECT + columnList
                           + FROM + collectionName
                           + WHERE + collectionName + ".GlobalRecordID in (" + parentGlobalRecordIds + ")", queryOptions);

                    foreach (var items in pageQuery.AsQueryable())
                    {
                        var json = JsonConvert.SerializeObject(items);
                        var pageResponseQA = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        string globalRecordId = pageResponseQA["GlobalRecordID"];
                        SurveyResponse surveyResponse;
                        FormResponseDetail formResponseDetail;
                        if (!responsesByGlobalRecordId.TryGetValue(globalRecordId, out surveyResponse))
                        {
                            surveyResponse = new SurveyResponse { SurveyId = new Guid(formId), ResponseId = new Guid(globalRecordId) };
                            responsesByGlobalRecordId.Add(globalRecordId, surveyResponse);
                        }

                        formResponseDetail = surveyResponse.ResponseDetail;
                        formResponseDetail.FormId = formId;
                        formResponseDetail.FormName = formName;

                        Dictionary<string, string> aggregatePageResponseQA;
                        PageResponseDetail pageResponseDetail = formResponseDetail.PageResponseDetailList.SingleOrDefault(p => p.PageId == pageId);
                        if (pageResponseDetail == null)
                        {
                            pageResponseDetail = new PageResponseDetail { FormId = formId, FormName = formName, PageId = pageId };
                            formResponseDetail.AddPageResponseDetail(pageResponseDetail);
                        }

                        aggregatePageResponseQA = pageResponseDetail.ResponseQA;

                        foreach (dynamic qa in pageResponseQA)
                        {
                            string key = qa.Key;
                            switch (key)
                            {
                                case "_ts":
                                    var newTimestamp = Int64.Parse(qa.Value);

                                    string _ts;
                                    var existingPageTimestamp = aggregatePageResponseQA.TryGetValue("_ts", out _ts) ? Int64.Parse(_ts) : 0;
                                    if (newTimestamp > existingPageTimestamp) aggregatePageResponseQA["_ts"] = qa.Value;

                                    if (newTimestamp > surveyResponse.Timestamp)
                                    {
                                        surveyResponse.Timestamp = newTimestamp;
                                        surveyResponse.DateUpdated = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(newTimestamp);
                                    }
                                    break;

                                default:
                                    aggregatePageResponseQA[key] = qa.Value;
                                    break;
                            }
                        }
                    }
                }

                surveyList = responsesByGlobalRecordId.Values.OrderByDescending(v => v.Timestamp).ToList();
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return surveyList;
        }
        #endregion Get All Responses With FieldNames


        #region Get form response properties by ResponseId
        public DocumentResponseProperties GetFormResponsePropertiesByResponseId(string responseId)
        {
            DocumentResponseProperties documentResponseProperties = new DocumentResponseProperties { GlobalRecordID = responseId };

            //Read all global record Id
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                var formResponseProperties = ReadFormPropertiesByResponseId(client, formInfoCollectionUri, responseId);
                documentResponseProperties.FormResponseProperties = formResponseProperties;
            }
            return documentResponseProperties;
        }
        #endregion Get form response properties by ResponseId

        #region Get all page responses by ResponseId
        /// <summary>
        /// GetAllPageResponsesByResponseId
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        public DocumentResponseProperties GetAllPageResponsesByResponseId(string responseId)
        {
            DocumentResponseProperties documentResponseProperties = new DocumentResponseProperties { GlobalRecordID = responseId };
            //Read all global record Id
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                var formResponseProperties = ReadFormPropertiesByResponseId(client, formInfoCollectionUri, responseId);
                documentResponseProperties.FormResponseProperties = formResponseProperties;

                if (formResponseProperties != null)
                {
                    foreach (var pageId in formResponseProperties.PageIds)
                    {
                        if (pageId != 0)
                        {
                            string collectionName = formResponseProperties.FormName + pageId;
                            var pageCollectionUri = GetCollectionUri(client, collectionName);
                            var pageResponseProperties = ReadPageResponsePropertiesByResponseId(responseId, client, pageCollectionUri);
                            documentResponseProperties.PageResponsePropertiesList.Add(pageResponseProperties);
                        }
                    }
                }
            }
            return documentResponseProperties;
        }
        #endregion Get all page responses by ResponseId

        #region Get hierarchial responses by ResponseId for DataConsisitencyServiceAPI
        /// <summary>
        /// GetHierarchialResponsesByResponseId
        /// </summary>
        /// <param name="responseId"></param>
        /// <param name="includeDeletedRecords"></param>
        /// <returns></returns>
        public HierarchicalDocumentResponseProperties GetHierarchialResponsesByResponseId(string responseId, bool includeDeletedRecords = false, bool excludeInProcessRecords = false)
        {
            HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = new HierarchicalDocumentResponseProperties();
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                var documentResponseProperties = ReadAllResponsesByExpression(Expression("GlobalRecordID", EQ, responseId), client, formInfoCollectionUri, includeDeletedRecords).SingleOrDefault();
                if (documentResponseProperties != null)
                {
                    hierarchicalDocumentResponseProperties.FormResponseProperties = documentResponseProperties.FormResponseProperties;
                    hierarchicalDocumentResponseProperties.PageResponsePropertiesList = documentResponseProperties.PageResponsePropertiesList;
                    hierarchicalDocumentResponseProperties.ChildResponseList = GetChildResponses(responseId, client, formInfoCollectionUri);
                }
                return hierarchicalDocumentResponseProperties;
            }
        }

        private List<HierarchicalDocumentResponseProperties> GetChildResponses(string parentResponseId, DocumentClient client, Uri formInfoCollectionUri, bool includeDeletedRecords = false)
        {
            var childResponseList = new List<HierarchicalDocumentResponseProperties>();
            var documentResponsePropertiesList = ReadAllResponsesByExpression(Expression("RelateParentId", EQ, parentResponseId), client, formInfoCollectionUri, includeDeletedRecords);
            foreach (var documentResponseProperties in documentResponsePropertiesList)
            {
                var childResponse = new HierarchicalDocumentResponseProperties();
                childResponse.FormResponseProperties = documentResponseProperties.FormResponseProperties;
                childResponse.PageResponsePropertiesList = documentResponseProperties.PageResponsePropertiesList;
                childResponseList.Add(childResponse);

                childResponse.ChildResponseList = GetChildResponses(documentResponseProperties.FormResponseProperties.GlobalRecordID, client, formInfoCollectionUri, includeDeletedRecords);
            }
            return childResponseList;
        }

        private List<DocumentResponseProperties> ReadAllResponsesByExpression(string expression, DocumentClient client, Uri formInfoCollectionUri, bool includeDeletedRecords, string collectionAlias = "c")
        {
            var documentResponsePropertiesList = new List<DocumentResponseProperties>();
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = client.CreateDocumentQuery(formInfoCollectionUri,
                       SELECT + AssembleSelect(collectionAlias, "*")
                       + FROM + collectionAlias
                       + WHERE + AssembleWhere(collectionAlias, expression, And_Expression("RecStatus", NE, RecordStatus.Deleted, includeDeletedRecords))
                       , queryOptions);

                documentResponsePropertiesList = query.AsEnumerable()
                       .Select(fi => new DocumentResponseProperties { FormResponseProperties = (FormResponseProperties)fi })
                       .ToList();

                // Iterate through the list of form responses to get the associated page responses.
                foreach (var documentResponseProperties in documentResponsePropertiesList)
                {
                    var formResponseProperties = documentResponseProperties.FormResponseProperties;
                    if (formResponseProperties != null)
                    {
                        documentResponseProperties.PageResponsePropertiesList = ReadAllPages(formResponseProperties, client);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return documentResponsePropertiesList;
        }

        private List<PageResponseProperties> ReadAllPages(FormResponseProperties formResponseProperties, DocumentClient client)
        {
            List<PageResponseProperties> pageResponsePropertiesList = new List<PageResponseProperties>();
            var responseId = formResponseProperties.GlobalRecordID;
            foreach (var pageId in formResponseProperties.PageIds)
            {
                if (pageId != 0)
                {
                    string collectionName = formResponseProperties.FormName + pageId;
                    var pageCollectionUri = GetCollectionUri(client, collectionName);
                    var pageResponseProperties = ReadPageResponsePropertiesByResponseId(responseId, client, pageCollectionUri);
                    if (pageResponseProperties != null)
                    {
                        pageResponsePropertiesList.Add(pageResponseProperties);
                    }
                }
            }

            return pageResponsePropertiesList;
        }

        private PageResponseProperties ReadPageResponsePropertiesByResponseId(string responseId, DocumentClient client, Uri pageCollectionUri, string collectionAlias = "c")
        {
            PageResponseProperties pageResponseProperties = null;
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = client.CreateDocumentQuery(pageCollectionUri,
                       SELECT + AssembleSelect(collectionAlias, "*")
                       + FROM + collectionAlias
                       + WHERE + AssembleWhere(collectionAlias, Expression("GlobalRecordID", EQ, responseId))
                       , queryOptions);

                pageResponseProperties = (PageResponseProperties)query.AsEnumerable().FirstOrDefault();
                return pageResponseProperties;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        #endregion Get hierarchial responses by ResponseId for DataConsisitencyServiceAPI


        private List<SurveyResponse> GetAllDataByChildFormIdByRelateId(DocumentClient client, string formId, string relateParentId, Dictionary<int, FieldDigest> fieldDigestList, string collectionName)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                // One SurveyResponse per GlobalRecordId
                Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

                List<FormResponseProperties> childResponses = ReadAllResponsesByRelateParentResponseId(client, relateParentId, formId);
                string childCSVResponseIds = childResponses.Count > 0 ? ("'" + string.Join("','", childResponses.Select(r => r.GlobalRecordID)) + "'") : string.Empty;

                // Query DocumentDB one page at a time. Only query pages that contain a specified field.
                var pageGroups = fieldDigestList.Values.GroupBy(d => d.PageId);
                foreach (var pageGroup in pageGroups)
                {
                    var pageId = pageGroup.Key;
                    var formName = pageGroup.First().FormName;
                    var pageColectionName = collectionName + pageId;
                    var columnList = AssembleSelect(pageColectionName, pageGroup.Select(g => "ResponseQA." + g.FieldName.ToLower()).ToArray())
                           + ","
                           + AssembleSelect(pageColectionName, "GlobalRecordID", "_ts");
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName + pageId);

                    var pageQuery = client.CreateDocumentQuery(docUri, SELECT + columnList + " FROM  " + collectionName + pageId + WHERE + collectionName + pageId + ".GlobalRecordID in ( " + childCSVResponseIds + ")", queryOptions);

                    foreach (var items in pageQuery.AsQueryable())
                    {
                        var json = JsonConvert.SerializeObject(items);
                        var pageResponseQA = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        string globalRecordId = pageResponseQA["GlobalRecordID"];
                        SurveyResponse surveyResponse;
                        FormResponseDetail formResponseDetail;
                        if (!responsesByGlobalRecordId.TryGetValue(globalRecordId, out surveyResponse))
                        {
                            surveyResponse = new SurveyResponse { ResponseId = new Guid(globalRecordId) };
                            responsesByGlobalRecordId.Add(globalRecordId, surveyResponse);
                        }

                        formResponseDetail = surveyResponse.ResponseDetail;

                        Dictionary<string, string> aggregatePageResponseQA;
                        PageResponseDetail pageResponseDetail = formResponseDetail.PageResponseDetailList.SingleOrDefault(p => p.PageId == pageId);
                        if (pageResponseDetail == null)
                        {
                            pageResponseDetail = new PageResponseDetail { PageId = pageId };
                            formResponseDetail.AddPageResponseDetail(pageResponseDetail);
                        }

                        aggregatePageResponseQA = pageResponseDetail.ResponseQA;

                        foreach (dynamic qa in pageResponseQA)
                        {
                            string key = qa.Key;
                            switch (key)
                            {
                                case "_ts":
                                    var newTimestamp = Int64.Parse(qa.Value);

                                    string _ts;
                                    var existingPageTimestamp = aggregatePageResponseQA.TryGetValue("_ts", out _ts) ? Int64.Parse(_ts) : 0;
                                    if (newTimestamp > existingPageTimestamp) aggregatePageResponseQA["_ts"] = qa.Value;

                                    if (newTimestamp > surveyResponse.Timestamp)
                                    {
                                        surveyResponse.Timestamp = newTimestamp;
                                        surveyResponse.DateUpdated = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(newTimestamp);
                                    }
                                    break;

                                default:
                                    aggregatePageResponseQA[key] = qa.Value;
                                    break;
                            }
                        }
                    }
                }

                surveyList = responsesByGlobalRecordId.Values.OrderByDescending(v => v.Timestamp).ToList();
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return surveyList;
        }

        #region Do children exist for responseId
        /// <summary>
        /// DoChildrenExistForResponseId
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        public bool DoChildrenExistForResponseId(string responseId)
        {
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                var count = CountRelatedResponses(responseId, client, formInfoCollectionUri);
                return count != 0;
            }
        }

        private int CountRelatedResponses(string responseId, DocumentClient client, Uri formInfoCollectionUri)
        {
            var collectionAlias = FormInfoCollectionName;

            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = client.CreateDocumentQuery(formInfoCollectionUri,
                       SELECT
                       + AssembleSelect(collectionAlias, "GlobalRecordID")
                       + FROM + collectionAlias
                       + WHERE
                       + AssembleWhere(collectionAlias, Expression("RelateParentId", EQ, responseId),
                                                                       And_Expression("RecStatus", NE, RecordStatus.Deleted))
                       , queryOptions);
                var count = query.AsEnumerable().Count();
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return 0;
        }
        #endregion Do children exist for responseId

        private FormResponseProperties ReadFormInfoByResponseId(string responseId, DocumentClient client, Uri formInfoCollectionUri)
        {
            var collectionAlias = FormInfoCollectionName;

            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = client.CreateDocumentQuery(formInfoCollectionUri,
                       SELECT
                       + AssembleSelect(collectionAlias, "*")
                       + FROM + collectionAlias
                       + WHERE
                       + AssembleWhere(collectionAlias, Expression("GlobalRecordID", EQ, responseId),
                                                                       And_Expression("RecStatus", NE, RecordStatus.Deleted))
                       , queryOptions);
                var surveyDataFromDocumentDB = (FormResponseProperties)query.AsEnumerable().FirstOrDefault();
                return surveyDataFromDocumentDB;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        public int GetFormResponseCount(string formId, bool includeDeletedRecords = false)
        {

            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                var formResponseCount = GetFormResponseCount(client, formInfoCollectionUri, formId, includeDeletedRecords);
                return formResponseCount;
            }
        }

        private int GetFormResponseCount(DocumentClient client, Uri formInfoCollectionUri, string formId, bool includeDeletedRecords)
        {
            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = client.CreateDocumentQuery(formInfoCollectionUri,
                       SELECT
                       + AssembleSelect(FormInfoCollectionName, "GlobalRecordID")
                       + FROM + FormInfoCollectionName
                       + WHERE
                       + AssembleWhere(FormInfoCollectionName, Expression("FormId", EQ, formId),
                                                                       And_Expression("RecStatus", NE, RecordStatus.Deleted, includeDeletedRecords))
                       , queryOptions);
                var formResponseCount = query.AsEnumerable().Count();
                return formResponseCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return 0;
        }

        #region Get Form Response State
        /// <summary>
        ///    GetFormResponseState
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        public FormResponseProperties GetFormResponseState(string responseId)
        {
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                var formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                var formResponseProperties = ReadFormResponseState(responseId, client, formInfoCollectionUri);
                return formResponseProperties;
            }
        }

        private FormResponseProperties ReadFormResponseState(string responseId, DocumentClient client, Uri formInfoCollectionUri)
        {
            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = client.CreateDocumentQuery(formInfoCollectionUri,
                       SELECT
                       + AssembleSelect(FormInfoCollectionName, "*")
                       + FROM + FormInfoCollectionName
                       + WHERE
                       + AssembleWhere(FormInfoCollectionName, Expression("GlobalRecordID", EQ, responseId),
                                                                       And_Expression("RecStatus", NE, RecordStatus.Deleted))
                       , queryOptions);
                var formResponseProperties = (FormResponseProperties)query.AsEnumerable().FirstOrDefault();
                return formResponseProperties;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }
        #endregion GetFormResponseState

        #region Read All Responses By RelateParentResponseId
        private List<FormResponseProperties> ReadAllResponsesByRelateParentResponseId(DocumentClient client, string relateParentId, string formId)
        {
            try
            {
                List<FormResponseProperties> globalRecordIdList = new List<FormResponseProperties>();

                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                IQueryable<dynamic> query;

                Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
                bool skipAnd = formId == null;
                if (relateParentId != null)
                {
                    query = client.CreateDocumentQuery(formInfoCollectionUri,
                           SELECT
                           + AssembleSelect(FormInfoCollectionName, "*")
                           + FROM + FormInfoCollectionName
                           + WHERE
                           + AssembleWhere(FormInfoCollectionName, Expression("RelateParentId", EQ, relateParentId),
                                                                                            And_Expression("RecStatus", NE, RecordStatus.Deleted),
                                                                                            And_Expression("FormId", EQ, formId, skipAnd))
                           , queryOptions);
                }
                else
                {
                    query = client.CreateDocumentQuery(formInfoCollectionUri,
                       SELECT
                       + AssembleSelect(FormInfoCollectionName, "*")
                       + FROM + FormInfoCollectionName
                       + WHERE
                       + AssembleWhere(FormInfoCollectionName, Expression("RecStatus", NE, 0),
                                                                                         And_Expression("FormId", EQ, formId, skipAnd))
                       , queryOptions);
                }

                globalRecordIdList = query.AsEnumerable<dynamic>().Select(x => (FormResponseProperties)x).ToList();

                return globalRecordIdList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        #endregion

        #region Read List of all GlobalRecordId by FormId, RecStatus
        private FormResponseProperties ReadFormPropertiesByResponseId(DocumentClient client, Uri formInfoCollectionUri, string responseId)
        {
            // tell server we only want 25 record
            FeedOptions options = new FeedOptions { MaxItemCount = 25, EnableCrossPartitionQuery = true };
            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = client.CreateDocumentQuery(formInfoCollectionUri,
                       SELECT
                       + AssembleSelect(FormInfoCollectionName, "*")
                       + FROM + FormInfoCollectionName
                       + WHERE
                       + AssembleWhere(FormInfoCollectionName, Expression("GlobalRecordID", EQ, responseId),
                                                                                            And_Expression("RecStatus", NE, RecordStatus.Deleted))
                       , queryOptions);

                FormResponseProperties formResponseProperties = query.AsEnumerable().FirstOrDefault();
                return formResponseProperties;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }
        #endregion
    }
}

