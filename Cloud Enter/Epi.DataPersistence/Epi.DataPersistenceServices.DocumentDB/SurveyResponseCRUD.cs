#define ConfigureIndexing

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epi.Cloud.Common.Constants;
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
        private string DatabaseName;
        private string AttachmentId = ConfigurationManager.AppSettings[AppSettings.Key.AttachmentId];
        private const string FormInfoCollectionName = "FormInfo";

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
        

        #region UpdateAttachment
        public async Task<bool> UpdateAttachment(string responseId, int responseStatus, int userId = 0, int newResponseStatus = 0)
        {
            newResponseStatus = responseStatus;
            Attachment attachment = null;
            bool isSuccessful = false;
            bool deleteResponse = false;
            try
            {
                Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
                var formResponseProperties = ReadFormInfoByResponseId(responseId, formInfoCollectionUri);
                if (formResponseProperties != null)
                {
                    //Is status is Saved and check if attachment is existed or not.If attachment is null and delete attachment
                    if (newResponseStatus == RecordStatus.Saved)
                    {
                        attachment = ReadAttachment(responseId, AttachmentId);
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
                                var formResponseSave = await Client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
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

        

        #region NewRecordDontSave
        public bool DeleteAllSurveyData(string responseId, int responseStatus, int userId = 0, int newResponseStatus = 0)
        {
            newResponseStatus = responseStatus;
            bool isSuccessful = false;
            try
            {
                Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
                var existingFormResponseProperties = ReadFormInfoByResponseId(responseId, formInfoCollectionUri);
                if (existingFormResponseProperties != null)
                {
                    if (newResponseStatus != existingFormResponseProperties.RecStatus)
                    {
                        //Read all survey info by responseId
                        var hierarchialResponse = GetHierarchialResponsesByResponseId(existingFormResponseProperties.GlobalRecordID, true);

                        //Delete FormInfo
                        if (hierarchialResponse.FormResponseProperties != null)
                        {
                            if (hierarchialResponse.FormResponseProperties.GlobalRecordID == existingFormResponseProperties.GlobalRecordID)
                            {
                                existingFormResponseProperties.RecStatus = RecordStatus.Deleted;
                                //var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, FormInfoCollectionName, existingFormResponseProperties.GlobalRecordID);
                                var response = Client.UpsertDocumentAsync(formInfoCollectionUri, existingFormResponseProperties, null).ConfigureAwait(false);

                                //var response = Client.DeleteDocumentAsync(docLink).Result;
                            }
                        }
                        ////Delete Parent
                        //if (hierarchialResponse.PageResponsePropertiesList != null)
                        //{
                        //    foreach (var pageAttachmentResponseProperties in hierarchialResponse.PageResponsePropertiesList)
                        //    {
                        //        var collectionName = hierarchialResponse.FormResponseProperties.FormName + pageAttachmentResponseProperties.PageId;
                        //        try
                        //        {
                        //            var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, collectionName, existingFormResponseProperties.GlobalRecordID);
                        //            var response = Client.DeleteDocumentAsync(docLink).Result;
                        //        }
                        //        catch (Exception ex)
                        //        {

                        //        }
                        //    }
                        //}
                        ////Delete Child

                        //if (hierarchialResponse.ChildResponseList != null && hierarchialResponse.ChildResponseList.Count > 0)
                        //{
                        //    foreach (var pageAttachmentResponseProperties in hierarchialResponse.ChildResponseList)
                        //    {
                        //        DeleteChildOfChilds(hierarchialResponse);
                        //    }
                        //}

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

        #region Restore Child of Child
        public async Task<bool> RestoreAttachment(HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties, FormResponseProperties existingFormResponseProperties)
        {
            bool isSuccessful = false;
            if (hierarchicalDocumentResponseProperties.PageResponsePropertiesList.Count!= 0)
            {
                Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
                existingFormResponseProperties = ReadFormInfoByResponseId(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, formInfoCollectionUri);

                //Restore Attachment to FormInfo
                if (hierarchicalDocumentResponseProperties.FormResponseProperties != null)
                {
                    if (existingFormResponseProperties.GlobalRecordID == existingFormResponseProperties.GlobalRecordID)
                    {
                        existingFormResponseProperties.RecStatus = RecordStatus.Saved;
                        //var formResponse = Client.UpsertDocumentAsync(formInfoCollectionUri, hierarchicalDocumentResponseProperties.FormResponseProperties).Result;
                        var formResponse = await Client.UpsertDocumentAsync(formInfoCollectionUri, hierarchicalDocumentResponseProperties.FormResponseProperties).ConfigureAwait(false);
                    }
                }
                if (hierarchicalDocumentResponseProperties.PageResponsePropertiesList != null)
                {
                    foreach (var pageAttachmentResponseProperties in hierarchicalDocumentResponseProperties.PageResponsePropertiesList)
                    {
                        var collectionName = hierarchicalDocumentResponseProperties.FormResponseProperties.FormName + pageAttachmentResponseProperties.PageId;
                        Uri pageCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                        try
                        {
                            var ParentResponse = await Client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

            }
            //if (hierarchicalDocumentResponseProperties.ChildResponseList.Count > 0)
            //{
            //    HierarchicalDocumentResponseProperties _hierarchicalDocumentResponseProperties = new HierarchicalDocumentResponseProperties();
            //    _hierarchicalDocumentResponseProperties.ChildResponseList = hierarchicalDocumentResponseProperties.ChildResponseList;
            //    var result = RestoreAttachment(hierarchicalDocumentResponseProperties, existingFormResponseProperties);
            //}
            return isSuccessful;
        }


        #endregion

        #region Delete Child of Child
        public void DeleteChildOfChilds(HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties)
        {
            //Delete FormInfo   
            foreach (var child in hierarchicalDocumentResponseProperties.ChildResponseList)
            {

                foreach (var _page in child.FormResponseProperties.PageIds)
                {
                    //Delete document in Document DB Child collection
                    var collectionName = child.FormResponseProperties.FormName + _page;
                    try
                    {
                        var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, collectionName, child.FormResponseProperties.GlobalRecordID);
                        var response = Client.DeleteDocumentAsync(docLink).Result;
                    }
                    catch (Exception ex)
                    {

                    }
                    //Delete document in Document DB FormInfo collection
                    collectionName = child.FormResponseProperties.FormName + _page;
                    try
                    {
                        var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, FormInfoCollectionName, child.FormResponseProperties.GlobalRecordID);
                        var response = Client.DeleteDocumentAsync(docLink).Result;
                    }
                    catch (Exception ex)
                    {

                    }

                }

                if (child.ChildResponseList.Count > 0)
                {
                    DeleteChildOfChilds(child);
                }
            }
        }

        #endregion

        #region Restore Child of Child
        public async void RestoreChildOfChilds(HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties)
        {
            //Delete FormInfo   
            foreach (var child in hierarchicalDocumentResponseProperties.ChildResponseList)
            {

                foreach (var _page in child.FormResponseProperties.PageIds)
                {
                    //Delete document in Document DB Child collection
                    var collectionName = child.FormResponseProperties.FormName + _page;
                    try
                    {
                        Uri pageCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                        var forminfoResponse = await Client.UpsertDocumentAsync(pageCollectionUri, child);                       
                    }
                    catch (Exception ex)
                    {

                    }
                    ////Delete document in Document DB FormInfo collection
                    //collectionName = child.FormResponseProperties.FormName + _page;
                    //try
                    //{
                    //    var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, FormInfoCollectionName, child.FormResponseProperties.GlobalRecordID);
                    //    var response = Client.DeleteDocumentAsync(docLink).Result;
                    ////}
                    //catch (Exception ex)
                    //{

                    //}

                }

                if (child.ChildResponseList.Count > 0)
                {
                    RestoreChildOfChilds(child);
                }
            }
        }

        #endregion

        #region Save Page Response Properties Async
        /// <summary>
        /// Created instance of DocumentClient and Getting reference to database and Document collections
        /// </summary>
        public async Task<bool> SavePageResponsePropertiesAsync(DocumentResponseProperties documentResponseProperties)
        {
            bool tasksRanToCompletion = false;
            try
            {
                var numberOfPages = documentResponseProperties.PageResponsePropertiesList.Count();
                if (numberOfPages == 0) return true;

                using (BlockingCollection<Task<ResourceResponse<Document>>> pendingTasksBC = new BlockingCollection<Task<ResourceResponse<Document>>>(numberOfPages))
                {
                    int numberOfPagesInitiated = 0;
                    int numberOfPagesCompleted = 0;

                    var globalRecordId = documentResponseProperties.FormResponseProperties.GlobalRecordID;

                    foreach (var newPageResponseProperties in documentResponseProperties.PageResponsePropertiesList)
                    {
                        bool isUpdated = false;
                        var pageId = newPageResponseProperties.PageId;
                        Uri pageCollectionUri = GetCollectionUri(newPageResponseProperties.ToColectionName(documentResponseProperties.FormResponseProperties.FormName));
                        var pageResponseProperties = ReadPageResponsePropertiesByResponseId(globalRecordId, pageCollectionUri);
                        if (pageResponseProperties != null)
                        {
                            foreach (var newFieldKvp in newPageResponseProperties.ResponseQA)
                            {
                                if (newFieldKvp.Value != null)
                                {
                                    var responseQA = pageResponseProperties.ResponseQA;
                                    string existingValue;
                                    var existingFieldExists = responseQA.TryGetValue(newFieldKvp.Key, out existingValue);
                                    {
                                        if (!existingFieldExists || existingValue != newFieldKvp.Value)
                                        {
                                            responseQA[newFieldKvp.Key] = newFieldKvp.Value;
                                            isUpdated = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            pageResponseProperties = newPageResponseProperties;
                            isUpdated = true;
                        }
                        if (isUpdated)
                        {
                            var task = Client.UpsertDocumentAsync(pageCollectionUri, pageResponseProperties);
                            var success = pendingTasksBC.TryAdd(task, 100);
                            if (success)
                            {
                                Interlocked.Increment(ref numberOfPagesInitiated);
                            }
                            else
                            {
                                throw new TimeoutException("Unable to add task");
                            }
                        }
                        else
                        {
                            Interlocked.Decrement(ref numberOfPages); 
                        }
                    }

                    pendingTasksBC.CompleteAdding();

                    while (numberOfPages != numberOfPagesCompleted)
                    {
                        Task<ResourceResponse<Document>> task;
                        if (pendingTasksBC.TryTake(out task, 100))
                        {
                            Interlocked.Increment(ref numberOfPagesCompleted);
                            var result = await task.ConfigureAwait(false);
                        }
                        else
                        {
                            throw new TimeoutException("Unable to take task");
                        }
                    }

                    tasksRanToCompletion = true;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return await Task.FromResult(tasksRanToCompletion);
        }

        #endregion

        #region Save Form Response Properties Async

        /// <summary>
        /// This method help to save form properties 
        /// and also used for delete operation.Ex:RecStatus=0
        /// </summary>
        /// <param name="formResponseProperties"></param>
        /// <returns></returns>
        public async Task<ResourceResponse<Document>> SaveFormResponsePropertiesAsync(FormResponseProperties formResponseProperties)
		{
            ResourceResponse<Document> result = null;
            try
            {
                //Instance of DocumentClient"
                var formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);

                //Verify Response Id is exist or Not
                var existingFormResponseProperties = ReadFormInfoByResponseId(formResponseProperties.GlobalRecordID, formInfoCollectionUri);
                if (existingFormResponseProperties == null)
                {
                    formResponseProperties.FirstSaveTime = DateTime.UtcNow;
                    formResponseProperties.Id = formResponseProperties.GlobalRecordID;
                    formResponseProperties.PageIds = formResponseProperties.PageIds ?? new List<int>();
                    result = await Client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties).ConfigureAwait(false);
                }
                else
                {
                    formResponseProperties.FirstSaveTime = existingFormResponseProperties.FirstSaveTime;
                    //Create attachment
                    if (existingFormResponseProperties.IsRootForm && existingFormResponseProperties.RecStatus==RecordStatus.Saved && formResponseProperties.RecStatus == RecordStatus.InProcess)
                    {
                        Attachment attachment = null;
                        var hierarchialResponse = GetHierarchialResponsesByResponseId(existingFormResponseProperties.GlobalRecordID, true);
                        var hierarchialResponseJson = JsonConvert.SerializeObject(hierarchialResponse);
                        attachment = CreateAttachment(existingFormResponseProperties.SelfLink, AttachmentId, existingFormResponseProperties.GlobalRecordID, hierarchialResponseJson);

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

                    var isUpdated = pageIdsUpdated
                        || existingFormResponseProperties.LastSaveTime != formResponseProperties.LastSaveTime
                        || existingFormResponseProperties.RecStatus != formResponseProperties.RecStatus
                        || existingFormResponseProperties.HiddenFieldsList != formResponseProperties.HiddenFieldsList
                        || existingFormResponseProperties.DisabledFieldsList != formResponseProperties.DisabledFieldsList
                        || existingFormResponseProperties.HighlightedFieldsList != formResponseProperties.HighlightedFieldsList
                        || existingFormResponseProperties.RequiredFieldsList != formResponseProperties.RequiredFieldsList;

                    if (isUpdated)
                    {
                        result = await Client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties, null).ConfigureAwait(false);
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

		#region Get PageResponseProperties By ResponseId, FormId, and PageId
		public PageResponseProperties GetPageResponsePropertiesByResponseId(string responseId, string formName, int pageId)
		{
			PageResponseProperties pageResponseProperties = null;
			try
			{
				//Instance of DocumentClient"
					var collectionName = formName + pageId;
					Uri pageCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);

					//Read collection and store data  
					pageResponseProperties = ReadPageResponsePropertiesByResponseId(responseId, pageCollectionUri);
					return pageResponseProperties;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return null;
		}

#endregion Get PageResponseProperties By ResponseId, FormId, and PageId


		#region Get All Responses With FieldNames 
		public List<SurveyResponse> GetAllResponsesWithCriteria(IDictionary<int, FieldDigest> fields, IDictionary<int, KeyValuePair<FieldDigest, string>> searchFields, string relateParentId = null, int pageSize = 0, int pageNumber = 0)
		{
			 
			List<SurveyResponse> surveyResponse = null;

            try
            {
                var surveyResponses = ReadAllResponsesWithCriteria(fields, searchFields, relateParentId, pageSize, pageNumber);
                return surveyResponses;
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
			return surveyResponse;
		}

		private List<SurveyResponse> ReadAllResponsesWithCriteria(IDictionary<int, FieldDigest> fieldDigestList, IDictionary<int, KeyValuePair<FieldDigest, string>> searchFields, string relateParentId, int pageSize = 0, int pageNumber = 0)
		{
			// Set some common query options
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
			List<SurveyResponse> surveyList = new List<SurveyResponse>();
            int FailedSearchStatus = int.MaxValue;
            int WentThroughSearch = int.MaxValue - 1;


            try
			{
                // Build a composit of both the grid display field digests and the search field digests
                var compositeFieldDigestList = fieldDigestList.Values.Select(x => x).Union(searchFields.Values.Select(x => x.Key)).Distinct();

				// One SurveyResponse per GlobalRecordId
				Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

				List<FormResponseProperties> parentGlobalIdList = ReadAllResponsesByRelateParentResponseId(relateParentId, fieldDigestList.FirstOrDefault().Value.FormId, pageSize, pageNumber);
				string parentGlobalRecordIds = string.Empty;

				if (parentGlobalIdList != null)
				{
					parentGlobalRecordIds = "'" + string.Join("','", parentGlobalIdList.Select(p => p.GlobalRecordID)) + "'";
				}

				// Query DocumentDB one page at a time. Only query pages that contain a specified field.
                var pageGroups = compositeFieldDigestList.GroupBy(d => d.PageId);
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
					Uri pageCollectionUri = GetCollectionUri(collectionName);
					var pageQuery = Client.CreateDocumentQuery(pageCollectionUri,
						SELECT + columnList
						+ FROM + collectionName
						+ WHERE + collectionName + ".GlobalRecordID in (" + parentGlobalRecordIds + ")", queryOptions);

                    var searchQueries = searchFields.Values.Where(kvp => kvp.Key.PageId == pageId).ToArray();

                    foreach (var items in pageQuery.AsQueryable())
					{
                        bool wentThroughSearch = false;
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

                        // If this particular record has already failed the search criteria
                        // then we can skip additional tests.
                        if (formResponseDetail.RecStatus != FailedSearchStatus)
                        {
                            Dictionary<string, string> aggregatePageResponseQA;
                            PageResponseDetail pageResponseDetail = formResponseDetail.PageResponseDetailList.SingleOrDefault(p => p.PageId == pageId);
                            if (pageResponseDetail == null)
                            {
                                pageResponseDetail = new PageResponseDetail { FormId = formId, FormName = formName, PageId = pageId };
                                formResponseDetail.AddPageResponseDetail(pageResponseDetail);
                            }

                            foreach (var searchQuery in searchQueries)
                            {
                                if (formResponseDetail.RecStatus != FailedSearchStatus) formResponseDetail.RecStatus = WentThroughSearch;
                                var fieldName = searchQuery.Key.FieldName;
                                var searchValue = searchQuery.Value;
                                string responseValue;
                                bool responseExists = pageResponseQA.TryGetValue(fieldName, out responseValue);
                                if ((!responseExists && searchValue != null) || !String.Equals(responseValue, searchValue, StringComparison.OrdinalIgnoreCase))
                                {
                                    formResponseDetail.RecStatus = FailedSearchStatus;
                                    break;
                                }
                                else
                                {
                                }
                            }

                            // If the current record just failed the search criteria 
                            // then we can skip the remaining code.
                            if (formResponseDetail.RecStatus != FailedSearchStatus)
                            {
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
					}
				}

                if (searchFields.Count > 0)
                {
                    surveyList = responsesByGlobalRecordId.Values
                        .Where(s => s.ResponseDetail.RecStatus == WentThroughSearch)
                        .OrderByDescending(v => v.Timestamp).ToList();
                }
                else
                {
                    surveyList = responsesByGlobalRecordId.Values
                        .OrderByDescending(v => v.Timestamp).ToList();
                }
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
			Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
			var formResponseProperties = ReadFormPropertiesByResponseId(formInfoCollectionUri, responseId);
			documentResponseProperties.FormResponseProperties = formResponseProperties;
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
            Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
            var formResponseProperties = ReadFormPropertiesByResponseId(formInfoCollectionUri, responseId);
            documentResponseProperties.FormResponseProperties = formResponseProperties;

            if (formResponseProperties != null)
            {
                foreach (var pageId in formResponseProperties.PageIds)
                {
                    if (pageId != 0)
                    {
                        string collectionName = formResponseProperties.FormName + pageId;
                        var pageCollectionUri = GetCollectionUri(collectionName);
                        var pageResponseProperties = ReadPageResponsePropertiesByResponseId(responseId, pageCollectionUri);
                        documentResponseProperties.PageResponsePropertiesList.Add(pageResponseProperties);
                    }
                }
            }
            return documentResponseProperties;
        }
        #endregion Get all page responses by ResponseId

        #region Get hierarchial responses by ResponseId
        /// <summary>
        /// GetHierarchialResponsesByResponseId
        /// </summary>
        /// <param name="responseId"></param>
        /// <param name="includeDeletedRecords"></param>
        /// <returns></returns>
        /// <remarks> Used by the DataConsisitencyServiceAPI</remarks>
        public HierarchicalDocumentResponseProperties GetHierarchialResponsesByResponseId(string responseId, bool includeDeletedRecords = false, bool excludeInProcessRecords = false)
        {
            HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = new HierarchicalDocumentResponseProperties();
            Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
            var documentResponseProperties = ReadAllResponsesByExpression(Expression("GlobalRecordID", EQ, responseId), formInfoCollectionUri, includeDeletedRecords).SingleOrDefault();
            if (documentResponseProperties != null)
            {
                hierarchicalDocumentResponseProperties.FormResponseProperties = documentResponseProperties.FormResponseProperties;
                hierarchicalDocumentResponseProperties.PageResponsePropertiesList = documentResponseProperties.PageResponsePropertiesList;
                hierarchicalDocumentResponseProperties.ChildResponseList = GetChildResponses(responseId, formInfoCollectionUri);
            }
            return hierarchicalDocumentResponseProperties;
        }

		private List<HierarchicalDocumentResponseProperties> GetChildResponses(string parentResponseId, Uri formInfoCollectionUri, bool includeDeletedRecords = false)
		{
			var childResponseList = new List<HierarchicalDocumentResponseProperties>();
			var documentResponsePropertiesList = ReadAllResponsesByExpression(Expression("RelateParentId", EQ, parentResponseId), formInfoCollectionUri, includeDeletedRecords);
			foreach (var documentResponseProperties in documentResponsePropertiesList)
			{
				var childResponse = new HierarchicalDocumentResponseProperties();
				childResponse.FormResponseProperties = documentResponseProperties.FormResponseProperties;
				childResponse.PageResponsePropertiesList = documentResponseProperties.PageResponsePropertiesList;
				childResponseList.Add(childResponse);

				childResponse.ChildResponseList = GetChildResponses(documentResponseProperties.FormResponseProperties.GlobalRecordID, formInfoCollectionUri, includeDeletedRecords);
			}
			return childResponseList;
		}

		private List<DocumentResponseProperties> ReadAllResponsesByExpression(string expression, Uri formInfoCollectionUri, bool includeDeletedRecords, string collectionAlias = "c")
		{
			var documentResponsePropertiesList = new List<DocumentResponseProperties>();
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
			try
			{
				var query = Client.CreateDocumentQuery(formInfoCollectionUri,
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
						documentResponseProperties.PageResponsePropertiesList = ReadAllPages(formResponseProperties);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return documentResponsePropertiesList;
		}

		private List<PageResponseProperties> ReadAllPages(FormResponseProperties formResponseProperties)
		{
			List<PageResponseProperties> pageResponsePropertiesList = new List<PageResponseProperties>();
			var responseId = formResponseProperties.GlobalRecordID;
			foreach (var pageId in formResponseProperties.PageIds)
			{
				if (pageId != 0)
				{
					string collectionName = formResponseProperties.FormName + pageId;
					var pageCollectionUri = GetCollectionUri(collectionName);
					var pageResponseProperties = ReadPageResponsePropertiesByResponseId(responseId, pageCollectionUri);
					if (pageResponseProperties != null)
					{
						pageResponsePropertiesList.Add(pageResponseProperties);
					}
				}
			}

			return pageResponsePropertiesList;
		}

        private List<DocumentResponseProperties> ReadAllResponsesIdsByExpression(string expression, Uri formInfoCollectionUri, bool includeDeletedRecords, string collectionAlias = "c")
        {
            var documentResponsePropertiesList = new List<DocumentResponseProperties>();
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = Client.CreateDocumentQuery(formInfoCollectionUri,
                    SELECT + AssembleSelect(collectionAlias, "*")
                    + FROM + collectionAlias
                    + WHERE + AssembleWhere(collectionAlias, expression, And_Expression("RecStatus", NE, RecordStatus.Deleted, includeDeletedRecords))
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

        private PageResponseProperties ReadPageResponsePropertiesByResponseId(string responseId, Uri pageCollectionUri, string collectionAlias = "c")
		{
			PageResponseProperties pageResponseProperties = null;
			// Set some common query options
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
			try
			{
				var query = Client.CreateDocumentQuery(pageCollectionUri,
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

        #endregion Get hierarchial responses by ResponseId

        private List<SurveyResponse> GetAllDataByChildFormIdByRelateId(string formId, string relateParentId, Dictionary<int, FieldDigest> fieldDigestList, string collectionName)
		{
			// Set some common query options
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
			List<SurveyResponse> surveyList = new List<SurveyResponse>();
			try
			{
				// One SurveyResponse per GlobalRecordId
				Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

				List<FormResponseProperties> childResponses = ReadAllResponsesByRelateParentResponseId(relateParentId, formId);
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

					var pageQuery = Client.CreateDocumentQuery(docUri, SELECT + columnList + " FROM  " + collectionName + pageId + WHERE + collectionName + pageId + ".GlobalRecordID in ( " + childCSVResponseIds + ")", queryOptions);

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

        #region Does response exist
        /// <summary>
        /// 
        /// </summary>
        /// <param name="childFormId"></param>
        /// <param name="parentResponseId"></param>
        /// <returns></returns>
        public bool DoesResponseExist(string childFormId, string parentResponseId)
        {
			Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
			var count = CountResponses(parentResponseId, childFormId, formInfoCollectionUri);
			return count != 0;
        }

        private int CountResponses(string parentResponseId, string childFormId, Uri formInfoCollectionUri)
        {
            var collectionAlias = FormInfoCollectionName;

            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var query = Client.CreateDocumentQuery(formInfoCollectionUri,
                    SELECT
                    + AssembleSelect(collectionAlias, "GlobalRecordID")
                    + FROM + collectionAlias
                    + WHERE
                    + AssembleWhere(collectionAlias, Expression("FormId", EQ, childFormId),
                                                And_Expression("RelateParentId", EQ, parentResponseId),
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
        #endregion Does response exist


        #region Do children exist for responseId
        /// <summary>
        /// DoChildrenExistForResponseId
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        public bool DoChildrenExistForResponseId(string responseId)
        {
            Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
            var count = CountRelatedResponses(responseId, formInfoCollectionUri);
            return count != 0;
        }

		private int CountRelatedResponses(string responseId, Uri formInfoCollectionUri)
		{
			var collectionAlias = FormInfoCollectionName;

			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = Client.CreateDocumentQuery(formInfoCollectionUri,
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

		private FormResponseProperties ReadFormInfoByResponseId(string responseId, Uri formInfoCollectionUri)
		{
			var collectionAlias = FormInfoCollectionName;

			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = Client.CreateDocumentQuery(formInfoCollectionUri,
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
		    Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
		    var formResponseCount = GetFormResponseCount(formInfoCollectionUri, formId, includeDeletedRecords);
		    return formResponseCount;
		}

		private int GetFormResponseCount(Uri formInfoCollectionUri, string formId, bool includeDeletedRecords)
		{
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = Client.CreateDocumentQuery(formInfoCollectionUri,
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
		///	GetFormResponseState
		/// </summary>
		/// <param name="responseId"></param>
		/// <returns></returns>
		public FormResponseProperties GetFormResponseState(string responseId)
		{
			var formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
			var formResponseProperties = ReadFormResponseState(responseId, formInfoCollectionUri);
			return formResponseProperties;
		}

		private FormResponseProperties ReadFormResponseState(string responseId, Uri formInfoCollectionUri)
		{
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = Client.CreateDocumentQuery(formInfoCollectionUri,
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
		private List<FormResponseProperties> ReadAllResponsesByRelateParentResponseId(string relateParentId, string formId,int pageSize=0,int pageNumber=0)
		{
			try
			{
				List<FormResponseProperties> globalRecordIdList = new List<FormResponseProperties>();

				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = 100 };
				IQueryable<dynamic> query;

				Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
				bool skipAnd = formId == null;
				if (relateParentId != null)
				{
					query = Client.CreateDocumentQuery(formInfoCollectionUri,
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
					query = Client.CreateDocumentQuery(formInfoCollectionUri,
					   SELECT 
					   + AssembleSelect(FormInfoCollectionName, "*")
					   + FROM + FormInfoCollectionName
					   + WHERE
					   + AssembleWhere(FormInfoCollectionName, Expression("RecStatus", NE, 0),
															And_Expression("FormId", EQ, formId, skipAnd))
					   , queryOptions);
				}
                if(pageSize==0 && pageNumber==0)
                {
                    globalRecordIdList = query.AsEnumerable<dynamic>().Select(x => (FormResponseProperties)x).ToList();
                }
                else
                {
                    //globalRecordIdList = query.AsEnumerable<dynamic>().Skip(pageNumber).Take(pageSize).OrderByDescending(x => x._ts).Select(x => (FormResponseProperties)x).ToList();
                    globalRecordIdList = query.AsEnumerable<dynamic>().Select(x => (FormResponseProperties)x).ToList();
                    //globalRecordIdList = query.AsEnumerable<dynamic>().Skip(pageNumber).Take(pageSize).Select(x => (FormResponseProperties)x).ToList();
                }
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
		private FormResponseProperties ReadFormPropertiesByResponseId(Uri formInfoCollectionUri, string responseId)
		{
			// tell server we only want 25 record
			FeedOptions options = new FeedOptions { MaxItemCount = 25, EnableCrossPartitionQuery = true };
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = Client.CreateDocumentQuery(formInfoCollectionUri,
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
