#define ConfigureIndexing

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epi.Common.Utilities;
using Epi.Cloud.Common.Constants;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.Metadata;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Extensions;
using Epi.PersistenceServices.DocumentDB;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class SurveyResponseCRUD : MetadataAccessor
    {
        private string DatabaseName;
        private string AttachmentId = ConfigurationManager.AppSettings[AppSettings.Key.AttachmentId];

        public SurveyResponseCRUD()
        {
            Initialize();
        }

        private DocumentClient Client
        {
            get { return _client ?? GetOrCreateClient(); }
        }

        private Microsoft.Azure.Documents.Database ResponseDatabase
        {
            get { return _database ?? GetOrCreateDatabase(DatabaseName); }
        }

        /// <summary>
        /// GetFormResponseState
        /// </summary>
        /// <param name="responseContext"></param>
        /// <returns></returns>
        public FormResponseProperties GetFormResponseState(IResponseContext responseContext)
        {
            var rootResponseResource = ReadRootResponseResource(responseContext);
            FormResponseProperties formResponseProperties = null;
            if (responseContext.IsRootResponse)
            {
                formResponseProperties = rootResponseResource.FormResponseProperties;
            }
            else
            {
                formResponseProperties = rootResponseResource.GetChildResponse(responseContext);
            }

            // Only return the basic state information (no field values or children)
            formResponseProperties.ResponseQA = new Dictionary<string, string>();
            return formResponseProperties;
        }

        private FormResponseResource ReadRootResponseResource(IResponseContext responseContext)
        {
            try
            {
                var rootResponseId = responseContext.RootResponseId ?? responseContext.ResponseId;
                var rootFormCollectionName = responseContext.RootFormName ?? GetRootFormName(responseContext.RootFormId);
                var rootFormCollectionUri = GetCollectionUri(rootFormCollectionName);
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

                var query = Client.CreateDocumentQuery(rootFormCollectionUri,
                    SELECT
                    + AssembleSelect(rootFormCollectionName, "*")
                    + FROM + rootFormCollectionName
                    + WHERE
                    + AssembleWhere(rootFormCollectionName, Expression("id", EQ, rootResponseId),
                                                            And_Expression(RecStatus, NE, RecordStatus.Deleted))
                    , queryOptions);

                FormResponseResource formResponseResource = query.AsEnumerable().FirstOrDefault();
                return formResponseResource;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }




        #region Delete
        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="responseContext"></param>
        /// <param name="deleteType"></param>
        /// <returns></returns>
        public async Task<FormResponseResource> Delete(IResponseContext responseContext, int deleteType)
        {
            var rootResponseId = responseContext.RootResponseId ?? responseContext.ResponseId;
            var rootFormName = responseContext.RootFormName ?? GetRootFormName(responseContext.RootFormId);
            

            bool isSuccessful = false;
            try
            {
                Uri rootFormCollectionUri = GetCollectionUri(rootFormName);

                FormResponseDetail hierarchialFormResponseDetail = null;

                var formResponseResource = ReadRootResponseResource(responseContext, false);
                if (formResponseResource != null)
                {
                    FormResponseDetail formResponseDetail = null;

                    var formResponseProperties = formResponseResource.FormResponseProperties;

                    // Determine if the ResponseId parameter is the root responseId or a child responseId
                    if (responseContext.IsRootResponse)
                    {
                        // First logically delete the root and child responses
                        isSuccessful = await LogicallyDeleteResponse(formResponseResource, formResponseProperties).ConfigureAwait(false);
                        formResponseProperties.RecStatus = RecordStatus.Deleted;

                        hierarchialFormResponseDetail = formResponseProperties.ToHierarchialFormResponseDetail(formResponseResource);

                        if (deleteType == RecordStatus.PhysicalDelete)
                        {
                            isSuccessful = await PhysicallyDeleteResponse(formResponseResource, formResponseProperties).ConfigureAwait(false);
                        }
                    }
                    else // if (responseContext.IsChildResponse)
                    {
                        formResponseProperties = formResponseResource.GetChildResponse(responseContext);

                        // only logically delete child responses
                        isSuccessful = await LogicallyDeleteResponse(formResponseResource, formResponseProperties).ConfigureAwait(false);

                        hierarchialFormResponseDetail = formResponseProperties.ToHierarchialFormResponseDetail(formResponseResource);
                    }

                    // TODO: send hierarchialFormResponseDetail to consistency service 

                    if (deleteType != RecordStatus.PhysicalDelete)
                    {
                        var result = await Client.UpsertDocumentAsync(rootFormCollectionUri, formResponseResource).ConfigureAwait(false);

                        return formResponseResource;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        private async Task<bool> PhysicallyDeleteResponse(FormResponseResource formResponseResource, FormResponseProperties formResponseProperties)
        {
            if (formResponseProperties.IsRootResponse)
            {
                var result = await Client.DeleteDocumentAsync(formResponseResource.SelfLink).ConfigureAwait(false);
            }
            else // if (formResponseProperties.IsChildResponse)
            {
                var result = await Client.UpsertDocumentAsync(formResponseResource.SelfLink, formResponseResource, null).ConfigureAwait(false);
            }
            return await Task.FromResult<bool>(false).ConfigureAwait(false);
        }

        private async Task<bool> LogicallyDeleteResponse(FormResponseResource formResponseResource, FormResponseProperties formResponseProperties)
        {
            if (formResponseProperties.IsRootResponse)
            {
                formResponseResource.LogicalCascadeDeleteChildren(formResponseProperties);
            }
            else // if (formResponseProperties.IsChildResponse)
            {
                formResponseResource.LogicalCascadeDeleteChildren(formResponseProperties);
            }


            return await Task.FromResult<bool>(true).ConfigureAwait(false);

            //if (formResponseProperties.RecStatus != RecordStatus.Deleted)
            //{
            //    formResponseResource.LogicalCascadeDelete(formResponseProperties);
            //    var result = await Client.UpsertDocumentAsync(formResponseResource.SelfLink, formResponseResource, null).ConfigureAwait(false);
            //}
        }

        #endregion

        #region UpdateAttachment
        /// <summary>
        /// UpdateAttachment
        /// </summary>
        /// <param name="responseContext"></param>
        /// <param name="responseStatus"></param>
        /// <param name="userId"></param>
        /// <param name="newResponseStatus"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAttachment(IResponseContext responseContext, int responseStatus, int userId = 0, int newResponseStatus = 0)
        {
            var responseId = responseContext.ResponseId;
            var rootFormName = responseContext.RootFormName;

            newResponseStatus = responseStatus;
            Attachment attachment = null;
            bool isSuccessful = false;
            bool deleteResponse = false;
            try
            {
                Uri rootFormCollectionUri = GetCollectionUri(rootFormName);
                FormResponseResource formResponseResource = ReadRootResponseResource(responseContext);
                if (formResponseResource != null)
                {
                    var formResponseProperties = formResponseResource.FormResponseProperties;
                    //Is status is Saved and check if attachment is existed or not.If attachment is null and delete attachment
                    if (newResponseStatus == RecordStatus.Saved)
                    {
                        attachment = ReadAttachment(rootFormName, responseId, AttachmentId);
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
                                formResponseProperties.IsNewRecord = false;
                                formResponseProperties.RecStatus = RecordStatus.Saved;
                                var formResponseSave = await Client.UpsertDocumentAsync(rootFormCollectionUri, formResponseResource).ConfigureAwait(false);
                                break;
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



        #region Restore Last Response Snapshot
        /// <summary>
        /// RestoreLastResponseSnapshot
        /// </summary>
        /// <param name="snapshotFormResponseResource"></param>
        /// <returns></returns>
        public bool RestoreLastResponseSnapshot(FormResponseResource snapshotFormResponseResource)
        {
            var snapshotFormResponseProperties = snapshotFormResponseResource.FormResponseProperties;
            string rootFormName = snapshotFormResponseProperties.FormName;
            Uri rootFormCollectionUri = GetCollectionUri(rootFormName);
            var resourceResponse = Client.UpsertDocumentAsync(rootFormCollectionUri, snapshotFormResponseResource).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }

        #endregion

        #region Save Form Response Properties Async

        /// <summary>
        /// This method help to save form properties 
        /// and also used for delete operation.Ex:RecStatus=0
        /// </summary>
        /// <param name="formResponseProperties"></param>
        /// <returns></returns>
        public async Task<ResourceResponse<Document>> SaveFormResponsePropertiesAsync(IResponseContext responseContext, FormResponseProperties formResponseProperties)
        {
            var now = DateTime.UtcNow;

            ResourceResponse<Document> result = null;
            var formResponseResource = new FormResponseResource();
            var rootFormCollectionUri = GetCollectionUri(responseContext.RootFormName);
            var isUpdated = false;
            try
            {
                //Verify Response Id exists or not
                formResponseResource = ReadRootResponseResource(responseContext, true);
                if (formResponseResource == null)
                {
                    if (responseContext.IsRootResponse)
                    {
                        formResponseProperties.IsNewRecord = true;
                        formResponseProperties.FirstSaveTime = now;
                        formResponseProperties.LastSaveTime = now;

                        formResponseResource = new FormResponseResource
                        {
                            Id = responseContext.RootResponseId,
                            FormResponseProperties = formResponseProperties
                        };
                        result = await Client.UpsertDocumentAsync(rootFormCollectionUri, formResponseResource).ConfigureAwait(false);
                    }
                    else // if (responseContext.IsChildResponse)
                    {
                    }
                }
                else
                {
                    var existingFormResponseProperties = formResponseResource.FormResponseProperties;
                    if (responseContext.IsRootResponse)
                    {
                        formResponseProperties.FirstSaveTime = existingFormResponseProperties.FirstSaveTime;
                        formResponseProperties.FirstSaveLogonName = existingFormResponseProperties.FirstSaveLogonName;

                        //Create attachment if transitioning from Saved to InProcess
                        if (existingFormResponseProperties.RecStatus == RecordStatus.Saved && formResponseProperties.RecStatus == RecordStatus.InProcess)
                        {
                            Attachment attachment = null;
                            var existingResponseJson = JsonConvert.SerializeObject(formResponseResource);
                            attachment = CreateAttachment(formResponseResource.SelfLink, AttachmentId, responseContext.RootFormName, existingFormResponseProperties.ResponseId, existingResponseJson);
                        }

                        formResponseResource.FormResponseProperties = formResponseProperties;
                        // Update the root response and its children.
                        result = await Client.UpsertDocumentAsync(rootFormCollectionUri, formResponseResource).ConfigureAwait(false);
                    }
                    else /* if (responseContext.IsChildResponse) */
                    {
                        var childFormResponseProperties = formResponseResource.GetChildResponse(responseContext);
                        if (childFormResponseProperties == null)
                        {
                            formResponseResource.AddOrReplaceChildResponse(formResponseProperties);
                            isUpdated = true;
                        }
                        else
                        {
                            formResponseResource.AddOrReplaceChildResponse(formResponseProperties);
                            isUpdated = true;
                        }
                    }

                        //|| existingFormResponseProperties.LastSaveTime != formResponseProperties.LastSaveTime
                        //|| existingFormResponseProperties.RecStatus != formResponseProperties.RecStatus
                        //|| existingFormResponseProperties.HiddenFieldsList != formResponseProperties.HiddenFieldsList
                        //|| existingFormResponseProperties.DisabledFieldsList != formResponseProperties.DisabledFieldsList
                        //|| existingFormResponseProperties.HighlightedFieldsList != formResponseProperties.HighlightedFieldsList
                        //|| existingFormResponseProperties.RequiredFieldsList != formResponseProperties.RequiredFieldsList;

                    if (isUpdated)
                    {
                        result = await Client.UpsertDocumentAsync(rootFormCollectionUri, formResponseResource, null).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return result;
        }
        #endregion


        #region Get All Responses With FieldNames 
        /// <summary>
        /// GetAllResponsesWithCriteria
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="searchFields"></param>
        /// <param name="parentResponseId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public List<FormResponseDetail> GetAllResponsesWithCriteria(IResponseContext responseContext, IDictionary<int, FieldDigest> fields, IDictionary<int, KeyValuePair<FieldDigest, string>> searchFields, string parentResponseId = null, int pageSize = 0, int pageNumber = 0)
        {
            List<FormResponseDetail> formResponseDetailList = null;

            try
            {
                formResponseDetailList = ReadAllResponsesWithCriteria(responseContext, fields, searchFields, parentResponseId, pageSize, pageNumber);
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return formResponseDetailList;
        }

        private List<FormResponseDetail> ReadAllResponsesWithCriteria(IResponseContext responseContext, IDictionary<int, FieldDigest> fieldDigestList, IDictionary<int, KeyValuePair<FieldDigest, string>> searchQualifiers, string parentResponseId, int pageSize = 0, int pageNumber = 0)
        {
            List<FormResponseDetail> formResponseDetailList = new List<FormResponseDetail>();
            List<FormResponseProperties> formResponsePropertiesList;

            if (responseContext.IsChildResponse)
            {
                if (searchQualifiers != null && searchQualifiers.Count > 0) throw new ArgumentException("Search not available on child forms");
                formResponsePropertiesList = ReadAllChildResponses(responseContext, pageSize, pageNumber, /*includeChildrenOfChild*/false);
                formResponseDetailList = formResponsePropertiesList.ToFormResponseDetailList();
            }
            else
            {
                try
                {
                    var searchFieldNameValueQualifiers = searchQualifiers != null && searchQualifiers.Count > 0 
                        ? searchQualifiers.Values.ToArray() 
                        : null;

                    formResponseDetailList = ReadAllRootResponses(responseContext, searchFieldNameValueQualifiers, pageSize, pageNumber, false).ToFormResponseDetailList();
                }
                catch (DocumentQueryException ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            return formResponseDetailList;
        }
        #endregion Get All Responses With FieldNames

        #region Get hierarchial responses by ResponseId
        /// <summary>
        /// GetHierarchialResponseListByResponseId
        /// </summary>
        /// <param name="responseId"></param>
        /// <param name="includeDeletedRecords"></param>
        /// <param name="excludeInProcessRecords"></param>
        /// <returns></returns>
        /// <remarks> Used by the DataConsisitencyServiceAPI</remarks>
        public List<FormResponseProperties> GetHierarchialResponseListByResponseId(IResponseContext responseContext, bool includeDeletedRecords = false, bool excludeInProcessRecords = false)
        {
            var rootResponseResource = ReadRootResponseResource(responseContext, includeDeletedRecords);
            var rootformResponseProperties = rootResponseResource.FormResponseProperties;
            var formResponsePropertiesList = new List<FormResponseProperties>();
            if (includeDeletedRecords ? true : rootformResponseProperties.RecStatus != RecordStatus.Deleted && excludeInProcessRecords ? rootformResponseProperties.RecStatus != RecordStatus.InProcess : true)
            {
                formResponsePropertiesList.Add(rootResponseResource.FormResponseProperties);
                foreach (var childResponse in rootResponseResource.ChildResponses.Values)
                {
                    var responseDictionary = childResponse.Values;
                    foreach (var responses in responseDictionary)
                    {
                        formResponsePropertiesList.AddRange(responses.Where(r => (includeDeletedRecords ? true : (r.RecStatus != RecordStatus.Deleted)) && (excludeInProcessRecords ? (r.RecStatus != RecordStatus.InProcess) : true)));
                    }
                }
            }

            return formResponsePropertiesList;
        }

#if false
        private List<DocumentResponseProperties> ReadAllResponsesByExpression(string expression, Uri formInfoCollectionUri, bool includeDeletedRecords, string collectionAlias = "c")
        {
            var documentResponsePropertiesList = new List<DocumentResponseProperties>();
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = Client.CreateDocumentQuery(formInfoCollectionUri,
                    SELECT + AssembleSelect(collectionAlias, "*")
                    + FROM + collectionAlias
                    + WHERE + AssembleWhere(collectionAlias, expression, And_Expression(RecStatus, NE, RecordStatus.Deleted, includeDeletedRecords))
                    , queryOptions);

                documentResponsePropertiesList = query.AsEnumerable()
                    .Select(fi => new DocumentResponseProperties { FormResponseProperties = (FormResponseProperties)fi })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return documentResponsePropertiesList;
        }

        private List<DocumentResponseProperties> ReadAllResponsesIdsByExpression(string expression, Uri rootFormCollectionUri, bool includeDeletedRecords, string collectionAlias = "c")
        {
            var documentResponsePropertiesList = new List<DocumentResponseProperties>();
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = Client.CreateDocumentQuery(rootFormCollectionUri,
                    SELECT + AssembleSelect(collectionAlias, "*")
                    + FROM + collectionAlias
                    + WHERE + AssembleWhere(collectionAlias, expression, And_Expression(RecStatus, NE, RecordStatus.Deleted, includeDeletedRecords))
                    , queryOptions);

                documentResponsePropertiesList = query.AsEnumerable()
                    .Select(fi => new DocumentResponseProperties { FormResponseProperties = (FormResponseProperties)fi })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return documentResponsePropertiesList;
        }
#endif

        #endregion Get hierarchial responses by ResponseId

#if false
        private List<SurveyResponse> GetAllDataByChildFormIdByRelateId(string formId, string parentResponseId, Dictionary<int, FieldDigest> fieldDigestList, string collectionName)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                // One SurveyResponse per GlobalRecordId
                Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

                List<FormResponseProperties> childResponses = ReadAllResponsesByRelateParentResponseId(parentResponseId, formId);
                string childCSVResponseIds = childResponses.Count > 0 ? ("'" + string.Join("','", childResponses.Select(r => r.ResponseId)) + "'") : string.Empty;

                // Query DocumentDB one page at a time. Only query pages that contain a specified field.
                var pageGroups = fieldDigestList.Values.GroupBy(d => d.PageId);
                foreach (var pageGroup in pageGroups)
                {
                    var pageId = pageGroup.Key;
                    var formName = pageGroup.First().FormName;
                    var pageColectionName = collectionName + pageId;
                    var columnList = AssembleSelect(pageColectionName, pageGroup.Select(g => "ResponseQA." + g.FieldName.ToLower()).ToArray())
                        + ","
                        + AssembleSelect(pageColectionName, "ResponseId", "_ts");
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName + pageId);

                    var pageQuery = Client.CreateDocumentQuery(docUri, SELECT + columnList + " FROM  " + collectionName + pageId + WHERE + collectionName + pageId + ".GlobalRecordID in ( " + childCSVResponseIds + ")", queryOptions);

                    foreach (var items in pageQuery.AsQueryable())
                    {
                        var json = JsonConvert.SerializeObject(items);
                        var pageResponseQA = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        string globalRecordId = pageResponseQA["ResponseId"];
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
#endif

        public bool DoChildResponsesExist(IResponseContext responseContext, bool includeDeletedRecords = false)
        {
            var rootResponseResource = ReadRootResponseResource(responseContext);
            var childResponsePropertiesList = rootResponseResource.GetChildResponseList(responseContext, /*addIfNoList*/true);
            if (!includeDeletedRecords)
            {
                childResponsePropertiesList = childResponsePropertiesList.Where(r => r.RecStatus != RecordStatus.Deleted).ToList();
            }
            return childResponsePropertiesList != null && childResponsePropertiesList.Count > 0;
        }

        /// <summary>
        /// GetFormResponseCount
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="includeDeletedRecords"></param>
        /// <returns></returns>
        public int GetFormResponseCount(string formId, bool includeDeletedRecords = false)
        {
            var rootFormId = GetRootFormId(formId);
            var rootFormName = GetRootFormName(formId);
            Uri rootFormCollectionUri = GetCollectionUri(rootFormName);
            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = Client.CreateDocumentQuery(rootFormCollectionUri,
                    SELECT
                    + AssembleSelect(rootFormName, "id")
                    + FROM + rootFormName
                    + (includeDeletedRecords
                        ? string.Empty 
                        : WHERE + AssembleWhere(rootFormName, Expression(RecStatus, NE, RecordStatus.Deleted)))
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

#region Get Response
        /// <summary>
        ///	GetFormResponseState
        /// </summary>
        /// <param name="responseContext"></param>
        /// <returns></returns>
        public FormResponseProperties GetResponse(IResponseContext responseContext, bool includeDeleted = false)
        {
            var formResponseProperties = ReadResponse(responseContext, includeDeleted);
            return formResponseProperties;
        }

        private FormResponseProperties ReadResponse(IResponseContext responseContext, bool includeDeleted)
        {
            FormResponseProperties formResponseProperties = null;
            try
            {
                FormResponseResource formResponseResource = ReadRootResponseResource(responseContext, includeDeleted);
                if (formResponseResource != null)
                {
                    formResponseProperties = formResponseResource.FormResponseProperties;
                    if (!includeDeleted && formResponseProperties.RecStatus == RecordStatus.Deleted)
                    {
                        formResponseProperties = null;
                    }
                    else
                    {
                        var rootResponseId = responseContext.RootResponseId ?? responseContext.ResponseId;
                        var responseId = responseContext.ResponseId;

                        if (responseId != rootResponseId)
                        {
                            formResponseProperties = formResponseResource.GetChildResponse(responseContext);
                            if (formResponseProperties != null && !includeDeleted && formResponseProperties.RecStatus == RecordStatus.Deleted)
                            {
                                formResponseProperties = null;
                            }
                        }
                    }
                }

                return formResponseProperties;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        private FormResponseResource ReadRootResponseResource(IResponseContext responseContext, bool includeDeleted)
        {
            try
            {
                var rootFormName = responseContext.RootFormName ?? this.GetRootFormName(responseContext.RootFormId);
                var rootResponseId = responseContext.RootResponseId;

                var collectionAlias = rootFormName;
                var rootFormCollectionUri = GetCollectionUri(rootFormName);

                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = Client.CreateDocumentQuery(rootFormCollectionUri,
                    SELECT
                    + AssembleSelect(collectionAlias, "*")
                    + FROM + collectionAlias
                    + WHERE
                    + AssembleWhere(collectionAlias, Expression("id", EQ, rootResponseId),
                                                And_Expression(RecStatus, NE, RecordStatus.Deleted, includeDeleted))
                    , queryOptions);
                var formResponseResource = (FormResponseResource)query.AsEnumerable().FirstOrDefault();
                if (formResponseResource != null)
                {
                    ResolveMissingContext(formResponseResource, responseContext);
                }
                return formResponseResource;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        private static void ResolveMissingContext(FormResponseResource formResponseResource, IResponseContext responseContext)
        {
            // verify that the response context is fully resolved.
            FormResponseResource.ResponseDirectory responseDirectory = null;
            if (formResponseResource.ChildResponseIndex.TryGetValue(responseContext.ResponseId, out responseDirectory))
            {
                responseContext.FormId = responseDirectory.FormId;
                responseContext.FormName = responseDirectory.FormName;
                responseContext.ParentFormId = responseDirectory.ParentFormId;
                responseContext.ParentFormName = responseDirectory.ParentFormName;
                responseContext.ParentResponseId = responseDirectory.ParentResponseId;
            }
        }


        #endregion GetResponse

        private List<FormResponseProperties> ReadAllRootResponses(IResponseContext responseContext, KeyValuePair<FieldDigest, string>[] searchQualifiers = null,  int pageSize = 0, int pageNumber = 0, bool includeChildren = false)
        {
            try
            {
                var rootFormName = responseContext.RootFormName ?? this.GetRootFormName(responseContext.RootFormId);

                var collectionAlias = rootFormName;
                var rootFormCollectionUri = GetCollectionUri(rootFormName);
                var nonWildSearchQualifiers = searchQualifiers != null
                    ? searchQualifiers.Where(q => !(q.Value.Contains('*') || q.Value.Contains('?'))).ToArray()
                    : null; 
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = Client.CreateDocumentQuery(rootFormCollectionUri,
                    SELECT
                    + AssembleSelect(collectionAlias, "*")
                    + FROM + collectionAlias
                    + WHERE
                    + (nonWildSearchQualifiers != null && nonWildSearchQualifiers.Length > 0
                        ? AssembleWhere(collectionAlias, Expression(RecStatus, NE, RecordStatus.Deleted), And_SearchExpressions(nonWildSearchQualifiers))
                        : AssembleWhere(collectionAlias, Expression(RecStatus, NE, RecordStatus.Deleted)))
                    , queryOptions);

                var formResponsePropertiesList = query.AsEnumerable().Select(r => ((FormResponseResource)r).FormResponseProperties).ToArray();
                var response = FilterQueryResponseByWildCardQualifiers(formResponsePropertiesList, searchQualifiers);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        private List<FormResponseProperties> FilterQueryResponseByWildCardQualifiers(IEnumerable<FormResponseProperties> enumerableFormResponseProperties, KeyValuePair<FieldDigest, string>[] searchQualifiers)
        {
            if (searchQualifiers != null)
            {
                var wildQualifiers = searchQualifiers.Where(search => search.Value.Contains("*") || search.Value.Contains("?")).ToArray();
                if (wildQualifiers.Length > 0)
                {
                    List<FormResponseProperties> filteredFormResponsePropertiesList = new List<FormResponseProperties>();

                    foreach (var formReponseProperties in enumerableFormResponseProperties)
                    {
                        bool isQuailfied = true;
                        foreach (var searchQuery in wildQualifiers)
                        {
                            var fieldName = searchQuery.Key.FieldName;
                            var searchValue = searchQuery.Value;
                            string responseValue;
                            bool responseExists = formReponseProperties.ResponseQA.TryGetValue(fieldName, out responseValue);
                            if (!responseValue.WildcardCompare(searchValue))
                            {
                                isQuailfied = false;
                                break;
                            }
                        }
                        if (isQuailfied) filteredFormResponsePropertiesList.Add(formReponseProperties);
                    }
                    return filteredFormResponsePropertiesList;
                }
            }
            return enumerableFormResponseProperties.ToList();
        }

        #region Read All Child Responses
        private List<FormResponseProperties> ReadAllChildResponses(IResponseContext responseContext, int pageSize = 0, int pageNumber = 0, bool includeChildrenOfChild = false)
        {
            try
            {
                List<FormResponseProperties> childResponses = new List<FormResponseProperties>();

                var rootFormName = responseContext.RootFormName;
                var rootResponseResource = ReadRootResponseResource(responseContext, false);
                var formResponsePropertiesList = rootResponseResource.GetChildResponseList(responseContext)
                    .Where(r => r.RecStatus != RecordStatus.Deleted).ToList();

                if (formResponsePropertiesList != null && formResponsePropertiesList.Count > 0)
                {
                    childResponses = formResponsePropertiesList
                        .OrderByDescending(r => r.LastSaveTime)
                        .ToList();
                    if (pageSize > 0)
                    {
                        childResponses = childResponses.Skip((pageNumber * pageSize) - pageSize).Take(pageSize).ToList();
                    }
                }

                return childResponses;
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
