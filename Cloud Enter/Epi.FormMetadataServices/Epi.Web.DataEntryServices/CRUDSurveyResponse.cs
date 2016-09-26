using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		public const string FormInfoCollectionName = "FormInfo";

		Microsoft.Azure.Documents.Database _database;
		Dictionary<string, DocumentCollection> _documentCollections = new Dictionary<string, DocumentCollection>();

		public CRUDSurveyResponse()
		{
			ParseConnectionString();

			//Getting reference to Database 
			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				_database = GetOrCreateDatabaseAsync(client, DatabaseName);
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
		private Microsoft.Azure.Documents.Database GetOrCreateDatabaseAsync(DocumentClient client, string databaseName)
		{
			if (_database == null)
			{
				_database = client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
				if (_database == null)
				{
					_database = client.CreateDatabaseAsync(new Microsoft.Azure.Documents.Database { Id = databaseName }, null).Result;
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
                    IndexingPolicy indexingPolicy = new IndexingPolicy();
                    indexingPolicy.Automatic = true;
                    indexingPolicy.IndexingMode = IndexingMode.Consistent;
                    indexingPolicy.ExcludedPaths = new Collection<ExcludedPath>
                        {
                             new ExcludedPath
                             {
                                 Path="/*"
                             }
                        };
                    switch (collectionId)
                    {
                        case FormInfoCollectionName:
                            // FormInfo collection indexing here.                           
                            indexingPolicy.IncludedPaths = new Collection<IncludedPath>
                       {
                              new IncludedPath
                               {
                                   Path="/_ts/?",
                                   Indexes=GetIndexInfo()
                               },
                              new IncludedPath
                               {
                                   Path="/GlobalRecordID/?",
                                   Indexes=GetIndexInfo()
                               },
                               new IncludedPath
                               {
                                   Path="/FormId/?",
                                   Indexes=GetIndexInfo()
                               },
                              new IncludedPath
                               {
                                   Path="/FormName/?",
                                   Indexes=GetIndexInfo()
                               },
                              new IncludedPath
                               {
                                   Path="/RecStatus/?",
                                   Indexes=GetIndexInfo()
                               },
                               new IncludedPath
                               {
                                   Path="/RelateParentId/?",
                                   Indexes=GetIndexInfo()
                               },
                               new IncludedPath
                               {
                                   Path="/IsRelatedView/?",
                                   Indexes=GetIndexInfo()
                               },
                               new IncludedPath
                               {
                                   Path="/IsDraftMode/?",
                                   Indexes=GetIndexInfo()
                               },

                           //Every property (also the Title)gets a hash index on strings, 
                       };


                            //collectionSpec.IndexingPolicy = indexingPolicy;
                            break;

                        default:
                            indexingPolicy.IncludedPaths = new Collection<IncludedPath>
                                                                {
                                                                    new IncludedPath
                                                                    {
                                                                        Path="/_ts/?",
                                                                        Indexes=GetIndexInfo()
                                                                    },
                                                                    new IncludedPath
                                                                    {
                                                                        Path="/GlobalRecordID/?",
                                                                        Indexes=GetIndexInfo()
                                                                    },
                                                                    new IncludedPath
                                                                    {
                                                                        Path="/PageId/?",
                                                                        Indexes=new Collection<Index>
                                                                        {
                                                                            new RangeIndex(DataType.Number)
                                                                        }
                                                                } 
                           //Every property (also the Title)gets a hash index on strings, 
                       };
                            break;
                    }

                    var collectionSpec = new DocumentCollection
                    {
                        Id = collectionId,
                        IndexingPolicy = indexingPolicy
                    };
#else
                    var collectionSpec = new DocumentCollection
                    {
                        Id = collectionId,
                    };
#endif //ConfigureIndexing

                    documentCollection = client.CreateDocumentCollectionAsync(databaseLink, collectionSpec).Result;
                    _documentCollections[collectionId] = documentCollection;
                }
            }
            return documentCollection;
        }
#endregion

        private Collection<Index> GetIndexInfo()
        {
            Collection<Index> Indexes = new Collection<Index>
                                   {
                                       new RangeIndex(DataType.Number),
                                       new HashIndex(DataType.String)
                                   };
            return Indexes;
        }

        private DocumentCollection GetCollectionReference(DocumentClient client, string collectionId)
		{
			//Get a reference to the DocumentDB Database 
			var database = GetOrCreateDatabaseAsync(client, DatabaseName);

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


		public bool UpdateResponseStatus(string responseId, int responseStatus)
		{
			bool isSuccessful = false;

			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				try
				{
					Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);

					var formResponseProperties = ReadFormInfoByResponseId(responseId, client, formInfoCollectionUri);
					if (formResponseProperties != null)
					{
						if (formResponseProperties.RecStatus != responseStatus)
						{
							formResponseProperties.RecStatus = responseStatus;
							var response = client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
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
		public async Task<bool> InsertResponseAsync(DocumentResponseProperties documentResponseProperties, int userId)
		{
			try
			{
				var responseId = documentResponseProperties.GlobalRecordID;

				//Instance of DocumentClient"
				using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
				{
				   Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);

					var formResponseProperties = ReadFormInfoByResponseId(responseId, client, formInfoCollectionUri);

					//Create Survey Properties 
					Uri pageCollectionUri = GetCollectionUri(client, documentResponseProperties.CollectionName);
				   
					//Read Surveyinfo from document db
					var pageResponseProperties = ReadPageResponsePropertiesByResponseId(documentResponseProperties.GlobalRecordID, client, pageCollectionUri);
					if (pageResponseProperties == null)
					{
						pageResponseProperties = documentResponseProperties.PageResponsePropertiesList[0];
					}
					else
					{
						pageResponseProperties.ResponseQA = documentResponseProperties.PageResponsePropertiesList[0].ResponseQA;
					}

					if (formResponseProperties == null)
					{
						var formName = documentResponseProperties.FormName;
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

					var pageResponse = await client.UpsertDocumentAsync(pageCollectionUri, pageResponseProperties);
					var formResponse = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return true;
		}
#endregion

        //public async Task<bool> InsertChildResponseAsync(FormDocumentDBEntity documentResponseProperties, int userId)


#region SaveQuestionInDocumentDB

        /// <summary>
        /// This method help to save form properties 
        /// and also used for delete operation.Ex:RecStatus=0
        /// </summary>
        /// <param name="documentResponseProperties"></param>
        /// <returns></returns>
        public async Task<bool> SaveResponseAsync(DocumentResponseProperties documentResponseProperties)
		{
			try
			{
				//Instance of DocumentClient"
				using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
				{
					FormResponseProperties formResponseProperties = documentResponseProperties.FormResponseProperties;
					var formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);

					//Verify Response Id is exist or Not
					var responseFormInfo = ReadFormInfoByResponseId(formResponseProperties.GlobalRecordID, client, formInfoCollectionUri);
					if (responseFormInfo == null)
					{
						formResponseProperties.Id = formResponseProperties.GlobalRecordID;
						var response = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties);
					}
					else
					{
						documentResponseProperties.FormResponseProperties.Id = responseFormInfo.Id;
						documentResponseProperties.FormResponseProperties.RelateParentId = responseFormInfo.RelateParentId;
						
						var pageId = formResponseProperties.PageIds[0];
						formResponseProperties.PageIds = new List<int>();
						formResponseProperties.PageIds = responseFormInfo.PageIds;
						if (!responseFormInfo.PageIds.Contains(pageId))
						{
							formResponseProperties.PageIds.Add(pageId);
						}

						var response = await client.UpsertDocumentAsync(formInfoCollectionUri, formResponseProperties, null);
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

		
#region Get PageResponseProperties By ResponseId, FormId, and PageId
		public PageResponseProperties GetPageResponsePropertiesByResponseId(string responseId, string formId, int pageId)
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

		public HierarchicalDocumentResponseProperties GetHierarchicalResponses(string responseId, string parentResponseId = null)
		{
			var hierarchicalResult = new HierarchicalDocumentResponseProperties();
			var documentResponseProperties = GetAllPageResponsesByResponseId(responseId);
			hierarchicalResult.FormResponseProperties = documentResponseProperties.FormResponseProperties;
			hierarchicalResult.PageResponsePropertiesList = documentResponseProperties.PageResponsePropertiesList;

			return hierarchicalResult;
		}

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

#region Read all GlobalRecord By Related Parent ID
		public FormResponseProperties ReadAllGlobalRecordIDByParentID(DocumentClient client, Uri formInfoCollectionUri, string formName, string relateParentID)
		{
			try
			{
				// Set some common query options
				FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

				var query = client.CreateDocumentQuery(formInfoCollectionUri,
					SELECT
					+ AssembleSelect(formName, "DateCreated", "_self", "RelateParentId", "id")
					+ FROM + formName
					+ WHERE 
					+ AssembleWhere(formName, Expression("GlobalRecordID", EQ, relateParentID),
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

#region Do children exist for responseId
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
#endregion

		public int GetFormResponseCount(string formId)
		{

			using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
			{
				Uri formInfoCollectionUri = GetCollectionUri(client, FormInfoCollectionName);
				var formResponseCount = GetFormResponseCount(client, formInfoCollectionUri, formId);
				return formResponseCount;
			}
		}

		private int GetFormResponseCount(DocumentClient client, Uri formInfoCollectionUri, string formId)
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

				if (relateParentId != null)
				{
					query = client.CreateDocumentQuery(formInfoCollectionUri,
						SELECT 
						+ AssembleSelect(FormInfoCollectionName, "*")
						+ FROM + FormInfoCollectionName
						+ WHERE
						+ AssembleWhere(FormInfoCollectionName, Expression("RelateParentId", EQ, relateParentId),
															   And_Expression("FormId", EQ, formId),
															   And_Expression("RecStatus", NE, RecordStatus.Deleted))
						, queryOptions);
				}
				else
				{
					query = client.CreateDocumentQuery(formInfoCollectionUri,
					   SELECT 
					   + AssembleSelect(FormInfoCollectionName, "*")
					   + FROM + FormInfoCollectionName
					   + WHERE
					   + AssembleWhere(FormInfoCollectionName, Expression("FormId", EQ, formId), 
															  And_Expression("RecStatus", NE, 0))
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

#region Readfull Form Info for DataConsisitencyServiceAPI
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
	}
}
