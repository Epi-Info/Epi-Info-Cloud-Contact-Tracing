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
using System.Collections;
using Newtonsoft.Json;
using Epi.Cloud.Common.EntityObjects;
using Epi.Cloud.Common.Constants;

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
        public async Task<bool> InsertToSurveyToDocumentDB(Survey surveyData)
        {
            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {

                    //Getting reference to Database 
                    Database database = await ReadDatabaseAsync(client, surveyData.SurveyName);

                    //Create Survey Properties 
                    DocumentCollection collection = await ReadCollectionAsync(client, database.SelfLink, surveyData.SurveyName);

                    RequestOptions requestOption = null;

                    //If any opertaion first upsert table properties
                    var surveyProperties = await CreateSurveyDocumentPropertiesAsync(client, collection, surveyData.SurveyProperties, requestOption, surveyData.SurveyName);

                    //Create collection and store data 
                    collection = await ReadCollectionAsync(client, database.SelfLink, surveyData.SurveyName + "_" + surveyData.SurveyProperties.PageId);

                    await CreateDocumentsAsync(client, collection.SelfLink, surveyData.SurveyQuestionandAnswer, requestOption);
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }
        #endregion

        #region ReadOrCreateDabase
        /// <summary>
        ///If DB is not avaliable in Document Db create DB
        /// </summary>
        private async Task<Database> ReadDatabaseAsync(DocumentClient client, string databaseName)
        {
            Database db = client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
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
        private async Task<ResourceResponse<Database>> CreateDatabaseAsync(DocumentClient client, string databaseName)
        {
            var dbresponse = await client.CreateDatabaseAsync(new Database { Id = databaseName }, null);
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
                documentCollection = await CreateCollectionAsync(client, databaseLink, collectionId);
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
            var collectionResponse = await client.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = collectionId }, null);
            return collectionResponse;
        }
        #endregion

        #region CreateOrUpdateDocuments

        /// <summary>
        /// Get the collection and conver to json and send to document db
        /// </summary>
        /// <param name="collectionLink"></param>
        private async Task<ResourceResponse<Document>> CreateDocumentsAsync(DocumentClient client, string collectionLink, SurveyQuestionandAnswer surveyData, RequestOptions requestoption)
        {

            Document docs = new Document();
            try
            {
                surveyData.Id = surveyData.GlobalRecordID;

                // Family testfamil = new Family { FamilyName = "muthu", lastname = "Raja",Id="124578" };
                var documentTasks = await client.UpsertDocumentAsync(collectionLink, surveyData);
                var statusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;

            }
            catch (Exception ex)
            {
                var xe = ex.ToString();
            }
            return null;
        }


        private async Task<ResourceResponse<Document>> CreateSurveyDocumentPropertiesAsync(DocumentClient client, DocumentCollection collection, SurveyProperties surveyProperties, RequestOptions requestoption, string SurveyName)
        {

            Document docs = new Document();
            try
            {
                surveyProperties.Id = surveyProperties.GlobalRecordID;
                var responseDB = ReadSurveyDataPropertiesFromDocumentDB(client, surveyProperties, SurveyName, collection);
                if (responseDB == null)
                {
                    //Family testfamil = new Family { FamilyName = "muthu", lastname = "Raja",Id= "12345756" };
                    var documentTasks = await client.CreateDocumentAsync(collection.SelfLink, surveyProperties);
                    var statusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;
                }
                else
                {
                    surveyProperties.FirstSaveTime = responseDB.FirstSaveTime;
                    var documentTasks = await client.ReplaceDocumentAsync(responseDB.SelfLink, surveyProperties);
                    var statusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;
                }
                //Document doc = client.CreateDocumentQuery(collectionLink).Where(x => x.Id == surveyProperties.GlobalRecordID).AsEnumerable().FirstOrDefault();
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
        private SurveyQuestionandAnswer ReadSurveyDataFromDocumentDB(DocumentClient client, string dbname, string collectionName, string responseId)
        {
            // Use UriFactory to build the DocumentLink
            Uri docUri = UriFactory.CreateDocumentCollectionUri(dbname, collectionName);

            SurveyQuestionandAnswer surveyData = new SurveyQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var columnList = AssembleColumnList(collectionName,
                    "GlobalRecordID",
                    "PageId",
                    "SurveyQAList");

                var query = client.CreateDocumentQuery(docUri, "SELECT "
                    + columnList
                    + " FROM " + collectionName
                    + " WHERE " + collectionName + ".id = '" + responseId + "'"
                    , queryOptions);
                //var surveyDataFromDocumentDB1 = query.AsEnumerable().FirstOrDefault();


                var surveyDataFromDocumentDB = (SurveyQuestionandAnswer)query.AsEnumerable().FirstOrDefault();

                if (surveyDataFromDocumentDB != null)
                {
                    surveyData.SurveyQAList = surveyDataFromDocumentDB.SurveyQAList;

                    return surveyData;
                }
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
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
                + " WHERE " + surveyName + ".GlobalRecordID = '" + SurveyProperties.GlobalRecordID + "'" + " and " + surveyName + ".SurveyID ='" + SurveyProperties.SurveyID + "'"
                , queryOptions);
            var surveyDataFromDocumentDB = (SurveyProperties)query.AsEnumerable().FirstOrDefault();
            return surveyDataFromDocumentDB;
        }
        #endregion

        #region ReadSurveyFromDocumentDBByResponseId,PAgeId
        public async Task<SurveyQuestionandAnswer> ReadSurveyFromDocumentDBByPageandRespondIdAsync(string dbName, string responseId, string pageId)
        {

            SurveyQuestionandAnswer surveyAnswer = new SurveyQuestionandAnswer();

            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    //Getting reference to Database
                    //Database database = await ReadDatabaseAsync(client, dbName);

                    //Read Collection
                    //DocumentCollection collection = await ReadCollectionAsync(client, database.SelfLink, dbName);

                    //DocumentCollection DocumentDbResponse =await CollectionIsExistorNotAsync(client, dbName, dbName + "_" + pageId);
                    //var DocumentDbResponse = client.ReadDocumentCollectionAsync(docUri).Result;
                    //var statusCode = DocumentDbResponse.SelfLink;
                    if (true)
                    {
                        //Read collection and store data  
                        surveyAnswer = ReadSurveyDataFromDocumentDB(client, dbName, dbName + "_" + pageId, responseId);
                        return surveyAnswer;
                    }
                }
            }
            catch (DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return surveyAnswer;
        }
        #endregion

        #region ReadAllRecordsBySurveyID 
        public List<SurveyResponse> ReadAllRecordsBySurveyID(string dbName, string surveyId, List<string> Params, string pageId)
        {
            string collectionName = dbName + "_" + pageId;
            List<SurveyResponse> surveyResponse = null;

            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    //Instance of DocumentClient"
                    surveyResponse = GellAllSurveyDataBySurveyId(client, dbName, surveyId, Params, collectionName);
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
        public async Task<string> DeleteDocumentByIdAsync(Survey surveyInfo)
        {
            SurveyProperties surveyPropertie = new SurveyProperties();
            using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
            {
                Uri docUri = UriFactory.CreateDocumentUri(surveyInfo.SurveyName, surveyInfo.SurveyName, surveyInfo.SurveyProperties.GlobalRecordID);

                Document docs = new Document();
                string Query = "SELECT " + surveyInfo.SurveyName + ".DateCreated," + surveyInfo.SurveyName + ".SurveyID," + surveyInfo.SurveyName + ".GlobalRecordID," + surveyInfo.SurveyName + ".RecStatus," + surveyInfo.SurveyName + "._self from " + surveyInfo.SurveyName + " WHERE " + surveyInfo.SurveyName + ".GlobalRecordID = '" + surveyInfo.SurveyProperties.GlobalRecordID + "'";
                try
                {
                    //var responseDB = ReadDataFromDocumentDB(client,surveyInfo.SurveyName, Query, docUri);
                    surveyPropertie.FirstSaveTime = surveyInfo.SurveyProperties.FirstSaveTime;
                    surveyPropertie.LastSaveTime = DateTime.UtcNow;
                    surveyPropertie.RecStatus = RecordStatus.Deleted;
                    surveyPropertie.Id = surveyInfo.SurveyProperties.GlobalRecordID;
                    surveyPropertie.GlobalRecordID = surveyInfo.SurveyProperties.GlobalRecordID;
                    surveyPropertie.SurveyID = surveyInfo.SurveyProperties.SurveyID;
                    var documentTasks = await client.ReplaceDocumentAsync(docUri, surveyPropertie);
                    var statusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;
                }
                catch (Exception ex)
                {
                    var xe = ex.ToString();
                }
                return null;
            }

        }
        #endregion


        #region HelperMethod
        #region GetllAllDataBySurveyId
        #region ReadDataFromCollectionDocumentDB
        private List<SurveyResponse> GellAllSurveyDataBySurveyId(DocumentClient client, string dbname, string surveyId, List<string> DocumentDBParameter, string collectionName)
        {
            Uri docUri = UriFactory.CreateDocumentCollectionUri(dbname, collectionName);

            SurveyQuestionandAnswer surveyData = new SurveyQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                string PerameterString = string.Empty;
                foreach (string perameter in DocumentDBParameter)
                {
                    PerameterString += collectionName + ".SurveyQAList." + perameter + ",";
                }
                PerameterString = PerameterString.TrimEnd(',');

                IQueryable<SurveyProperties> surveyInfo = client.CreateDocumentQuery<SurveyProperties>(
                                                    UriFactory.CreateDocumentCollectionUri(dbname, dbname), queryOptions)
                                                    .Where(f => f.SurveyID == surveyId && f.RecStatus == RecordStatus.InProcess);
                foreach (var survey in surveyInfo)
                {
                    var query = client.CreateDocumentQuery(docUri, "SELECT " + collectionName + ".GlobalRecordID," + PerameterString + " FROM  " + collectionName + " WHERE " + collectionName + ".GlobalRecordID = '" + survey.GlobalRecordID + "'", queryOptions);
                    //var query = client.CreateDocumentQuery(docUri, "SELECT " + collectionName + ".GlobalRecordID," + PerameterString + " FROM  " + collectionName + " WHERE " + collectionName + ".SurveyID = '" + surveyId + "'", queryOptions);

                    var surveyDataFromDocumentDB = query.AsQueryable();

                    foreach (var items in surveyDataFromDocumentDB)
                    {
                        SurveyResponse surveyResponse = new SurveyResponse();
                        var json = JsonConvert.SerializeObject(items);
                        Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);


                        surveyResponse.ResponseId = new Guid(items.GlobalRecordID);
                        surveyResponse.SurveyId = new Guid(surveyId);
                        surveyResponse.StatusId = 1;

                        surveyResponse.ResponseQA = new Dictionary<string, string>();
                        foreach (var column in values)
                        {
                            surveyResponse.ResponseQA.Add(column.Key, column.Value);
                        }
                        surveyList.Add(surveyResponse);
                    }
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

        #endregion
    }
}


