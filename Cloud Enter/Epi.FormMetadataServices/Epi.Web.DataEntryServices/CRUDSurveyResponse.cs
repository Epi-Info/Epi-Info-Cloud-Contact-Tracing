
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.DataEntryServices.Model;
using Epi.Web.EF;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

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
                    var surveyProperties = CreateSurveyDocumentPropertiesAsync(client, collection, surveyData.SurveyProperties, requestOption, surveyData.SurveyName);

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
                    surveyProperties.DateCreated = responseDB.DateCreated;
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

        #region ReadDataFromCollectionDocumentDB
        private SurveyQuestionandAnswer ReadSurveyDataFromDocumentDB(DocumentClient client, string dbname, string collectionName, string responseId, DocumentCollection collection)
        {
            // Use UriFactory to build the DocumentLink
            Uri docUri = UriFactory.CreateDocumentCollectionUri(dbname, collectionName);

            SurveyQuestionandAnswer surveyData = new SurveyQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = client.CreateDocumentQuery(docUri, "SELECT " + collectionName + ".GlobalRecordID," + collectionName + ".PageId," + collectionName + ".SurveyQAList FROM   " + collectionName + " WHERE " + collectionName + ".id = '" + responseId + "'", queryOptions);
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

            var query = client.CreateDocumentQuery(collection.SelfLink, "SELECT " + surveyName + ".DateCreated," + surveyName + "._self from " + surveyName + " WHERE " + surveyName + ".GlobalRecordID = '" + SurveyProperties.GlobalRecordID + "'" + " and " + surveyName + ".SurveyID ='" + SurveyProperties.SurveyID + "'", queryOptions);
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
                    Database database = await ReadDatabaseAsync(client, dbName);

                    //Read Collection
                    DocumentCollection collection = await ReadCollectionAsync(client, database.SelfLink, dbName);


                    //Read collection and store data  
                    surveyAnswer = ReadSurveyDataFromDocumentDB(client, dbName, dbName + "_" + pageId, responseId, collection);
                    return surveyAnswer;
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
        public List<SurveyResponse> ReadAllRecordsBySurveyID(string dbName, string surveyId)
        {
            List<SurveyResponse> surveyResponse = null;

                try
                {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    //Instance of DocumentClient"
                    surveyResponse = GellAllSurveyDataBySurveyId(client, dbName, surveyId);
                }

                }
                catch (DocumentQueryException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            return surveyResponse;
        }
        #endregion

        #region HelperMethod
        #region GetllAllDataBySurveyId
        #region ReadDataFromCollectionDocumentDB
        private List<SurveyResponse> GellAllSurveyDataBySurveyId(DocumentClient client, string dbname, string surveyId)
        {
            // Use UriFactory to build the DocumentLink
            string collectionName = dbname;
            Uri docUri = UriFactory.CreateDocumentCollectionUri(dbname, collectionName);

            SurveyQuestionandAnswer surveyData = new SurveyQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<SurveyResponse> surveyList = new List<SurveyResponse>();
            try
            {
                var query = client.CreateDocumentQuery(docUri, "SELECT " + collectionName + ".GlobalRecordID," + collectionName + ".SurveyID," + collectionName + ".RecStatus," + collectionName + ".PageId," + collectionName + ".PagePosition," + collectionName + ".DateOfInterview," + collectionName + ".DateCreated," + collectionName + ".DateUpdated FROM   " + collectionName + " WHERE " + collectionName + ".SurveyID = '" + surveyId + "'", queryOptions);
                var surveyDataFromDocumentDB = query.AsQueryable();
                foreach (SurveyProperties item in surveyDataFromDocumentDB)
                {
                    SurveyResponse surveyResponse = new SurveyResponse();
                    surveyResponse.ResponseId = new Guid(item.GlobalRecordID);
                    surveyResponse.SurveyId = new Guid(item.SurveyID);
                    surveyResponse.DateUpdated = item.DateUpdated;
                    surveyResponse.StatusId = item.RecStatus;
                    surveyResponse.DateCreated = item.DateCreated;
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
        #endregion

        #endregion
    }
}


