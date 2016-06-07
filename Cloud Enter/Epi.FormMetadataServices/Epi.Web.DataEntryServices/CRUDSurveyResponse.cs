
using Epi.Cloud.DataEntryServices.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Epi.Cloud.DataEntryServices
{
    public class CRUDSurveyResponse
    {

        public DocumentClient client;


        #region InsertToSruveyToDocumentDB
        /// <summary>
        /// Created instance of DocumentClient and Getting reference to database and Document collections
        /// </summary>
        ///
        public bool InsertToSruveyToDocumentDB(Survey _surveyData)
        {
            string serviceEndpoint = "https://epicloud.documents.azure.com:443/";// ConfigurationManager.AppSettings["serviceEndpoint"];
            string authKey = "mS47A4aocluPX5RjrMvcDjKlvcRiGUTF9pJGEKeCSUrFHfdc6GfX7gTvnbiFmX8Oqu9R1fG2ryowRC4rCJ3Rkg==";// ConfigurationManager.AppSettings["authKey"];

            if (string.IsNullOrWhiteSpace(serviceEndpoint) || string.IsNullOrWhiteSpace(authKey))
            {
                //Please provide  service endpoint and authkey
            }

            else
            {
                try
                {
                    //Instance of DocumentClient"
                    client = new DocumentClient(new Uri(serviceEndpoint), authKey);

                    //var test = CreateDatabase(client, _surveyData.SurveyName);

                    //Getting reference to Database
                    //Database database = ReadDatabase(_surveyData.SurveyName).Result;
                    Database database = ReadOrCreateDatabase(_surveyData.SurveyName);

                    //Create Survey Properties 
                    DocumentCollection collection = ReadOrCreateCollection(database.SelfLink, _surveyData.SurveyName);

                    RequestOptions _Requestoption = null;

                    //If any opertaion first upsert table properties
                    var surveyProperties = CreateSurveyDocumentPropertiesAsync(collection, _surveyData.SurveyProperties, _Requestoption, _surveyData.SurveyName);

                    //Create collection and store data 
                    collection = ReadOrCreateCollection(database.SelfLink, _surveyData.SurveyName + "_" + _surveyData.SurveyProperties.PageId);

                    var surveyPage = CreateDocumentsAsync(collection.SelfLink, _surveyData.SurveyQuestionandAnswer, _Requestoption);
                }
                catch (Exception ex)
                {

                }
            }

            return true;
        }
        #endregion

        #region ReadSurveyFromDocumentDB 
        public SurveyQuestionandAnswer ReadSruveyFromDocumentDB(SurveyProperties _surveyData, string SurveyName)
        {
            string serviceEndpoint = "https://epicloud.documents.azure.com:443/";// ConfigurationManager.AppSettings["serviceEndpoint"];
            string authKey = "mS47A4aocluPX5RjrMvcDjKlvcRiGUTF9pJGEKeCSUrFHfdc6GfX7gTvnbiFmX8Oqu9R1fG2ryowRC4rCJ3Rkg==";// ConfigurationManager.AppSettings["authKey"];

            SurveyQuestionandAnswer _surveyAnswer = new SurveyQuestionandAnswer();


            if (string.IsNullOrWhiteSpace(serviceEndpoint) || string.IsNullOrWhiteSpace(authKey))
            {
                //Please provide  service endpoint and authkey
            }

            else
            {
                try
                {
                    //Instance of DocumentClient"
                    client = new DocumentClient(new Uri(serviceEndpoint), authKey);

                    //Getting reference to Database
                    Database database = null;// ReadDatabase(SurveyName);

                    //Read Collection
                    DocumentCollection collection = ReadCollection(database.SelfLink, SurveyName);

                    //Read collection and store data  
                    //_surveyAnswer=ReadSurveyDataFromDocumentDB(_surveyData, collection);
                    return _surveyAnswer;
                }
                catch (Exception ex)
                {

                }
            }
            return _surveyAnswer;
        }
        #endregion

        #region ReadSurveyFromDocumentDBByResponseId,PAgeId
        public SurveyQuestionandAnswer ReadSruveyFromDocumentDBByPageandRespondId(string dbName, string responseId, string pageId)
        {
            string serviceEndpoint = "https://epicloud.documents.azure.com:443/";// ConfigurationManager.AppSettings["serviceEndpoint"];
            string authKey = "mS47A4aocluPX5RjrMvcDjKlvcRiGUTF9pJGEKeCSUrFHfdc6GfX7gTvnbiFmX8Oqu9R1fG2ryowRC4rCJ3Rkg==";// ConfigurationManager.AppSettings["authKey"];

            SurveyQuestionandAnswer _surveyAnswer = new SurveyQuestionandAnswer();

            if (string.IsNullOrWhiteSpace(serviceEndpoint) || string.IsNullOrWhiteSpace(authKey))
            {
                //Please provide  service endpoint and authkey
            }

            else
            {
                try
                {
                    //Instance of DocumentClient"
                    this.client = new DocumentClient(new Uri(serviceEndpoint), authKey);

                    //Getting reference to Database
                    //var documentdbResponse= ReadDatabase(dbName);
                    //Database database = documentdbResponse.Result;
                    Database database = ReadOrCreateDatabase(dbName);

                    //Read Collection 
                    var collectionName = dbName + "_" + pageId;
                    DocumentCollection collection = ReadCollection(database.SelfLink, collectionName);

                    //Read collection and store data  
                    _surveyAnswer = ReadSurveyDataFromDocumentDB(collectionName, responseId, pageId, collection);
                    return _surveyAnswer;
                }
                catch (Exception ex)
                {

                }
            }
            return _surveyAnswer;
        }
        #endregion

        #region ReadOrCreateDabase
        /// <summary>
        ///If DB is not avaliable in Document Db create DB
        /// </summary>
        private Database ReadOrCreateDatabase(string databaseId)
        {

            Database db = client.CreateDatabaseQuery().Where(d => d.Id == databaseId).AsEnumerable().FirstOrDefault();
            try
            {
                // In case there was no database matching, go ahead and create it. 
                if (db == null)
                {
                    Console.WriteLine("2. Database not found, creating");

                    db = client.CreateDatabaseAsync(new Database { Id = databaseId }).Result;
                    return db;
                }
            }
            catch (Exception ex)
            {

            }

            return db;


        }


        #endregion

        #region ReadDatabase 
        /// <summary>
        ///If DB is not avaliable in Document Db create DB
        /// </summary>
        private async Task<ResourceResponse<Database>> ReadorCreateDatabase(string databaseName)
        {

            var Response = await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            if (Response != null)
            {
                return Task.FromResult(Response).Result;
            }
            else
            {
                try
                {
                    var dbresponse = await client.CreateDatabaseAsync(new Database { Id = databaseName });
                    return Task.FromResult(dbresponse).Result;
                }
                catch (DocumentClientException de)
                {
                    // If the database does not exist, create a new database
                    if (de.StatusCode == HttpStatusCode.NotFound)
                    {
                        //await this.client.CreateDatabaseAsync(new Database { Id = databaseName }); 
                        //"Database not found";
                    }
                    else
                    {
                        throw;
                    }
                }

            }

            return null;
        }
        #endregion

        #region ReadCollection 
        /// <summary>
        /// Read or CreateCollection in Document DB
        /// </summary>
        private DocumentCollection ReadCollection(string databaseLink, string collectionId)
        {
            var Doccol = client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(c => c.Id == collectionId)
                              .AsEnumerable()
                              .FirstOrDefault();
            if (Doccol == null)
            {
                Console.WriteLine("Collection Not exist");
            }
            return Doccol;
        }
        #endregion 

        #region ReadORCreateCollection
        /// <summary>
        /// Read or CreateCollection in Document DB
        /// </summary>
        private DocumentCollection ReadOrCreateCollection(string databaseLink, string collectionId)
        {
            var Doccol = client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(c => c.Id == collectionId)
                              .AsEnumerable()
                              .FirstOrDefault();
            if (Doccol == null)
            {
                Doccol = client.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = collectionId }).Result;
            }
            return Doccol;
        }
        #endregion

        #region CreateOrUpdateDocuments

        /// <summary>
        /// Get the collection and conver to json and send to document db
        /// </summary>
        /// <param name="collectionLink"></param>
        private async Task<ResourceResponse<Document>> CreateDocumentsAsync(string collectionLink, dynamic surveyData, RequestOptions requestoption)
        {
            bool disableAutomaticIdGeneration = true;
            Document docs = new Document();
            try
            {
                surveyData.Id = surveyData.GlobalRecordID;
                var documentTasks = await client.UpsertDocumentAsync(collectionLink, surveyData, requestoption, disableAutomaticIdGeneration);
                var _StatusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;

            }
            catch (Exception ex)
            {
                var xe = ex.ToString();
            }
            return null;
        }


        private async Task<ResourceResponse<Document>> CreateSurveyDocumentPropertiesAsync(DocumentCollection collection, SurveyProperties surveyProperties, RequestOptions requestoption, string SurveyName)
        {
            bool disableAutomaticIdGeneration = true;
            Document docs = new Document();
            try
            {
                surveyProperties.Id = surveyProperties.GlobalRecordID;
                var _responseDB = ReadSurveyDataPropertiesFromDocumentDB(surveyProperties, SurveyName, collection);
                if (_responseDB == null)
                {
                    var documentTasks = await client.CreateDocumentAsync(collection.SelfLink, surveyProperties, requestoption, disableAutomaticIdGeneration);
                    var _StatusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;
                }
                else
                {
                    surveyProperties.DateCreated = _responseDB.DateCreated;
                    surveyProperties.DateUpdated = surveyProperties.DateUpdated;
                    var documentTasks = await client.ReplaceDocumentAsync(_responseDB.SelfLink, surveyProperties);
                    var _StatusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;
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
        private SurveyQuestionandAnswer ReadSurveyDataFromDocumentDB(string collectionName, string responseId, string pageId, DocumentCollection collection)
        {

            SurveyQuestionandAnswer surveyData = new SurveyQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var query = this.client.CreateDocumentQuery(collection.SelfLink, "SELECT " + collectionName + ".GlobalRecordID," + collectionName + ".PageId," + collectionName + ".SurveyQAList FROM   " + collectionName + " WHERE " + collectionName + ".GlobalRecordID = '" + responseId + "'", queryOptions);
            var _surveyDataFromDocumentDB1 = query.AsEnumerable().FirstOrDefault();

            var _surveyDataFromDocumentDB = (SurveyQuestionandAnswer)query.AsEnumerable().FirstOrDefault();

            surveyData.SurveyQAList = _surveyDataFromDocumentDB.SurveyQAList;

            return surveyData;

        }
        #endregion

        #region ReadDataFromCollectionDocumentDB
        private SurveyProperties ReadSurveyDataPropertiesFromDocumentDB(SurveyProperties SurveyProperties, string surveyName, DocumentCollection collection)
        {

            SurveyQuestionandAnswer surveyData = new SurveyQuestionandAnswer();

            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var query = this.client.CreateDocumentQuery(collection.SelfLink, "SELECT " + surveyName + ".DateCreated," + surveyName + "._self from " + surveyName + " WHERE " + surveyName + ".GlobalRecordID = '" + SurveyProperties.GlobalRecordID + "'" + " and " + surveyName + ".SurveyID ='" + SurveyProperties.SurveyID + "'", queryOptions);
            var _surveyDataFromDocumentDB = (SurveyProperties)query.AsEnumerable().FirstOrDefault();
            return _surveyDataFromDocumentDB;
        }
        #endregion
    }
}


