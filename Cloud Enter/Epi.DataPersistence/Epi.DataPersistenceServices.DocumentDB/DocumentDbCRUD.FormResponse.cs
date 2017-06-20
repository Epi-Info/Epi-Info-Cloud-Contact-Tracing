#define ConfigureIndexing

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Common.Core.Interfaces;
using Epi.Common.Utilities;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Extensions;
using Epi.FormMetadata.DataStructures;
using Epi.PersistenceServices.DocumentDB;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class DocumentDbCRUD
    {
        private string DatabaseName;
        private string AttachmentId = ConfigurationManager.AppSettings[AppSettings.Key.AttachmentId];

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
                                                            And_Expression(FRP_RecStatus, NE, RecordStatus.Deleted))
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

        #region DeleteResponse
        /// <summary>
        /// DeleteResponse
        /// </summary>
        /// <param name="responseContext"></param>
        /// <param name="deleteType"></param>
        /// <returns></returns>
        public async Task<FormResponseResource> DeleteResponse(IResponseContext responseContext, int deleteType)
        {
            var rootResponseId = responseContext.RootResponseId ?? responseContext.ResponseId;
            var rootFormName = responseContext.RootFormName ?? GetRootFormName(responseContext.RootFormId);


            bool isSuccessful = false;
            try
            {
                Uri rootFormCollectionUri = GetCollectionUri(rootFormName);

                FormResponseDetail hierarchicalFormResponseDetail = null;

                var formResponseResource = ReadRootResponseResource(responseContext, false);
                if (formResponseResource != null)
                {
                    var formResponseProperties = formResponseResource.FormResponseProperties;

                    // Determine if the ResponseId parameter is the root responseId or a child responseId
                    if (responseContext.IsRootResponse)
                    {
                        // First logically delete the root and child responses
                        isSuccessful = await LogicallyDeleteResponse(formResponseResource, formResponseProperties).ConfigureAwait(false);
                        formResponseProperties.RecStatus = RecordStatus.Deleted;

                        hierarchicalFormResponseDetail = formResponseProperties.ToHierarchicalFormResponseDetail(formResponseResource);

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

                        hierarchicalFormResponseDetail = formResponseProperties.ToHierarchicalFormResponseDetail(formResponseResource);
                    }

                    if (deleteType != RecordStatus.PhysicalDelete && hierarchicalFormResponseDetail != null)
                    {
                        if (responseContext.IsChildResponse)
                        {
                            var result = await Client.UpsertDocumentAsync(rootFormCollectionUri, formResponseResource).ConfigureAwait(false);
                        }

                        // Send hierarchicalFormResponseDetail to consistency service 
                        var surveyPersistenceFacade = new DocDB_SurveyPersistenceFacade();
                        surveyPersistenceFacade.NotifyConsistencyService(hierarchicalFormResponseDetail);

                        if (responseContext.IsRootResponse)
                        {
                            var result = await PhysicallyDeleteResponse(formResponseResource, formResponseProperties).ConfigureAwait(false);
                        }

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
            newResponseStatus = responseStatus;
            bool isSuccessful = false;
            bool deleteResponse = false;
            try
            {
                Uri rootFormCollectionUri = GetCollectionUri(responseContext.RootFormName);
                FormResponseResource formResponseResource = ReadRootResponseResource(responseContext);
                if (formResponseResource != null)
                {
                    var formResponseProperties = formResponseResource.FormResponseProperties;
                    //Is status is Saved and check if attachment is existed or not.If attachment is null and delete attachment
                    if (newResponseStatus == RecordStatus.Saved)
                    {
                        Attachment attachment = ReadResponseAttachment(responseContext, AttachmentId);
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
            var documentResponse = ExecuteWithFollowOnAction(() => Client.UpsertDocumentAsync(rootFormCollectionUri, snapshotFormResponseResource));
            var isSuccessful = documentResponse != null;
            return isSuccessful;
        }

        #endregion

        #region Save Form Response Properties Async

        /// <summary>
        /// This method will save form properties 
        /// and also used for delete operation.Ex:RecStatus=0
        /// </summary>
        /// <param name="formResponsePropertiesList"></param>
        /// <returns></returns>
        public async Task<ResourceResponse<Document>> SaveFormResponsePropertiesAsync(List<FormResponseProperties> formResponsePropertiesList)
        {
            var now = DateTime.UtcNow;
            var rootFormResponseProperties = formResponsePropertiesList[0];
            ResourceResponse<Document> result = null;
            var formResponseResource = new FormResponseResource();
            var rootFormCollectionUri = GetCollectionUri(rootFormResponseProperties.RootFormName);
            try
            {
                //Verify that the Root Response Id exists
                formResponseResource = ReadRootResponseResource((IResponseContext)rootFormResponseProperties);
                if (formResponseResource == null)
                {
                    if (rootFormResponseProperties.IsRootResponse)
                    {
                        formResponseResource = new FormResponseResource
                        {
                            Id = rootFormResponseProperties.RootResponseId,
                            FormResponseProperties = rootFormResponseProperties
                        };
                    }
                    else // if (responseContext.IsChildResponse)
                    {
                        throw new Exception("Can't add a child response without an existing root response");
                    }
                }
                var existingFormResponseProperties = formResponseResource.FormResponseProperties;
                foreach (var formResponseProperties in formResponsePropertiesList)
                {
                    formResponseProperties.LastSaveTime = now;

                    if (formResponseProperties.IsRootResponse)
                    {
                        formResponseProperties.FirstSaveTime = existingFormResponseProperties.FirstSaveTime;
                        formResponseProperties.FirstSaveLogonName = existingFormResponseProperties.FirstSaveLogonName;

                        //Create attachment if transitioning from Saved to InProcess
                        if (existingFormResponseProperties.RecStatus == RecordStatus.Saved && formResponseProperties.RecStatus == RecordStatus.InProcess)
                        {
                            Attachment attachment = null;
                            var existingResponseJson = JsonConvert.SerializeObject(formResponseResource);
                            attachment = CreateResponseAttachment(formResponseResource.SelfLink, (IResponseContext)formResponseProperties, AttachmentId, existingResponseJson);
                        }

                        formResponseResource.FormResponseProperties = formResponseProperties;
                    }
                    else /* if (responseContext.IsChildResponse) */
                    {
                        formResponseResource.AddOrReplaceChildResponse(formResponseProperties);
                    }
                }

                // Update the root response and its children.
                result = await Client.UpsertDocumentAsync(rootFormCollectionUri, formResponseResource).ConfigureAwait(false);
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
            List<string> columnNames = new List<string>();
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
                    if (searchFieldNameValueQualifiers == null)
                    {
                        foreach (var fildname in fieldDigestList)
                        {
                            columnNames.Add(fildname.Value.TrueCaseFieldName);
                        }
                    }
                    else
                    {
                        foreach (var fildname in searchFieldNameValueQualifiers)
                        {
                            columnNames.Add(fildname.Value);
                        }
                    }


                    formResponseDetailList = ReadAllRootResponses(responseContext, columnNames, searchFieldNameValueQualifiers, pageSize, pageNumber, false).ToFormResponseDetailList();

                }
                catch (DocumentQueryException ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            return formResponseDetailList;
        }
        #endregion Get All Responses With FieldNames

        #region Get hierarchical responses by ResponseId
        /// <summary>
        /// GetHierarchicalResponseListByResponseId
        /// </summary>
        /// <param name="responseId"></param>
        /// <param name="includeDeletedRecords"></param>
        /// <param name="excludeInProcessRecords"></param>
        /// <returns></returns>
        /// <remarks> Used by the DataConsisitencyServiceAPI</remarks>
        public List<FormResponseProperties> GetHierarchicalResponseListByResponseId(IResponseContext responseContext, bool includeDeletedRecords = false, bool excludeInProcessRecords = false)
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

        #endregion Get hierarchical responses by ResponseId

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
                        : WHERE + AssembleWhere(rootFormName, Expression(FRP_RecStatus, NE, RecordStatus.Deleted)))
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
                                                And_Expression(FRP_RecStatus, NE, RecordStatus.Deleted, includeDeleted))
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
            FormResponseResource.ChildResponseContext responseDirectory = null;
            if (formResponseResource.ChildResponseContexts.TryGetValue(responseContext.ResponseId, out responseDirectory))
            {
                responseContext.FormId = responseDirectory.FormId;
                responseContext.FormName = responseDirectory.FormName;
                responseContext.ParentFormId = responseDirectory.ParentFormId;
                responseContext.ParentFormName = responseDirectory.ParentFormName;
                responseContext.ParentResponseId = responseDirectory.ParentResponseId;
            }
        }

        private List<FormResponseProperties> ReadAllRootResponses(IResponseContext responseContext, List<string> columnlist, KeyValuePair<FieldDigest, string>[] searchQualifiers = null, int pageSize = 0, int pageNumber = 0, bool includeChildren = false)
        {
            try
            {
                var rootFormName = responseContext.RootFormName ?? this.GetRootFormName(responseContext.RootFormId);
                string query;
                var collectionAlias = rootFormName;
                var rootFormCollectionUri = GetCollectionUri(rootFormName);
                var nonWildSearchQualifiers = searchQualifiers != null
                    ? searchQualifiers.Where(q => !(q.Value.Contains('*') || q.Value.Contains('?'))).ToArray()
                    : null;
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                List<string> formProperties = new List<string>
                {
                    "FormId","FirstSaveTime","LastSaveTime","ResponseId","UserName","IsDraftMode"
                };
                if (nonWildSearchQualifiers != null)
                {
                    query = SearchByFieldNames(collectionAlias, responseContext.FormId, formProperties, searchQualifiers);
                }
                else
                {
                    query = GetAllRecordByFormId(collectionAlias, responseContext.FormId, formProperties, columnlist);
                }
                var formResponsePropertiesList = GetAllRecordsBySurveyId(rootFormName, spGetRecordsBySurveyId, udfWildCardCompare, query).Result;


                //var response = FilterQueryResponseByWildCardQualifiers(formResponsePropertiesList, searchQualifiers);
                return formResponsePropertiesList;
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
                    //var search = SearchByFiledNames(collectionAlias, responseContext.FormId, formProperties, columnlist);

                    //foreach (var formReponseProperties in enumerableFormResponseProperties)
                    //{
                    //    bool isQuailfied = true;
                    //    foreach (var searchQuery in wildQualifiers)
                    //    {
                    //        var fieldName = searchQuery.Key.FieldName;
                    //        var searchValue = searchQuery.Value;
                    //        string responseValue;
                    //        bool responseExists = formReponseProperties.ResponseQA.TryGetValue(fieldName, out responseValue);
                    //        if (!responseValue.WildcardCompare(searchValue))
                    //        {
                    //            isQuailfied = false;
                    //            break;
                    //        }
                    //    }
                    //    if (isQuailfied) filteredFormResponsePropertiesList.Add(formReponseProperties);
                    //}
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
