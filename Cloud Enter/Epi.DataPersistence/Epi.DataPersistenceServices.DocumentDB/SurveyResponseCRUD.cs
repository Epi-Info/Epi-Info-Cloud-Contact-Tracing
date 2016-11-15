#define ConfigureIndexing

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
                        // formResponseProperties.RecStatus = responseStatus;
                        if (attachment != null)
                        {
                            deleteResponse = DeleteAttachment(attachment);
                        }
                    }

                    if (newResponseStatus != formResponseProperties.RecStatus)
                    {
                        FormResponseProperties existingFormResponseProperties = null;
                        switch (newResponseStatus)
                        {
                            case RecordStatus.Saved:
                                formResponseProperties.IsNewRecord = false;
                                formResponseProperties.RecStatus = RecordStatus.Saved;
                                var formResponseSave = await Client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
                                break;
                            case RecordStatus.Deleted:
                                existingFormResponseProperties = ReadFormInfoByResponseId(responseId, formInfoCollectionUri);

                                //Restore Attachment to FormInfo
                                if (existingFormResponseProperties != null)
                                {
                                    if (existingFormResponseProperties.GlobalRecordID == formResponseProperties.GlobalRecordID)
                                    {
                                        existingFormResponseProperties.IsNewRecord = false;
                                        existingFormResponseProperties.RecStatus = RecordStatus.Deleted;
                                        var formResponse = await Client.UpsertDocumentAsync(formInfoCollectionUri, existingFormResponseProperties);
                                    }
                                }
                                break;
                            case RecordStatus.Restore:
                                attachment = ReadAttachment(responseId, AttachmentId);
                                if (attachment == null)
                                {
                                    //Delete FormInfo
                                    var hierarchialResponse = GetHierarchialResponsesByResponseId(formResponseProperties.GlobalRecordID, true);

                                    //Delete FormInfo
                                    if (hierarchialResponse.FormResponseProperties != null)
                                    {
                                        if (hierarchialResponse.FormResponseProperties.GlobalRecordID == formResponseProperties.GlobalRecordID)
                                        {
                                            var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, FormInfoCollectionName, formResponseProperties.GlobalRecordID);
                                            var response = Client.DeleteDocumentAsync(docLink).Result;
                                        }
                                    }
                                    //Delete Parent
                                    if (hierarchialResponse.PageResponsePropertiesList != null)
                                    {
                                        foreach (var pageAttachmentResponseProperties in hierarchialResponse.PageResponsePropertiesList)
                                        {
                                            var collectionName = hierarchialResponse.FormResponseProperties.FormName + pageAttachmentResponseProperties.PageId;
                                            try
                                            {
                                                var docLink = string.Format("dbs/{0}/colls/{1}/docs/{2}", DatabaseName, collectionName, formResponseProperties.GlobalRecordID);
                                                var response = Client.DeleteDocumentAsync(docLink).Result;
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                    }
                                    //Delete Child

                                    if (hierarchialResponse.ChildResponseList != null && hierarchialResponse.ChildResponseList.Count > 0)
                                    {
                                        foreach (var pageAttachmentResponseProperties in hierarchialResponse.ChildResponseList)
                                        {
                                            DeleteChildOfChilds(hierarchialResponse);
                                        }
                                    }

                                }
                                else
                                {
                                    HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = ConvertAttachmentToHierarchical(attachment);
                                    existingFormResponseProperties = ReadFormInfoByResponseId(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, formInfoCollectionUri);

                                    //Restore Attachment to FormInfo
                                    if (hierarchicalDocumentResponseProperties.FormResponseProperties != null)
                                    {
                                        if (existingFormResponseProperties.GlobalRecordID == formResponseProperties.GlobalRecordID)
                                        {
                                            formResponseProperties.RecStatus = RecordStatus.Saved;
                                            var formResponse = await Client.UpsertDocumentAsync(formInfoCollectionUri, hierarchicalDocumentResponseProperties.FormResponseProperties);
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
                                                var ParentResponse = await Client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties);
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
                                            var forminfoResponse = await Client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties);
                                        }
                                    }
                                    //Delete new survey data in Document DB
                                    if (hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds != null && hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds.Count > 0)
                                    {
                                        var RemovePages = existingFormResponseProperties.PageIds.Except(hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds).ToList();
                                        if (RemovePages.Count > 0)
                                        {
                                            deleteResponse = DeleteSurveyDataInDocumentDB(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, hierarchicalDocumentResponseProperties.FormResponseProperties.FormName, RemovePages);
                                        }
                                    }

                                    //Delete Attachment
                                    Uri collectionUri = UriFactory.CreateAttachmentUri(DatabaseName, FormInfoCollectionName, hierarchicalDocumentResponseProperties.FormResponseProperties.Id, AttachmentId);
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
			return isSuccessful;
		}




        #region DeleteChilds
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


        #region Attachment
        public Attachment GetAttachmentInfo(string globalRecordID, string attachmentId, string documentSelfLink, string hierarchialResponseJson)
        {

            Attachment attachment = null;
            //Check Attachment is exist or not 
            attachment = ReadAttachment(globalRecordID, attachmentId);
            //if (attachment == null && hierarchialResponseJson != null)
            //{
            //    attachment = CreateAttachment(documentSelfLink, attachmentId, globalRecordID, hierarchialResponseJson);
            //}
            return attachment;
        }
        #endregion
        public async Task<bool> RestoreSurveyDataFromAttachment(Attachment attachmentInfo, string attachmentId, FormResponseProperties existingFormResponseProperties)
        {

            Attachment attachment = null;
            bool tasksRanToCompletion = false;
            bool deleteResponse = false;
            bool isSuccessful = false;
            try
            {
                var formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
                var formInfoResponse = await Client.UpsertDocumentAsync(formInfoCollectionUri, existingFormResponseProperties);
                HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = ConvertAttachmentToHierarchical(attachmentInfo);
                //Restore Attachment to FormInfo
                if (hierarchicalDocumentResponseProperties.FormResponseProperties != null)
                {
                    var existingFormRespodgnseProperties = ReadFormInfoByResponseId(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, formInfoCollectionUri);

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
                            var ParentResponse = Client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties).Result;
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
                        Client.UpsertDocumentAsync(pageCollectionUri, pageAttachmentResponseProperties);
                    }
                }
                //Delete new survey data
                if (hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds != null)
                {
                    var newFormResponseProperties = ReadFormInfoByResponseId(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, formInfoCollectionUri);
                    hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds = hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds.Except(newFormResponseProperties.PageIds).ToList();
                    deleteResponse = DeleteSurveyDataInDocumentDB(hierarchicalDocumentResponseProperties.FormResponseProperties.GlobalRecordID, hierarchicalDocumentResponseProperties.FormResponseProperties.FormName, hierarchicalDocumentResponseProperties.FormResponseProperties.PageIds);
                }
                if (deleteResponse)
                {
                    Uri collectionUri = UriFactory.CreateAttachmentUri(DatabaseName, FormInfoCollectionName, hierarchicalDocumentResponseProperties.FormResponseProperties.Id, attachmentId);
                    deleteResponse = DeleteAttachment(attachment);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return tasksRanToCompletion;
        }
        #region InsertToSurveyToDocumentDB
        /// <summary>
        /// Created instance of DocumentClient and Getting reference to database and Document collections
        /// </summary>
        public async Task<bool> InsertResponseAsync(DocumentResponseProperties documentResponseProperties)
        {
            bool tasksRanToCompletion = false;
            Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);

            var formResponseProperties = ReadFormInfoByResponseId(documentResponseProperties.GlobalRecordID, formInfoCollectionUri);

            foreach (var newPageResponseProperties in documentResponseProperties.PageResponsePropertiesList)
            {
                var pageId = newPageResponseProperties.PageId;
                Uri pageCollectionUri = GetCollectionUri(newPageResponseProperties.ToColectionName(documentResponseProperties.FormResponseProperties.FormName));
                var pageResponse = await Client.UpsertDocumentAsync(pageCollectionUri, newPageResponseProperties);

                if (!formResponseProperties.PageIds.Contains(pageId))
                {
                    formResponseProperties.PageIds.Add(pageId);
                }
            }

            formResponseProperties.PageIds.Sort();

            var formResponse = await Client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
            tasksRanToCompletion = true;
            return tasksRanToCompletion;
        }

        private async Task<ResourceResponse<Document>> UpsertDocumentAsync(Uri formInfoCollectionUri, FormResponseProperties formResponseProperties)
        {
            formResponseProperties = new FormResponseProperties();
            formResponseProperties.Id = "ATL10";
            try
            {
                var result = await Client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
            }
            catch (Exception ex)
            {
				Console.WriteLine(ex.ToString());
            }
            return null;
		}

		private Task<ResourceResponse<Document>> UpsertDocumentAsync(PageResponseProperties newPageResponseProperties, Uri pageCollectionUri)
		{
			return Client.UpsertDocumentAsync(pageCollectionUri, newPageResponseProperties);
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
                var formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);

                //Verify Response Id is exist or Not
                var existingFormResponseProperties = ReadFormInfoByResponseId(formResponseProperties.GlobalRecordID, formInfoCollectionUri);
                if (existingFormResponseProperties == null)
                {
                    formResponseProperties.Id = formResponseProperties.GlobalRecordID;
                    formResponseProperties.PageIds = formResponseProperties.PageIds ?? new List<int>();
                    var result = await Client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
                }
                else
                {
                    //Create attachment
                    if (existingFormResponseProperties.RecStatus == RecordStatus.Saved)
                    {
                        Attachment attachment = null;
                        var hierarchialResponse = GetHierarchialResponsesByResponseId(existingFormResponseProperties.GlobalRecordID, true);
                        var hierarchialResponseJson = JsonConvert.SerializeObject(hierarchialResponse);
                        attachment = CreateAttachment(existingFormResponseProperties.SelfLink, AttachmentId, existingFormResponseProperties.GlobalRecordID, hierarchialResponseJson);

                    }
                    //if (existingFormResponseProperties.RecStatus == RecordStatus.Saved || true)
                    //{
                    //    Attachment attachment = null;
                    //    attachment = GetAttachmentInfo(existingFormResponseProperties.GlobalRecordID, AttachmentId, existingFormResponseProperties.SelfLink, null);
                    //    if (attachment == null)
                    //    {
                    //        var hierarchialResponse = GetHierarchialResponsesByResponseId(existingFormResponseProperties.GlobalRecordID, true);
                    //        var hierarchialResponseJson = JsonConvert.SerializeObject(hierarchialResponse);
                    //        //Check Attachment is exist or not ,If not create attachment                             
                    //        attachment = GetAttachmentInfo(existingFormResponseProperties.GlobalRecordID, AttachmentId, existingFormResponseProperties.SelfLink, hierarchialResponseJson);
                    //    }
                    //}

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
                        var result = await Client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties, null);
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
		public List<SurveyResponse> GetAllResponsesWithFieldNames(IDictionary<int, FieldDigest> fields, string relateParentId = null)
		{
			 
			List<SurveyResponse> surveyResponse = null;

            try
            {
                var surveyResponses = ReadAllResponsesWithFieldNames(fields, relateParentId);
                return surveyResponses;
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
			return surveyResponse;
		}

		private List<SurveyResponse> ReadAllResponsesWithFieldNames(IDictionary<int, FieldDigest> fieldDigestList, string relateParentId)
		{
			// Set some common query options
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
			List<SurveyResponse> surveyList = new List<SurveyResponse>();
			try
			{
				// One SurveyResponse per GlobalRecordId
				Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

				List<FormResponseProperties> parentGlobalIdList = ReadAllResponsesByRelateParentResponseId(relateParentId, fieldDigestList.FirstOrDefault().Value.FormId);
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
					Uri pageCollectionUri = GetCollectionUri(collectionName);
					var pageQuery = Client.CreateDocumentQuery(pageCollectionUri,
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

        #endregion Get hierarchial responses by ResponseId for DataConsisitencyServiceAPI


        #region Get hierarchial response IDs by ResponseId
        /// <summary>
        /// GetHierarchialResponsesByResponseId
        /// </summary>
        /// <param name="responseId"></param>
        /// <param name="includeDeletedRecords"></param>
        /// <returns></returns>
        public HierarchicalDocumentResponseProperties GetHierarchialResponseIdsByResponseId(string responseId, bool includeDeletedRecords = false, bool excludeInProcessRecords = false)
        {
            HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = new HierarchicalDocumentResponseProperties();
            Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
            var documentResponseProperties = ReadAllResponseIdsByExpression(Expression("GlobalRecordID", EQ, responseId), formInfoCollectionUri, includeDeletedRecords).SingleOrDefault();
            if (documentResponseProperties != null)
            {
                hierarchicalDocumentResponseProperties.FormResponseProperties = documentResponseProperties.FormResponseProperties;
                hierarchicalDocumentResponseProperties.PageResponsePropertiesList = documentResponseProperties.PageResponsePropertiesList;
                hierarchicalDocumentResponseProperties.ChildResponseList = GetChildResponseIds(responseId, formInfoCollectionUri);
            }
            return hierarchicalDocumentResponseProperties;
        }

        private List<HierarchicalDocumentResponseProperties> GetChildResponseIds(string parentResponseId, Uri formInfoCollectionUri, bool includeDeletedRecords = false)
        {
            var childResponseList = new List<HierarchicalDocumentResponseProperties>();
            var documentResponsePropertiesList = ReadAllResponseIdsByExpression(Expression("RelateParentId", EQ, parentResponseId), formInfoCollectionUri, includeDeletedRecords);
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

        private List<DocumentResponseProperties> ReadAllResponseIdsByExpression(string expression, Uri formInfoCollectionUri, bool includeDeletedRecords, string collectionAlias = "c")
        {
            var documentResponsePropertiesList = new List<DocumentResponseProperties>();
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = Client.CreateDocumentQuery(formInfoCollectionUri,
                    SELECT + AssembleSelect(collectionAlias, "GlobalRecordID")
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

        #endregion Get hierarchial response IDs by ResponseId

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
		private List<FormResponseProperties> ReadAllResponsesByRelateParentResponseId(string relateParentId, string formId)
		{
			try
			{
				List<FormResponseProperties> globalRecordIdList = new List<FormResponseProperties>();

				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
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
