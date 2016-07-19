using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.EntityObjects;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace Epi.Cloud.DataEntryServices
{
	//class Family : Resource
	//{
	//    public string FamilyName { get; set; }
	//    public string lastname { get; set; }
	//}
	public class CRUDSurveyResponse : MetadataAccessor
	{
		private const string LT = "<";
		private const string LE = "<=";
		private const string EQ = "=";
		private const string NE = "!=";
		private const string GT = ">";
		private const string GE = ">=";
		private const string SELECT = "SELECT ";
		private const string FROM = " FROM ";
		private const string WHERE =  " WHERE ";
		private const string AND = " AND ";
		private const string OR = " OR ";

		// public DocumentClient client;
		public string serviceEndpoint;
		public string authKey;
		public string DatabaseName = "EpiInfo7";
		public string FormInfoCollectionName = "FormInfo";

		public CRUDSurveyResponse()
		{
			ParseConnectionString();

			//Getting reference to Database 
			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				Microsoft.Azure.Documents.Database database = GetOrCreateDatabaseAsync(client, DatabaseName).Result;
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

		private Uri CreateFormInfoDocumentCollectionUri()
		{
			return UriFactory.CreateDocumentCollectionUri(DatabaseName, FormInfoCollectionName);
		}

		public bool UpdateResponseStatus(string responseId, int responseStatus)
		{
			bool isSuccessful = false;

			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				try
				{
					//Getting reference to Database 
					Microsoft.Azure.Documents.Database database = GetOrCreateDatabaseAsync(client, DatabaseName).Result;
					//Create Survey Properties 
					DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, FormInfoCollectionName).Result;

					var formResponseProperties = ReadFormInfoByResponseId(responseId, client, collection);
					if (formResponseProperties != null)
					{
						if (formResponseProperties.RecStatus != responseStatus)
						{
							formResponseProperties.RecStatus = responseStatus;
							var response = client.UpsertDocumentAsync(collection.SelfLink, formResponseProperties);
							isSuccessful = true;
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

		#region InsertToSruveyToDocumentDB
			/// <summary>
			/// Created instance of DocumentClient and Getting reference to database and Document collections
			/// </summary>
			///
		public async Task<bool> InsertToSurveyToDocumentDBAsync(FormDocumentDBEntity formData, int userId)
		{
			try
			{
                var responseId = formData.GlobalRecordID;

				//Instance of DocumentClient"
				using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
				{
					//Getting reference to Database 
					Microsoft.Azure.Documents.Database database = GetOrCreateDatabaseAsync(client, DatabaseName).Result;

                    DocumentCollection formInfoCollection = ReadCollectionAsync(client, database.SelfLink, FormInfoCollectionName).Result;

                    var formResponseProperties = ReadFormInfoByResponseId(responseId, client, formInfoCollection);

                    //Create Survey Properties 
                    DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, formData.CollectionName).Result;

					Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, formData.CollectionName);
				   
					//Read Surveyinfo from document db
					var pageResponseProperties = ReadPageResponsePropertiesByResponseId(formData.GlobalRecordID, client, docUri);
					if (pageResponseProperties == null)
					{
                        pageResponseProperties = formData.PageResponsePropertiesList[0];
					}
					else
					{
                        pageResponseProperties.ResponseQA = formData.PageResponsePropertiesList[0].ResponseQA;
					}

                    if (formResponseProperties == null)
                    {
                        var formName = formData.FormName;
                        var formDigest = GetFormDigestByFormName(formName);
                        var formId = formDigest.FormId;

                        formResponseProperties = new FormResponseProperties();

                        formResponseProperties.GlobalRecordID = responseId;
                        formResponseProperties.FormId = formId;
                        formResponseProperties.FormName = formName;
                        formResponseProperties.FirstSaveTime = DateTime.UtcNow;
                        formResponseProperties.IsDraftMode = formDigest.IsDraftMode;
                        formResponseProperties.IsRelatedView = formDigest.ParentFormId != null;
                    }

                    var pageId = pageResponseProperties.PageId;
                    formResponseProperties.UserId = userId;
                    formResponseProperties.RecStatus = RecordStatus.InProcess;
                    if (!formResponseProperties.PageIds.Contains(pageResponseProperties.PageId))
                    {
                        formResponseProperties.PageIds.Add(pageId);
                    }

                    var pageResponse = await client.UpsertDocumentAsync(collection.SelfLink, pageResponseProperties);
                    var formResponse = await client.UpsertDocumentAsync(formInfoCollection.SelfLink, formResponseProperties);
                }
            }
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return true;
		}
		#endregion

		#region SaveQuestionInDocumentDB

		/// <summary>
		/// This method help to save form properties 
		/// and also used for delete operation.Ex:RecStatus=0
		/// </summary>
		/// <param name="formData"></param>
		/// <returns></returns>
		public async Task<bool> SaveSurveyQuestionInDocumentDBAsync(FormDocumentDBEntity formData)
		{
			try
			{
				//Instance of DocumentClient"
				using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
				{
					//Getting reference to Database 
					Microsoft.Azure.Documents.Database database = GetOrCreateDatabaseAsync(client, DatabaseName).Result;

					//Create Survey Properties 
					DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, FormInfoCollectionName).Result;

                    FormResponseProperties formResponseProperties = formData.FormResponseProperties;

					//Verify Response Id is exist or Not
					var responseFormInfo = ReadFormInfoByResponseId(formResponseProperties.GlobalRecordID, client, collection);
					if (responseFormInfo == null)
					{
						formResponseProperties.Id = formResponseProperties.GlobalRecordID;
						var documentCollectionUri = CreateFormInfoDocumentCollectionUri();
						var response = await client.UpsertDocumentAsync(documentCollectionUri, formResponseProperties);
					}
					else
					{
						formData.FormResponseProperties.Id = responseFormInfo.Id;
						formData.FormResponseProperties.RelateParentId = responseFormInfo.RelateParentId;
						
						var pageId = formResponseProperties.PageIds[0];
						formResponseProperties.PageIds = new List<int>();
						formResponseProperties.PageIds = responseFormInfo.PageIds;
						if (!responseFormInfo.PageIds.Contains(pageId))
						{
							formResponseProperties.PageIds.Add(pageId);
						}

						var response = await client.UpsertDocumentAsync(collection.SelfLink, formResponseProperties, null);

					}


				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return true;
		}
		#endregion

		#region MapPageResponseProperties
		public PageResponseProperties MapPageResponseProperties(PageResponseProperties sourcePageResponseProperties, Dictionary<string, string> responseQA)
		{
			PageResponseProperties pageResponseProperties = new PageResponseProperties();
			pageResponseProperties.ResponseQA = responseQA;
			pageResponseProperties.GlobalRecordID = sourcePageResponseProperties.GlobalRecordID;
			pageResponseProperties.Id = sourcePageResponseProperties.Id;
			return pageResponseProperties;
		}
		#endregion

		#region ReadOrCreateDabase
		/// <summary>
		///If DB is not avaliable in Document Db create DB
		/// </summary>
		private async Task<Microsoft.Azure.Documents.Database> GetOrCreateDatabaseAsync(DocumentClient client, string databaseName)
		{
			var database = client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
			if (database == null)
			{
				database = await client.CreateDatabaseAsync(new Microsoft.Azure.Documents.Database { Id = databaseName }, null);
			}
			return database;
		}


		#endregion


		#region ReadCollection 
		/// <summary>
		/// Read or CreateCollection in Document DB
		/// </summary>
		private async Task<DocumentCollection> ReadCollectionAsync(DocumentClient client, string databaseLink, string collectionId)
		{
			var documentCollection = client.CreateDocumentCollectionQuery(databaseLink).Where(c => c.Id == collectionId).AsEnumerable().FirstOrDefault();
			if (documentCollection == null)
			{
				var collectionSpec = new DocumentCollection { Id = collectionId };
				//IndexingPolicy indexingPolicy = new IndexingPolicy { Automatic = false };
				//indexingPolicy.IndexingMode = IndexingMode.Consistent;

				//indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });
				//indexingPolicy.IncludedPaths.Add(new IncludedPath
				//{
				//    Path = "/GlobalRecordID/?",
				//    Indexes = new Collection<Index>()
				//    {
				//        new HashIndex(DataType.String) { Precision = 3 }
				//    }
				//});
				//indexingPolicy.IncludedPaths.Add(new IncludedPath
				//{
				//    Path = "/RecStatus/?",
				//    Indexes = new Collection<Index>()
				//    {
				//        new RangeIndex(DataType.Number) { Precision = -1 }
				//    }
				//});
				//indexingPolicy.IncludedPaths.Add(new IncludedPath
				//{
				//    Path = "/RelateParentId/?",
				//    Indexes = new Collection<Index>()
				//    {
				//        new HashIndex(DataType.String) { Precision = -1 }
				//    }
				//});

				//indexingPolicy.IncludedPaths.Add(new IncludedPath
				//{
				//    Path = "/_ts/?",
				//    Indexes = new Collection<Index>()
				//    {
				//        new RangeIndex(DataType.Number) { Precision = -1 }
				//    }
				//});

				//collectionSpec.IndexingPolicy = indexingPolicy;
				documentCollection = client.CreateDocumentCollectionAsync(databaseLink, collectionSpec).Result;
				//documentCollection = CreateCollectionAsync(client, databaseLink, collectionId).Result;

			}
			return documentCollection;
		}
		#endregion 

		#region ReadOrCreateCollection
		/// <summary>
		/// Read or CreateCollection in Document DB
		/// </summary>
		private async Task<ResourceResponse<DocumentCollection>> CreateCollectionAsync(DocumentClient client, string databaseLink, string collectionId)
		{
			// Use UriFactory to build the DocumentLink 
			var collectionResponse = client.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = collectionId }, null);
			return collectionResponse.Result;
		}
		#endregion


		#region Get PageResponseProperties By ResponseId, FormId, and PageId
		public PageResponseProperties GetPageResponsePropertiesByResponseId(string responseId, string formId, string pageId)
		{
			PageResponseProperties pageResponseProperties = null;
			try
			{
				//Instance of DocumentClient"
				using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
				{
					var collectionName = GetFormDigest(formId).FormName + pageId;
					Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);

					//Read collection and store data  
					pageResponseProperties = ReadPageResponsePropertiesByResponseId(responseId, client, docUri);
					return pageResponseProperties;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return null;
		}

		private PageResponseProperties ReadPageResponsePropertiesByResponseId(string responseId, DocumentClient client, Uri DocUri, string collectionAlias = "c")
		{
			PageResponseProperties pageResponseProperties = null;
			// Set some common query options
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
			try
			{
				var query = client.CreateDocumentQuery(DocUri,
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

                List<FormResponseProperties> parentGlobalIdList = ReadAllGlobalRecordIdByRelateParentID(client, null, fieldDigestList.FirstOrDefault().Value.FormId);
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
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                    var pageQuery = client.CreateDocumentQuery(docUri,
                        SELECT + columnList
                        + FROM + collectionName
                        + WHERE + collectionName + ".GlobalRecordID in (" + parentGlobalRecordIds + ")", queryOptions);

                    foreach (var items in pageQuery.AsQueryable())
                    {
                        var json = JsonConvert.SerializeObject(items);
                        var pageResponseQA = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        string globalRecordId = pageResponseQA["GlobalRecordID"];
                        Epi.Cloud.Common.EntityObjects.SurveyResponse surveyResponse;
                        FormResponseDetail formResponseDetail;
                        if (!responsesByGlobalRecordId.TryGetValue(globalRecordId, out surveyResponse))
                        {
                            surveyResponse = new Epi.Cloud.Common.EntityObjects.SurveyResponse { SurveyId = new Guid(formId), ResponseId = new Guid(globalRecordId) };
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

        #region Get FormInfo By ResponseId
        public FormDocumentDBEntity GetFormInfoByResponseId(string responseId)
		{
			Uri docUri = CreateFormInfoDocumentCollectionUri();
			FormDocumentDBEntity formDocumentDbResponse = new FormDocumentDBEntity { GlobalRecordID = responseId };
			//Read all global record Id
			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				var formResponseProperties = ReadFormPropertiesByResponseId(client, docUri, responseId);
				formDocumentDbResponse.FormResponseProperties = formResponseProperties;
			}
			return formDocumentDbResponse;
		}
        #endregion Get FormInfo By ResponseId

        #region Get FormInfo By ResponseId
        public FormDocumentDBEntity GetFormPageResponsesByResponseId(string responseId)
        {
            Uri docUri = CreateFormInfoDocumentCollectionUri();
            FormDocumentDBEntity formDocumentDbEntity = new FormDocumentDBEntity { GlobalRecordID = responseId };
            //Read all global record Id
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                var formResponseProperties = ReadFormPropertiesByResponseId(client, docUri, responseId);
                formDocumentDbEntity.FormResponseProperties = formResponseProperties;

                if (formResponseProperties != null)
                {
                    foreach (var pageId in formResponseProperties.PageIds)
                    {
                        string collectionName = formResponseProperties.FormName + pageId;
                        docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                        var pageResponseProperties = ReadPageResponsePropertiesByResponseId(responseId, client, docUri);
                        formDocumentDbEntity.PageResponsePropertiesList.Add(pageResponseProperties);
                    }
                }
            }
            return formDocumentDbEntity;
        }
        #endregion Get FormInfo By ResponseId



        private string AssembleSelect(string collectionName, params string[] columnNames)
		{
			string columnList;
			if (columnNames.Length == 1 && columnNames[0] == "*")
			{
				columnList = "*";
			}
			else
			{
				columnList = collectionName + '.' + string.Join(", " + collectionName + '.', columnNames);
			}
			return columnList;
		}

		private string AssembleWhere(string collectionName, params string[] expressions)
		{
			string where;
			where = string.Join(" ", expressions.Select(e => e.ToString().Replace("?", collectionName)));
			return where;
		}

		private static string Expression(string left, string relational_operator, object right)
		{
			string expression;

			if (right is int)
				expression = string.Format("?.{0} {1} {2}", left, relational_operator, right.ToString());
			else
				expression = string.Format("?.{0} {1} {2}", left, relational_operator, "'" + right.ToString() + "'");
			return expression;
		}

		private static string And_Expression(string left, string relational_operator, object right)
		{
			var expression = string.Format("AND ?.{0} {1} {2}", left, relational_operator, "'" + right.ToString() + "'");
			return expression;
		}

		private static string Or_Expression(string left, string relational_operator, object right)
		{
			var expression = string.Format("OR ?.{0} {1} {2}", left, relational_operator, "'" + right.ToString() + "'");
			return expression;
		}

		private List<SurveyResponse> GetAllDataByChildFormIdByRelateId(DocumentClient client, string FormId, string relateParentId, Dictionary<int, FieldDigest> fieldDigestList, string collectionName)
		{
			// Set some common query options
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
			List<SurveyResponse> surveyList = new List<SurveyResponse>();
			try
			{
				// One SurveyResponse per GlobalRecordId
				Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

				List<FormResponseProperties> _ChildGlobalIdList = ReadAllGlobalRecordIdByRelateParentID(client, relateParentId, FormId);
				string ChildGlobalRecordIds = string.Empty;

				foreach (var id in _ChildGlobalIdList)
				{
					ChildGlobalRecordIds += ChildGlobalRecordIds == string.Empty ? "'" + id.GlobalRecordID + "'" : ",'" + id.GlobalRecordID + "'";
				}


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

					var pageQuery = client.CreateDocumentQuery(docUri, SELECT + columnList + " FROM  " + collectionName + pageId + WHERE + collectionName + pageId + ".GlobalRecordID in ( " + ChildGlobalRecordIds + ")", queryOptions);

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

#region Read all GlobalRecord By Related Parent ID
		public FormResponseProperties ReadAllGlobalRecordIDByParentID(DocumentClient client, DocumentCollection collection, string SurveyName, string relateParentID)
		{
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = client.CreateDocumentQuery(collection.SelfLink,
					SELECT
					+ AssembleSelect(SurveyName, "DateCreated", "_self", "RelateParentId", "id")
					+ FROM + SurveyName
					+ WHERE 
					+ AssembleWhere(SurveyName, Expression("GlobalRecordID", EQ, relateParentID),
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
		#endregion

		#region ResponseId Is exist or Not in Document DB
	public bool DoesResponseIdExist(string responseId)
	{

		using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
		{
			//Getting reference to Database 
			Microsoft.Azure.Documents.Database database = GetOrCreateDatabaseAsync(client, DatabaseName).Result;

			//Create Survey Properties 
			DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, FormInfoCollectionName).Result;

			//Verify Response Id is exist or Not
			var responseFormInfo = ReadFormInfoByResponseId(responseId, client, collection);
			return responseFormInfo != null;
		}
	}

	private FormResponseProperties ReadFormInfoByResponseId(string responseId, DocumentClient client, DocumentCollection collection)
		{
			var collectionAlias = FormInfoCollectionName;

			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = client.CreateDocumentQuery(collection.SelfLink,
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
#endregion

		public int GetFormResponseCount(string formId)
		{

			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				//Getting reference to Database 
				Microsoft.Azure.Documents.Database database = GetOrCreateDatabaseAsync(client, DatabaseName).Result;

				//Create Survey Properties 
				DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, FormInfoCollectionName).Result;

				//Verify Response Id is exist or Not
				var formResponseCount = GetFormResonseCount(client, collection, formId);
				return formResponseCount;
			}
		}

		private int GetFormResonseCount(DocumentClient client, DocumentCollection collection, string formId)
		{
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = client.CreateDocumentQuery(collection.SelfLink,
					SELECT
					+ AssembleSelect(FormInfoCollectionName, "GlobalRecordID")
					+ FROM + FormInfoCollectionName
					+ WHERE
					+ AssembleWhere(FormInfoCollectionName, Expression("FormId", EQ, formId),
												And_Expression("RecStatus", NE, RecordStatus.Deleted))
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

		public FormResponseProperties GetFormResponseState(string responseId)
		{

			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				//Getting reference to Database 
				Microsoft.Azure.Documents.Database database = GetOrCreateDatabaseAsync(client, DatabaseName).Result;

				//Create Survey Properties 
				DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, FormInfoCollectionName).Result;

				//Verify Response Id is exist or Not
				var formResponseProperties = ReadFormResponseState(responseId, client, collection);
				return formResponseProperties;
			}
		}

		private FormResponseProperties ReadFormResponseState(string responseId, DocumentClient client, DocumentCollection collection)
		{
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = client.CreateDocumentQuery(collection.SelfLink,
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

#region Read SurveyInfo from DocumentDB by RelateParentID
		private PageResponseProperties ReadSurveyInfoByRelateParentID(DocumentClient client, Uri DocUri, string SurveyName, string RelateParentID)
		{
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
				var query = client.CreateDocumentQuery(DocUri,
					SELECT 
					+ AssembleSelect(SurveyName, "id", "ProjectDigest", "GlobalRecordID", "RelateParentId", "FormId", "_self", "Digest")
					+ FROM + SurveyName
					+ WHERE
					+ AssembleWhere(SurveyName, Expression("GlobalRecordID", EQ, RelateParentID))
					, queryOptions);

				var surveyDataItems = query.AsEnumerable();
				var pageResponseProperties = (PageResponseProperties)surveyDataItems.FirstOrDefault();
				return pageResponseProperties;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return null;
		}
#endregion

#region Read GlobalRecordId By Relate Parent Id
		private List<FormResponseProperties> ReadAllGlobalRecordIdByRelateParentID(DocumentClient client, string relateParentId, string formId)
		{
			Uri docUri = CreateFormInfoDocumentCollectionUri();
			try
			{
				List<FormResponseProperties> globalRecordIdList = new List<FormResponseProperties>();

				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
				IEnumerable<dynamic> surveyDatadFromDocumentDdfB;
				IQueryable<dynamic> query;

				if (relateParentId != null)
				{
					query = client.CreateDocumentQuery(docUri,
						SELECT 
						+ AssembleSelect(FormInfoCollectionName, "GlobalRecordID", "FormId", "FormName", "RecStatus", 
																"FirstSaveLogonName", "FirstSaveTime", "LastSaveLogonName", 
																"LastSaveTime", "UserId", "IsRelatedView", "IsDraftMode", 
																"id", "_ts")
						+ FROM + FormInfoCollectionName
						+ WHERE
						+ AssembleWhere(FormInfoCollectionName, Expression("RelateParentId", EQ, relateParentId),
															   And_Expression("FormId", EQ, formId),
															   And_Expression("RecStatus", NE, RecordStatus.Deleted))
						, queryOptions);
				}
				else
				{
					query = client.CreateDocumentQuery(docUri,
					   SELECT 
					   + AssembleSelect(FormInfoCollectionName, "GlobalRecordID", "FormId", "FormName", "RecStatus",
															   "FirstSaveLogonName", "FirstSaveTime", "LastSaveLogonName",
															   "LastSaveTime", "UserId", "IsRelatedView", "IsDraftMode",
															   "id", "_ts")
					   + FROM + FormInfoCollectionName
					   + WHERE
					   + AssembleWhere(FormInfoCollectionName, Expression("FormId", EQ, formId), 
															  And_Expression("RecStatus", NE, 0))
					   , queryOptions);
				}

				surveyDatadFromDocumentDdfB = query.AsEnumerable<dynamic>();
				globalRecordIdList = surveyDatadFromDocumentDdfB.Select(x => (FormResponseProperties)x).ToList();

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
		private FormResponseProperties ReadFormPropertiesByResponseId(DocumentClient client, Uri docUri, string responseId)
		{
			// tell server we only want 25 record
			FeedOptions options = new FeedOptions { MaxItemCount = 25, EnableCrossPartitionQuery = true };
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = client.CreateDocumentQuery(docUri,
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


		#region ReadFormResponseProperties by ResponseId ,RecStatus != Deleted
		private FormResponseProperties ReadFormResponseProperties(DocumentClient client, Uri docUri, string responseId)
		{
			// tell server we only want 25 record
			//FeedOptions options = new FeedOptions { MaxItemCount = 25, EnableCrossPartitionQuery = true };
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
				var query = client.CreateDocumentQuery(docUri,
					SELECT
					+ "*"
					+ WHERE
					+ AssembleWhere(FormInfoCollectionName, Expression("GlobalRecordID", EQ, responseId),
															And_Expression("RecStatus", NE, RecordStatus.Deleted))
					, queryOptions);

				var formInfoResponse = query.AsEnumerable().FirstOrDefault();

				FormResponseProperties formResponseProperties = new FormResponseProperties();
				formResponseProperties.GlobalRecordID = formInfoResponse.GlobalRecordID;
				formResponseProperties.FormId = formInfoResponse.FormId;
				formResponseProperties.FormName = formInfoResponse.FormName;
				formResponseProperties.RecStatus = Convert.ToInt32(formInfoResponse.RecStatus);
				formResponseProperties.RelateParentId = formInfoResponse.RelateParentId;
				formResponseProperties.FirstSaveLogonName = formInfoResponse.FirstSaveLogonName;
				formResponseProperties.FirstSaveTime = formInfoResponse.FirstSaveTime;
				formResponseProperties.LastSaveLogonName = formInfoResponse.LastSaveLogonName;
				formResponseProperties.LastSaveTime = formInfoResponse.LastSaveTime;
				formResponseProperties.UserId = Convert.ToInt32(formInfoResponse.UserId);
				formResponseProperties.IsRelatedView = Convert.ToBoolean(formInfoResponse.IsRelatedView);
				formResponseProperties.IsDraftMode = Convert.ToBoolean(formInfoResponse.IsDraftMode);
				return formResponseProperties;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return null;
		}
#endregion

#region ReadDataFromColleciton
		public object ReadDataFromCollection(string globalRecordId, string collectionName, string documnetQuery)
		{

			Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
			try
			{
				//Instance of DocumentClient"
				using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
				{
					var query = client.CreateDocumentQuery(docUri, documnetQuery);
					var surveyDataFromDocumentDB = (FormResponseProperties)query.AsEnumerable().FirstOrDefault();
					return surveyDataFromDocumentDB;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return "True";
		}
#endregion

#region Readfull Form Info 
		public List<string> ReadfullFormInfo(string responseId)
		{
			List<string> surveyNames = new List<string>();
			surveyNames.Add("Zika1");
			surveyNames.Add("Zika2");
			surveyNames.Add("Zika3");

			List<string> fullFormInfo = new List<string>();

			Uri docUri = null;
			string pageResponseDetailResource = string.Empty;
			PageResponseProperties ResponseDocumentDB = new PageResponseProperties();
			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				foreach (var collectionName in surveyNames)
				{
					docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);

					var query = client.CreateDocumentQuery(docUri, 
						SELECT 
						+ AssembleSelect(collectionName, "GlobalRecordID", "ResponseQA")
						+ FROM + collectionName
						+ WHERE 
						+ AssembleWhere(collectionName, Expression("GlobalRecordID", EQ, responseId)));

					var surveyDatadFromDocumentDdfB = query.AsEnumerable();


					foreach (var item in surveyDatadFromDocumentDdfB)
					{
						pageResponseDetailResource = JsonConvert.SerializeObject((PageResponseProperties)item);
					}
					fullFormInfo.Add(pageResponseDetailResource);
				}

			}
			return fullFormInfo;
		}
		#endregion
#if false
		public async Task<FormDocumentDBEntity> ReadAllGlobalRecordIdByResponseId(string responseId)
		{
			Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, FormInfoColectionName);
			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				FormPropertiesResource _formProperties = ReadFormProperties(client, docUri, FormInfoColectionName, responseId);
				string formName = _formProperties.FormName;
				if (_formProperties != null)
				{
					docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, formName + pageId);
					var FormQuestionAndAnswer = ReadSurveyInfoByResponseIdAndSurveyId(client, docUri, formName, _formProperties.GlobalRecordID);
					surveyDocumentDbResponse.FormProperties = _formProperties;
					surveyDocumentDbResponse.PageResponseDetail = FormQuestionAndAnswer;
				}
			}
			return surveyDocumentDbResponse;
		}
#endif
	}
}
