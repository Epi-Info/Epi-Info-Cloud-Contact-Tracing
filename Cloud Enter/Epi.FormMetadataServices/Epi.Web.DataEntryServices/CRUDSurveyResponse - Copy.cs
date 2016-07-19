using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.DataEntryServices.Model;
//using Epi.Web.EF;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Epi.Cloud.Common.EntityObjects;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Common;
using System.Collections.ObjectModel;

namespace Epi.Cloud.DataEntryServices
{
    //class Family : Resource
    //{
    //    public string FamilyName { get; set; }
    //    public string lastname { get; set; }
    //}
    public class CRUDSurveyResponse : MetadataAccessor
    {
        // public DocumentClient client;
        public string serviceEndpoint;
        public string authKey;
        public string DatabaseName = "EpiInfo7";
        public string FormInfoColectionName = "FormInfo";

        public CRUDSurveyResponse()
        {
            ParseConnectionString();
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

        #region InsertToSruveyToDocumentDB
        /// <summary>
        /// Created instance of DocumentClient and Getting reference to database and Document collections
        /// </summary>
        ///
        public async Task<bool> InsertToSurveyToDocumentDBAsync(FormDocumentDBEntity formData)
        {
            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    //Getting reference to Database 
                    Microsoft.Azure.Documents.Database database = ReadDatabaseAsync(client, DatabaseName).Result;

                    //Create Survey Properties 
                    DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, formData.CollectionName).Result;

                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, formData.CollectionName);
                   
                    //Read Surveyinfo from document db
                    var DocumentDBResponse = ReadSurveyInfoByResponseIdandSurveyId(client, docUri, formData.SurveyName, formData.PageResponseDetail.GlobalRecordID);
                    if (DocumentDBResponse.GlobalRecordID == null)
                    {
                        var responseDb = await client.UpsertDocumentAsync(docUri, formData.PageResponseDetail);
                    }
                    else
                    {
                        var _UIdigest = formData.PageResponseDetail.ResponseQA;
                        formData.PageResponseDetail = MappingSurveyDataToDocumentDb(DocumentDBResponse, _UIdigest);
                        var response = await client.UpsertDocumentAsync(collection.SelfLink, formData.PageResponseDetail);
                    }
                }
            }
            catch (Exception ex)
            {

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
                    Microsoft.Azure.Documents.Database database = ReadDatabaseAsync(client, DatabaseName).Result;

                    //Create Survey Properties 
                    DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, FormInfoColectionName).Result;

                    //Verify Response Id is exist or Not
                    var DocumentDBResponse = ResponseIdExistOrNot(client, collection, FormInfoColectionName, formData.FormProperties.GlobalRecordID);
                    //var team2Doc = client.CreateDocumentQuery<FormProperties>(collection.DocumentsLink).Where(d => d.GlobalRecordID == formData.FormProperties.GlobalRecordID).AsEnumerable().FirstOrDefault();
                    //string json = JsonConvert.SerializeObject(formData.FormProperties);
                    if (DocumentDBResponse == null)
                    {
                        formData.FormProperties.Id = formData.FormProperties.GlobalRecordID;
                        var response = await client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseName, FormInfoColectionName), formData.FormProperties);
                    }
                    else
                    {
                        formData.FormProperties.Id = DocumentDBResponse.Id;
                        formData.FormProperties.RelateParentId = DocumentDBResponse.RelateParentId;
                        var response = await client.UpsertDocumentAsync(collection.SelfLink, formData.FormProperties, null);

                    }


                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }
        #endregion

        #region MappingSurveyDataToDocumentDb
        public PageResponseDetailResource MappingSurveyDataToDocumentDb(PageResponseDetailResource DocumentDBSurveyMetaPages, Dictionary<string, string> UIDigest)
        {
            PageResponseDetailResource formQA = new PageResponseDetailResource();
            formQA.ResponseQA = UIDigest;
            formQA.GlobalRecordID = DocumentDBSurveyMetaPages.GlobalRecordID;
            formQA.Id = DocumentDBSurveyMetaPages.Id;
            return formQA;
        }
        #endregion

        #region ReadOrCreateDabase
        /// <summary>
        ///If DB is not avaliable in Document Db create DB
        /// </summary>
        private async Task<Microsoft.Azure.Documents.Database> ReadDatabaseAsync(DocumentClient client, string databaseName)
        {
            Microsoft.Azure.Documents.Database db = client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
            if (db == null)
            {
                var dbresponse = await CreateDatabaseAsync(client, databaseName);
                return dbresponse;
            }
            else
            {
                return db;
            }
        }


        #endregion

        #region CreateDatabase 
        /// <summary>
        ///If DB is not avaliable in Document Db create DB
        /// </summary>
        private async Task<ResourceResponse<Microsoft.Azure.Documents.Database>> CreateDatabaseAsync(DocumentClient client, string databaseName)
        {
            var dbresponse = await client.CreateDatabaseAsync(new Microsoft.Azure.Documents.Database { Id = databaseName }, null);
            return Task.FromResult(dbresponse).Result;
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
                //        new HashIndex(DataType.String) { Precision = 3 }

                //    }
                //});

                //indexingPolicy.IncludedPaths.Add(new IncludedPath
                //{
                //    Path = "/ts/?",
                //    Indexes = new Collection<Index>()
                //    {
                //        new HashIndex(DataType.String) { Precision = 3 }
                //    }
                //});

                //collectionSpec.IndexingPolicy = indexingPolicy;
                documentCollection = client.CreateDocumentCollectionAsync(databaseLink, collectionSpec).Result;
                //documentCollection = CreateCollectionAsync(client, databaseLink, collectionId).Result;

            }
            return documentCollection;
        }
        #endregion 

        #region ReadORCreateCollection
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


        #region ReadDataFromCollectionDocumentDB
        private PageResponseDetailResource ReadSurveyDataFromDocumentDB(DocumentClient client, Uri DocUri, string collectionName, string responseId)
        {

            PageResponseDetailResource formData = new PageResponseDetailResource();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = client.CreateDocumentQuery(DocUri, "SELECT  " + collectionName + ".ResponseQA "
                    + " FROM " + collectionName
                    + " WHERE " + collectionName + ".GlobalRecordID = '" + responseId + "'"
                    , queryOptions);

                var surveyDataFromDocumentDB = (PageResponseDetailResource)query.AsEnumerable().FirstOrDefault();

                if (surveyDataFromDocumentDB != null && surveyDataFromDocumentDB.ResponseQA != null)
                {
                    formData.ResponseQA = surveyDataFromDocumentDB.ResponseQA;
                    return formData;
                }
            }
            catch (Exception ex)
            {

            }


            return null;

        }
        #endregion






        #region ReadSurveyFromDocumentDBByResponseId,PAgeId
        public PageResponseDetailResource ReadSurveyFromDocumentDBByPageandRespondIdAsync(string collectionName, string formid, string responseId, string pageId)
        {

            PageResponseDetailResource surveyAnswer = new PageResponseDetailResource();
            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {

                    string CollectionName = collectionName + pageId;
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                    //Read collection and store data  
                    surveyAnswer = ReadSurveyDataFromDocumentDB(client, docUri, CollectionName, responseId);
                    return surveyAnswer;
                }
            }
            catch (DocumentQueryException ex)
            {

            }
            return surveyAnswer;
        }
        #endregion

        #region ReadAllRecordsBySurveyID 
        public List<SurveyResponse> ReadAllRecordsBySurveyID(string dbName, IDictionary<int, FieldDigest> fieldDigestList)
        {
            List<SurveyResponse> surveyResponse = null;

            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    //Instance of DocumentClient"
                    surveyResponse = GellAllSurveyDataBySurveyId(client, fieldDigestList, null);
                }

            }
            catch (Exception ex)
            {

            }
            return surveyResponse;
        }
        #endregion


        #region ReadAllRecordByChildFormIdandRelateParentId
        public List<SurveyResponse> ReadAllRecordByChildFormIdandRelateParentId(string SurveyId, string RelateParentId, IDictionary<int, FieldDigest> fields)
        {
             
            List<SurveyResponse> surveyResponse = null;

            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    //Instance of DocumentClient"
                    var surveyResponses = GellAllSurveyDataBySurveyId(client, fields, RelateParentId);
                    return surveyResponses;
                }

            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return surveyResponse;
        }
        #endregion

        #region ReadAllGlobalRecordidBySurveyId
        public async Task<FormDocumentDBEntity> ReadFormInfoByGlobalRecordIdAndSurveyId(string formId, string responseId, int pageId)
        {
            Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, FormInfoColectionName);
            FormPropertiesResource _formPropertiesList = new FormPropertiesResource();
            FormPropertiesResource _formProperties = new FormPropertiesResource();
            FormDocumentDBEntity surveyDocumentDbResponse = new FormDocumentDBEntity();
            string collectionName = GetFormDigest(formId).FormName;
            //Read all global record Id
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                _formProperties = ReadFormProperties(client, docUri, FormInfoColectionName, formId, responseId);
                if (_formProperties != null)
                {
                    docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName + pageId);
                    var FormQuestionAndAnswer = ReadSurveyInfoByResponseIdandSurveyId(client, docUri, collectionName, _formProperties.GlobalRecordID);
                    surveyDocumentDbResponse.FormProperties = _formProperties;
                    surveyDocumentDbResponse.PageResponseDetail = FormQuestionAndAnswer;
                }
            }
            return surveyDocumentDbResponse;
        }
        #endregion      




        #region ReadDataFromCollectionDocumentDB
        private List<SurveyResponse> GellAllSurveyDataBySurveyId(DocumentClient client, IDictionary<int, FieldDigest> fieldDigestList, string relateParentId)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                // One SurveyResponse per GlobalRecordId
                Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

                List<FormPropertiesResource> parentGlobalIdList = ReadAllGlobalRecordIdByRelateParentID(client, null, fieldDigestList.FirstOrDefault().Value.FormId);
                string parentGlobalRecordIds = string.Empty;

                if (parentGlobalIdList != null)
                {
                    foreach (var id in parentGlobalIdList)
                    {
                        parentGlobalRecordIds += parentGlobalRecordIds == string.Empty ? "'" + id.GlobalRecordID + "'" : ",'" + id.GlobalRecordID + "'";
                    }
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
                    var columnList = AssembleColumnList(collectionName, pageGroup.Select(g => "ResponseQA." + g.FieldName.ToLower()).ToArray())
                        + ","
                        + AssembleColumnList(collectionName, "GlobalRecordID", "_ts");
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);
                    var pageQuery = client.CreateDocumentQuery(docUri, "SELECT " + columnList + " FROM  " + collectionName + " WHERE " + collectionName + ".GlobalRecordID in ( " + parentGlobalRecordIds + ")", queryOptions);

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
                        formResponseDetail.FormId = formId;
                        formResponseDetail.FormName = formName;

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

        private string AssembleColumnList(string collectionName, params string[] columnNames)
        {
            var columnList = collectionName + '.' + string.Join(',' + collectionName + '.', columnNames);
            return columnList;
        }
        #endregion


        private List<SurveyResponse> GetAllDataByChildFormIdByRelateId(DocumentClient client, string FormId, string relateParentId, Dictionary<int, FieldDigest> fieldDigestList, string collectionName)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                // One SurveyResponse per GlobalRecordId
                Dictionary<string, SurveyResponse> responsesByGlobalRecordId = new Dictionary<string, SurveyResponse>();

                List<FormPropertiesResource> _ChildGlobalIdList = ReadAllGlobalRecordIdByRelateParentID(client, relateParentId, FormId);
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
                    var columnList = AssembleColumnList(pageColectionName, pageGroup.Select(g => "ResponseQA." + g.FieldName.ToLower()).ToArray())
                        + ","
                        + AssembleColumnList(pageColectionName, "GlobalRecordID", "_ts");
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName + pageId);

                    var pageQuery = client.CreateDocumentQuery(docUri, "SELECT " + columnList + " FROM  " + collectionName + pageId + " WHERE " + collectionName + pageId + ".GlobalRecordID in ( " + ChildGlobalRecordIds + ")", queryOptions);

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
        public FormPropertiesResource ReadAllGlobalRecordIDByParentID(DocumentClient client, DocumentCollection collection, string SurveyName, string relateParentID)
        {
            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                //var response = client.ReadDocumentAsync(UriFactory.CreateDocumentUri(SurveyName, CollectionName, ResponseId));
                //var readDocument = response.Result;

                var columnList = AssembleColumnList(SurveyName,
              "DateCreated",
              "_self",
              "RelateParentId",
              "id");
                var query = client.CreateDocumentQuery(collection.SelfLink, "SELECT "
                    + columnList
                    + " from " + SurveyName
                    + " WHERE " + SurveyName + ".GlobalRecordID = '" + relateParentID + "'and " + SurveyName + ".RecStatus != 0", queryOptions);
                var surveyDataFromDocumentDB = (FormPropertiesResource)query.AsEnumerable().FirstOrDefault();
                return surveyDataFromDocumentDB;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
#endregion





#region ResponseId Is exist or Not in Document DB
        public FormPropertiesResource ResponseIdExistOrNot(DocumentClient client, DocumentCollection collection, string SurveyName, string ResponseId)
        {
            try
            {
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                //var response = client.ReadDocumentAsync(UriFactory.CreateDocumentUri(SurveyName, CollectionName, ResponseId));
                //var readDocument = response.Result;

                var columnList = AssembleColumnList(SurveyName,
              "DateCreated",
              "_self",
              "RelateParentId",
              "id");
                var query = client.CreateDocumentQuery(collection.SelfLink, "SELECT "
                    + columnList
                    + " from " + SurveyName
                    + " WHERE " + SurveyName + ".GlobalRecordID = '" + ResponseId + "'and " + SurveyName + ".RecStatus != 0", queryOptions);
                var surveyDataFromDocumentDB = (FormPropertiesResource)query.AsEnumerable().FirstOrDefault();
                return surveyDataFromDocumentDB;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
#endregion

#region Read SurveyInfo from DocumentDB by ResponseId
        public PageResponseDetailResource ReadSurveyInfoByResponseIdandSurveyId(DocumentClient client, Uri DocUri, string SurveyName, string ResponseId)
        {
            try
            {
                PageResponseDetailResource ResponseDocumentDB = new PageResponseDetailResource();

                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                var query = client.CreateDocumentQuery(DocUri, "SELECT " + SurveyName
                    + ".id," + SurveyName
                    + ".ProjectDigest," + SurveyName
                    + ".GlobalRecordID," + SurveyName
                    + ".FormId," + SurveyName
                    + "._self," + SurveyName
                    + ".ResponseQA from " + SurveyName
                    + " WHERE " + SurveyName + ".GlobalRecordID = '" + ResponseId + "'", queryOptions);

                var surveyDatadFromDocumentDdfB = query.AsEnumerable();


                foreach (var item in surveyDatadFromDocumentDdfB)
                {
                    var DocumentDbDigest = (PageResponseDetailResource)item;
                    return DocumentDbDigest;
                }

                return ResponseDocumentDB;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
#endregion


#region Read SurveyInfo from DocumentDB by RelateParentID
        public PageResponseDetailResource ReadSurveyInfoByRelateParentID(DocumentClient client, Uri DocUri, string SurveyName, string RelateParentID)
        {
            try
            {
                PageResponseDetailResource ResponseDocumentDB = new PageResponseDetailResource();

                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                var query = client.CreateDocumentQuery(DocUri, "SELECT " + SurveyName
                    + ".id," + SurveyName
                    + ".ProjectDigest," + SurveyName
                    + ".GlobalRecordID," + SurveyName
                    + ".RelateParentId," + SurveyName
                    + ".FormId," + SurveyName
                    + "._self," + SurveyName
                    + ".Digest from " + SurveyName
                    + " WHERE " + SurveyName + ".GlobalRecordID = '" + RelateParentID + "'", queryOptions);

                var surveyDatadFromDocumentDdfB = query.AsEnumerable();


                foreach (var item in surveyDatadFromDocumentDdfB)
                {
                    var DocumentDbDigest = (PageResponseDetailResource)item;
                    return DocumentDbDigest;
                }

                return ResponseDocumentDB;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
#endregion

#region Read GlobalRecordId By Relate Parent Id
        public List<FormPropertiesResource> ReadAllGlobalRecordIdByRelateParentID(DocumentClient client, string RelateParentID, string formId)
        {
            Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, FormInfoColectionName);
            try
            {
                FormPropertiesResource ResponseDocumentDB = null;
                List<FormPropertiesResource> _globalRecordIdList = new List<FormPropertiesResource>();

                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                IEnumerable<dynamic> surveyDatadFromDocumentDdfB;

                if (RelateParentID != null)
                {
                    var query = client.CreateDocumentQuery(docUri, "SELECT " + FormInfoColectionName
                       + ".id," + FormInfoColectionName
                       + ".GlobalRecordID from " + FormInfoColectionName
                       + " WHERE " + FormInfoColectionName + ".RelateParentId = '" + RelateParentID + "'and " + FormInfoColectionName + ".RecStatus != 0 and " + FormInfoColectionName + ".FormId ='" + formId + "'", queryOptions);
                    surveyDatadFromDocumentDdfB = query.AsEnumerable();
                }
                else
                {
                    var query = client.CreateDocumentQuery(docUri, "SELECT " + FormInfoColectionName
                         + ".id," + FormInfoColectionName
                         + ".GlobalRecordID from " + FormInfoColectionName
                         + " WHERE " + FormInfoColectionName + ".RecStatus != 0 and " + FormInfoColectionName + ".FormId ='" + formId + "'", queryOptions);
                    surveyDatadFromDocumentDdfB = query.AsEnumerable();
                }


                foreach (var item in surveyDatadFromDocumentDdfB)
                {
                    ResponseDocumentDB = (FormPropertiesResource)item;
                    _globalRecordIdList.Add(ResponseDocumentDB);

                }
                return _globalRecordIdList;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

#endregion

#region Read List of all GlobalRecordId by FormId ,RecStatus
        public FormPropertiesResource ReadFormProperties(DocumentClient client, Uri DocUri, string SurveyName, string formId, string ResponseId)
        {
            // tell server we only want 25 record
            FeedOptions options = new FeedOptions { MaxItemCount = 25, EnableCrossPartitionQuery = true };
            try
            {
                FormPropertiesResource _formproperties = new FormPropertiesResource();
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                var query = client.CreateDocumentQuery(DocUri, "SELECT " + SurveyName
                    + ".id," + SurveyName
                    + ".UserId," + SurveyName
                    + ".GlobalRecordID," + SurveyName
                    + ".RecStatus," + SurveyName
                    + ".FirstSaveTime," + SurveyName
                    + ".RelateParentId," + SurveyName
                    + ".LastSaveTime From " + SurveyName
                    + " WHERE " + SurveyName + ".FormId = '" + formId + "'" + " AND " + SurveyName + ".GlobalRecordID='" + ResponseId + "'and " + FormInfoColectionName + ".RecStatus != 0", queryOptions);

                var TestQuyery = client.CreateDocumentQuery<FormPropertiesResource>(DocUri, options).AsDocumentQuery();

                // var surveyDatadFromDocumentDdfB = (FormProperties)query.AsEnumerable().FirstOrDefault();
                var surveyDatadFromDocumentDdfBd = query.AsEnumerable().FirstOrDefault();
                _formproperties.GlobalRecordID = surveyDatadFromDocumentDdfBd.GlobalRecordID;
                _formproperties.FirstSaveTime = surveyDatadFromDocumentDdfBd.FirstSaveTime;
                _formproperties.LastSaveTime = surveyDatadFromDocumentDdfBd.LastSaveTime;
                _formproperties.RelateParentId = surveyDatadFromDocumentDdfBd.RelateParentId;
                _formproperties.RecStatus = Convert.ToInt16(surveyDatadFromDocumentDdfBd.RecStatus);
                _formproperties.UserId = Convert.ToInt16(surveyDatadFromDocumentDdfBd.UserId);
                return _formproperties;
            }
            catch (Exception ex)
            {

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
                    var surveyDataFromDocumentDB = (FormPropertiesResource)query.AsEnumerable().FirstOrDefault();
                    return surveyDataFromDocumentDB;
                }
            }
            catch (Exception ex)
            {

            }
            return "True";
        }
        #endregion

        #region Readfull Form Info 
        public List<string> ReadfullFormInfo(string globalId)
        {
            List<string> surveyNames = new List<string>();
            surveyNames.Add("Zika1");
            surveyNames.Add("Zika2");
            surveyNames.Add("Zika3");

            List<string> FullFormInfo = new List<string>();

            Uri docUri = null;
            string FormDigest = string.Empty;
            PageResponseDetailResource ResponseDocumentDB = new PageResponseDetailResource();
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                foreach (var collectionName in surveyNames)
                {

                    docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName);

                    var query = client.CreateDocumentQuery(docUri, "SELECT " + collectionName
                                                            + ".GlobalRecordID," + collectionName
                                                            + ".ResponseQA from " + collectionName
                                                            + " WHERE " + collectionName + ".GlobalRecordID = '" + globalId + "'");

                    var surveyDatadFromDocumentDdfB = query.AsEnumerable();


                    foreach (var item in surveyDatadFromDocumentDdfB)
                    {
                        FormDigest = JsonConvert.SerializeObject((PageResponseDetailResource)item);
                    }
                    FullFormInfo.Add(FormDigest);
                }

            }
            return FullFormInfo;
        }
        #endregion


    }
}
