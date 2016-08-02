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

namespace Epi.Cloud.DataEntryServices
{
    //class Family : Resource
    //{
    //    public string FamilyName { get; set; }
    //    public string lastname { get; set; }
    //}
    public class CRUDSurveyResponse
    {
        // public DocumentClient client;
        public string serviceEndpoint;
        public string authKey;
        public string DatabaseName = "EpiInfo7";
        public string FormInfo = "FormInfo";

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
        public async Task<bool> InsertToSurveyToDocumentDBAsync(Survey formData)
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

                    RequestOptions requestOption = null;

                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, formData.CollectionName);
                    string json = JsonConvert.SerializeObject(formData.FormQuestionandAnswer);
                    //Read Surveyinfo from document db
                    var DocumentDBResponse = ReadSurveyInfoByResponseIdandSurveyId(client, docUri, formData.SurveyName, formData.FormQuestionandAnswer.GlobalRecordID);
                    if (DocumentDBResponse.GlobalRecordID == null)
                    {
                        var responseDb = await client.CreateDocumentAsync(docUri, formData.FormQuestionandAnswer);
                    }
                    else
                    {
                        var _UIdigest = formData.FormQuestionandAnswer.SurveyQAList;
                        formData.FormQuestionandAnswer = MappingSurveyDataToDocumentDb(DocumentDBResponse, _UIdigest);
                        var response = await client.UpsertDocumentAsync(collection.SelfLink, formData.FormQuestionandAnswer);
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
        public async Task<bool> SaveSurveyQuestionInDocumentDBAsync(Survey formData)
        {
            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    //Getting reference to Database 
                    Microsoft.Azure.Documents.Database database = ReadDatabaseAsync(client, DatabaseName).Result;

                    //Create Survey Properties 
                    DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, FormInfo).Result;

                    //Verify Response Id is exist or Not
                    var DocumentDBResponse = ResponseIdExistOrNot(client, collection, FormInfo, formData.GlobalRecordID);
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, FormInfo);
                    string json = JsonConvert.SerializeObject(formData.FormProperties);
                    if (DocumentDBResponse == null)
                    {
                        var response = await client.CreateDocumentAsync(docUri, formData.FormProperties);
                    }
                    else
                    {
                        formData.FormProperties.Id = DocumentDBResponse.Id;
                        var response = await client.UpsertDocumentAsync(collection.SelfLink, formData.FormProperties);
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
        public FormQuestionandAnswer MappingSurveyDataToDocumentDb(FormQuestionandAnswer DocumentDBSurveyMetaPages, Dictionary<string, string> UIDigest)
        {
            FormQuestionandAnswer FormQA = new FormQuestionandAnswer();
            FormQA.SurveyQAList = UIDigest;
            FormQA.GlobalRecordID = DocumentDBSurveyMetaPages.GlobalRecordID;
            FormQA.Id = DocumentDBSurveyMetaPages.Id;
            return FormQA;
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
                documentCollection = CreateCollectionAsync(client, databaseLink, collectionId).Result;

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

        #region CreateOrUpdateDocuments

        /// <summary>
        /// Get the collection and conver to json and send to document db
        /// </summary>
        /// <param name="collectionLink"></param>
        private Document CreateDocumentsAsync(DocumentClient client, string collectionLink, FormQuestionandAnswer surveyData, RequestOptions requestoption)
        {

            Document docs = new Document();
            try
            {
                var DDBresponse = client.UpsertDocumentAsync(collectionLink, surveyData, requestoption);
                var Response = DDBresponse.Result;

            }
            catch (Exception ex)
            {
                var xe = ex.ToString();
            }
            return docs;
        }


        private async Task<ResourceResponse<Document>> CreateSurveyDocumentPropertiesAsync(DocumentClient client, DocumentCollection collection, SurveyProperties surveyProperties, RequestOptions requestoption, string SurveyName)
        {

            Document docs = new Document();
            try
            {
                var responseDB = ReadSurveyDataPropertiesFromDocumentDB(client, surveyProperties, SurveyName, collection);
                if (responseDB != null)
                {
                    surveyProperties.FirstSaveTime = responseDB.FirstSaveTime;
                    var documentTasks = await client.ReplaceDocumentAsync(responseDB.SelfLink, surveyProperties);
                    var statusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;
                }
            }
            catch (Exception ex)
            {
                var xe = ex.ToString();
            }
            return null;
        }

        #endregion

        #region ReadDataForToChangetheDeleteStatus
        private SurveyProperties ReadDataFromDocumentDB(DocumentClient client, string dbname, string Query, Uri docUri)
        {
            try
            {
                //var resquery = client.CreateDocumentQuery<SurveyProperties>(docUri).Where(f => f.GlobalRecordID == "cdcad4bf-6c70-4cb9-88a6-1851cba4aaf4");
                //var result = resquery.AsEnumerable().FirstOrDefault<SurveyProperties>();

                var response = client.CreateDocumentQuery(docUri, Query);
                var surveyDataFromDocumentDB = (SurveyProperties)response.AsEnumerable().FirstOrDefault();
                return surveyDataFromDocumentDB;
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;

        }
        #endregion

        #region ReadDataFromCollectionDocumentDB
        private FormQuestionandAnswer ReadSurveyDataFromDocumentDB(DocumentClient client, Uri DocUri, string collectionName, string responseId)
        {

            FormQuestionandAnswer formData = new FormQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = client.CreateDocumentQuery(DocUri, "SELECT  " + collectionName + ".SurveyQAList "
                    + " FROM " + collectionName
                    + " WHERE " + collectionName + ".GlobalRecordID = '" + responseId + "'"
                    , queryOptions);

                var surveyDataFromDocumentDB = (FormQuestionandAnswer)query.AsEnumerable().FirstOrDefault();

                if (surveyDataFromDocumentDB != null && surveyDataFromDocumentDB.SurveyQAList != null)
                {
                    formData.SurveyQAList = surveyDataFromDocumentDB.SurveyQAList;
                    return formData;
                }
            }
            catch (Exception ex)
            { 

            }


            return null;

        }
        #endregion

        #region ReadDataFromCollectionDocumentDB
        private SurveyProperties ReadSurveyDataPropertiesFromDocumentDB(DocumentClient client, SurveyProperties SurveyProperties, string surveyName, DocumentCollection collection)
        {


            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var columnList = AssembleColumnList(surveyName,
                "DateCreated",
                "_self");

            var query = client.CreateDocumentQuery(collection.SelfLink, "SELECT "
                + columnList
                + " from " + surveyName
                + " WHERE " + surveyName + ".GlobalRecordID = '" + SurveyProperties.GlobalRecordID + "'" + " and " + surveyName + ".FormId ='" + SurveyProperties.FormId + "'"
                , queryOptions);
            var surveyDataFromDocumentDB = (SurveyProperties)query.AsEnumerable().FirstOrDefault();
            return surveyDataFromDocumentDB;
        }
        #endregion




        #region ReadSurveyFromDocumentDBByResponseId,PAgeId
        public FormQuestionandAnswer ReadSurveyFromDocumentDBByPageandRespondIdAsync(string collectionName,string formid, string responseId, string pageId)
        {

            FormQuestionandAnswer surveyAnswer = new FormQuestionandAnswer();
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
                    surveyResponse = GellAllSurveyDataBySurveyId(client, DatabaseName, fieldDigestList);
                }

            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return surveyResponse;
        }
        #endregion


        #region ReadAllRecordByChildFormIdandRelateParentId
        public List<SurveyResponse> ReadAllRecordByChildFormIdandRelateParentId(string SurveyId, string RelateParentId, string dbName, Dictionary<int, FieldDigest> Params)
        {
            string collectionName = dbName;
            List<SurveyResponse> surveyResponse = null;

            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    //Instance of DocumentClient"
                    var surveyResponses = GetAllDataByChildFormIdbyRelateId(client, dbName, SurveyId, RelateParentId, Params, collectionName);
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


        #region DeleteDocumentById
        public async Task<string> DeleteDocumentByIdAsync(Survey formDetails)
        {
            //FormProperties formPropertie = new FormProperties();
            //using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            //{
            //    Uri docUri = UriFactory.CreateDocumentUri(DatabaseName, FormInfo, formDetails.SurveyProperties.GlobalRecordID);
            //    var query = client.CreateDocumentQuery<FormProperties>(docUri, new FeedOptions() { MaxItemCount = 1 });
            //    var DocumentDBResponse = query.Where(x => x.GlobalRecordID == formDetails.SurveyProperties.GlobalRecordID)
            //                              .Select(x => new FormProperties
            //                              { FormId = x.FormId,
            //                                  GlobalRecordID = x.GlobalRecordID,
            //                                  RelateParentId = x.RelateParentId,
            //                                  FirstSaveLogonName = x.FirstSaveLogonName,
            //                                  FirstSaveTime = x.FirstSaveTime,
            //                                  Id = x.Id
            //                              }).AsEnumerable();
            //    if(DocumentDBResponse!=null)
            //    {

            //    }
            //    Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, FormInfo);
            //    string json = JsonConvert.SerializeObject(formData.FormProperties);
            //    if (DocumentDBResponse == null)
            //    {
            //        surveyPropertie.FirstSaveTime = formDetails.SurveyProperties.FirstSaveTime;
            //        surveyPropertie.LastSaveTime = DateTime.UtcNow;
            //        //surveyPropertie.RecStatus = RecordStatus.Deleted;
            //        surveyPropertie.Id = formDetails.SurveyProperties.GlobalRecordID;
            //        surveyPropertie.GlobalRecordID = formDetails.SurveyProperties.GlobalRecordID;
            //        surveyPropertie.FormId = formDetails.SurveyProperties.FormId;
            //        var documentTasks = await client.ReplaceDocumentAsync(docUri, surveyPropertie);
            //        var response = await client.CreateDocumentAsync(docUri, formData.FormProperties);
            //    }
            //    try
            //    {
            //        //var responseDB = ReadDataFromDocumentDB(client,surveyInfo.SurveyName, Query, docUri);
            //        surveyPropertie.FirstSaveTime = formDetails.SurveyProperties.FirstSaveTime;
            //        surveyPropertie.LastSaveTime = DateTime.UtcNow;
            //        //surveyPropertie.RecStatus = RecordStatus.Deleted;
            //        surveyPropertie.Id = formDetails.SurveyProperties.GlobalRecordID;
            //        surveyPropertie.GlobalRecordID = formDetails.SurveyProperties.GlobalRecordID;
            //        surveyPropertie.FormId = formDetails.SurveyProperties.FormId;
            //        var documentTasks = await client.ReplaceDocumentAsync(docUri, surveyPropertie);
            //        var statusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;
            //    }
            //    catch (Exception ex)
            //    {
            //        var xe = ex.ToString();
            //    }
            //    return null;
            //}
            return "True";

        }
        #endregion


        #region ReadAllGlobalRecordidBySurveyId
        public Survey ReadFormInfoByGlobalRecordIdAndSurveyId(string collectionName, string FormId,string responseId,string pageId)
        {
            Uri docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, FormInfo);
            FormProperties _formPropertiesList = new FormProperties();
            FormProperties _formProperties = new FormProperties();
            Survey surveyDocumentDbResponse = new Survey();

            //Read all global record Id
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                 _formProperties = ReadFormProperties(client, docUri, FormInfo, FormId, responseId);
                if (_formProperties!= null)
                {
                    docUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionName+ pageId);
                    var FormQuestionAndAnswer = ReadSurveyInfoByResponseIdandSurveyId(client, docUri, collectionName, _formProperties.GlobalRecordID);
                    surveyDocumentDbResponse.FormProperties = _formProperties;
                    surveyDocumentDbResponse.FormQuestionandAnswer= FormQuestionAndAnswer;
                }
            }    
            return surveyDocumentDbResponse;
        }
        #endregion

        #region HelperMethod




        #region ReadDataFromCollectionDocumentDB
        private List<SurveyResponse> GellAllSurveyDataBySurveyId(DocumentClient client, string dbname, IDictionary<int, FieldDigest> fieldDigestList)
        {

            SurveyResponse surveyResponse = new SurveyResponse();

            FormQuestionandAnswer surveyData = new FormQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                Dictionary<string, Dictionary<string, string>> responsesByGlobalRecordId = new Dictionary<string, Dictionary<string, string>>();
                //var results = new List<dynamic[]>();
                var pageGroups = fieldDigestList.Values.GroupBy(d => d.PageId);
                foreach (var pageGroup in pageGroups)
                {
                    var pageId = pageGroup.Key;
                    var formName = pageGroup.First().FormName;
                    var collectionName = formName + pageId;
                    var columnList = AssembleColumnList(collectionName, pageGroup.Select(g => "SurveyQAList." + g.Name.ToLower()).ToArray())
                        + ","
                        + AssembleColumnList(collectionName, "GlobalRecordID", "_ts");
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(dbname, collectionName);
                    var pageQuery = client.CreateDocumentQuery(docUri, "SELECT " + columnList + " FROM  " + collectionName /* + " WHERE " + collection + ".GlobalRecordID = '" + formId + "'"*/, queryOptions);

                    foreach (var items in pageQuery.AsQueryable())
                    {
                        var json = JsonConvert.SerializeObject(items);
                        var pageResponseQA = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        var existingTimestamp = Int64.MinValue;
                        Dictionary<string, string> aggregateResponseQA;
                        var globalRecordId = pageResponseQA["GlobalRecordID"];
                        if (!responsesByGlobalRecordId.TryGetValue(globalRecordId, out aggregateResponseQA))
                        {
                            aggregateResponseQA = new Dictionary<string, string>();
                            responsesByGlobalRecordId.Add(globalRecordId, aggregateResponseQA);
                        }
                        else
                        {
                            existingTimestamp = Int64.Parse(aggregateResponseQA["_ts"]);
                        }

                        foreach (dynamic qa in pageResponseQA)
                        {
                            string key = qa.Key;
                            switch (key)
                            {
                                case "_ts":
                                    var newTimeStamp = Int64.Parse(qa.Value);
                                    if (newTimeStamp > existingTimestamp)
                                    {
                                        existingTimestamp = newTimeStamp;
                                        aggregateResponseQA["_ts"] = qa.Value;
                                    }
                                    break;

                                default:
                                    aggregateResponseQA[key] = qa.Value;
                                    break;
                            }
                        }
                    }
                }

                foreach (var qa in responsesByGlobalRecordId.Values.OrderByDescending(v => Int64.Parse(v["_ts"])))
                {
                    surveyResponse = new SurveyResponse
                    {
                        ResponseQA = qa,
                        ResponseId = new Guid(qa["GlobalRecordID"]),
                        DateUpdated = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(Double.Parse(qa["_ts"]))
                    };
                    surveyList.Add(surveyResponse);
                }
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

        #region ReadDataFromCollectionDocumentDB
        private List<SurveyResponse> GetAllDataByChildFormIdbyRelateId(DocumentClient client, string dbname, string FormId, string RelateParentId, Dictionary<int, FieldDigest> fieldDigestList, string collectionName)
        {
            SurveyResponse surveyResponse = new SurveyResponse();

            FormQuestionandAnswer surveyData = new FormQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                Dictionary<string, Dictionary<string, string>> responsesByGlobalRecordId = new Dictionary<string, Dictionary<string, string>>();
                //var results = new List<dynamic[]>();
                var pageGroups = fieldDigestList.Values.GroupBy(d => d.PageId);
                foreach (var pageGroup in pageGroups)
                {
                    var pageId = pageGroup.Key;
                    var formName = pageGroup.First().FormName;
                     collectionName = "CSFAnalysis10";
                    var columnList = AssembleColumnList(collectionName, pageGroup.Select(g => "SurveyQAList." + g.Name.ToLower()).ToArray())
                        + ","
                        + AssembleColumnList(collectionName, "GlobalRecordID", "_ts");
                    Uri docUri = UriFactory.CreateDocumentCollectionUri(dbname, collectionName);
                    var pageQuery = client.CreateDocumentQuery(docUri, "SELECT " + columnList + " FROM  " + collectionName /* + " WHERE " + collection + ".GlobalRecordID = '" + formId + "'"*/, queryOptions);

                    foreach (var items in pageQuery.AsQueryable())
                    {
                        var json = JsonConvert.SerializeObject(items);
                        var pageResponseQA = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        var existingTimestamp = Int64.MinValue;
                        Dictionary<string, string> aggregateResponseQA;
                        var globalRecordId = pageResponseQA["GlobalRecordID"];
                        if (!responsesByGlobalRecordId.TryGetValue(globalRecordId, out aggregateResponseQA))
                        {
                            aggregateResponseQA = new Dictionary<string, string>();
                            responsesByGlobalRecordId.Add(globalRecordId, aggregateResponseQA);
                        }
                        else
                        {
                            existingTimestamp = Int64.Parse(aggregateResponseQA["_ts"]);
                        }

                        foreach (dynamic qa in pageResponseQA)
                        {
                            string key = qa.Key;
                            switch (key)
                            {
                                case "_ts":
                                    var newTimeStamp = Int64.Parse(qa.Value);
                                    if (newTimeStamp > existingTimestamp)
                                    {
                                        existingTimestamp = newTimeStamp;
                                        aggregateResponseQA["_ts"] = qa.Value;
                                    }
                                    break;

                                default:
                                    aggregateResponseQA[key] = qa.Value;
                                    break;
                            }
                        }
                    }
                }

                foreach (var qa in responsesByGlobalRecordId.Values.OrderByDescending(v => Int64.Parse(v["_ts"])))
                {
                    surveyResponse = new SurveyResponse
                    {
                        ResponseQA = qa,
                        ResponseId = new Guid(qa["GlobalRecordID"]),
                        DateUpdated = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(Double.Parse(qa["_ts"]))
                    };
                    surveyList.Add(surveyResponse);
                }
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return surveyList;
        }
        #endregion

        #region ResponseId Is exist or Not in Document DB
        public SurveyProperties ResponseIdExistOrNot(DocumentClient client, DocumentCollection collection, string SurveyName, string ResponseId)
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
              "id");
                var query = client.CreateDocumentQuery(collection.SelfLink, "SELECT "
                    + columnList
                    + " from " + SurveyName
                    + " WHERE " + SurveyName + ".GlobalRecordID = '" + ResponseId + "'", queryOptions);
                var surveyDataFromDocumentDB = (SurveyProperties)query.AsEnumerable().FirstOrDefault();
                return surveyDataFromDocumentDB;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        #endregion

        #region Read SurveyInfo from DocumentDB by ResponseId
        public FormQuestionandAnswer ReadSurveyInfoByResponseIdandSurveyId(DocumentClient client, Uri DocUri, string SurveyName, string ResponseId)
        {
            try
            {
                FormQuestionandAnswer ResponseDocumentDB = new FormQuestionandAnswer();

                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                var query = client.CreateDocumentQuery(DocUri, "SELECT " + SurveyName
                    + ".id," + SurveyName
                    + ".ProjectDigest," + SurveyName
                    + ".GlobalRecordID," + SurveyName
                    + ".FormId," + SurveyName
                    + "._self," + SurveyName
                    + ".SurveyQAList from " + SurveyName
                    + " WHERE " + SurveyName + ".GlobalRecordID = '" + ResponseId + "'", queryOptions);

                var surveyDatadFromDocumentDdfB = query.AsEnumerable();


                foreach (var item in surveyDatadFromDocumentDdfB)
                {
                    var DocumentDbDigest = (FormQuestionandAnswer)item;
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
        public FormQuestionandAnswer ReadSurveyInfoByRelateParentID(DocumentClient client, Uri DocUri, string SurveyName, string RelateParentID)
        {
            try
            {
                FormQuestionandAnswer ResponseDocumentDB = new FormQuestionandAnswer();

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
                    var DocumentDbDigest = (FormQuestionandAnswer)item;
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

        #region CollectionIsExistorNot
        private async Task<DocumentCollection> CollectionIsExistorNotAsync(DocumentClient client, string dbname, string collectionName)
        {
            try
            {
                var documentTasks = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(dbname, collectionName));
                return documentTasks;

            }
            catch (DocumentClientException ex)
            {

            }
            return null;
        }
        #endregion



        #region Read List of all GlobalRecordId by FormId ,RecStatus
        public FormProperties ReadFormProperties(DocumentClient client, Uri DocUri, string SurveyName,string formId, string ResponseId)
        {
            try
            {
                FormProperties _formproperties = new FormProperties();
                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                var query = client.CreateDocumentQuery(DocUri, "SELECT " + SurveyName
                    + ".id," + SurveyName 
                    + ".GlobalRecordID," + SurveyName  
                    + ".FirstSaveTime," + SurveyName
                    + ".RelateParentId," + SurveyName
                    + ".LastSaveTime From " + SurveyName 
                    + " WHERE " + SurveyName + ".FormId = '" + formId + "'"+" AND "+ SurveyName+ ".GlobalRecordID='"+ ResponseId+"'", queryOptions);


               // var surveyDatadFromDocumentDdfB = (FormProperties)query.AsEnumerable().FirstOrDefault();
                var surveyDatadFromDocumentDdfBd = query.AsEnumerable().FirstOrDefault();
                _formproperties.GlobalRecordID = surveyDatadFromDocumentDdfBd.GlobalRecordID;
                _formproperties.FirstSaveTime = surveyDatadFromDocumentDdfBd.FirstSaveTime;
                _formproperties.LastSaveTime = surveyDatadFromDocumentDdfBd.LastSaveTime;
                _formproperties.RelateParentId= surveyDatadFromDocumentDdfBd.RelateParentId;
                return _formproperties;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        #endregion

        #endregion
    }
}
