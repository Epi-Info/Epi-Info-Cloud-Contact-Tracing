
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
    //class Family : Resource
    //{
    //    public string FamilyName { get; set; }
    //    public string lastname { get; set; }
    //}
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
            //string serviceEndpoint = "https://epicloud.documents.azure.com:443/";// ConfigurationManager.AppSettings["serviceEndpoint"];
            //string authKey = "mS47A4aocluPX5RjrMvcDjKlvcRiGUTF9pJGEKeCSUrFHfdc6GfX7gTvnbiFmX8Oqu9R1fG2ryowRC4rCJ3Rkg==";// ConfigurationManager.AppSettings["authKey"];

            string serviceEndpoint = "https://epiclouddev.documents.azure.com:443/";// ConfigurationManager.AppSettings["serviceEndpoint"];
            string authKey = "9y0faG9P1x0t5L95GCdSZtcQ37K7xbZaCe2Cs1LMkImqJUxED2lXuWvKfa9onyA1Ptguw5gNnwM5uFz8lcMbWA==";// ConfigurationManager.AppSettings["authKey"];


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
                    Database database = ReadDatabase(_surveyData.SurveyName);

                    //Create Survey Properties 
                    DocumentCollection collection = ReadCollection(database.SelfLink, _surveyData.SurveyName);

                    RequestOptions _Requestoption = null;

                    //If any opertaion first upsert table properties
                    var surveyProperties = CreateSurveyDocumentPropertiesAsync(collection, _surveyData.SurveyProperties, _Requestoption, _surveyData.SurveyName);

                    //Create collection and store data 
                    collection = ReadCollection(database.SelfLink, _surveyData.SurveyName + "_" + _surveyData.SurveyProperties.PageId);

                    var surveyPage = CreateDocumentsAsync(collection.SelfLink, _surveyData.SurveyQuestionandAnswer, _Requestoption); 
                }
                catch (Exception ex)
                {

                }
            }

            return true;
        }
        #endregion

      
        #region ReadOrCreateDabase
        /// <summary>
        ///If DB is not avaliable in Document Db create DB
        /// </summary>
        private Database ReadDatabase(string databaseName)
        {
            Database db = client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
            if (db == null)
            {
                var dbresponse = CreateDatabaseAsync(databaseName);
                return dbresponse.Result;
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
        private async Task<ResourceResponse<Database>>  CreateDatabaseAsync(string databaseName)
        {
            var dbresponse = await client.CreateDatabaseAsync(new Database { Id = databaseName }, null);
            return Task.FromResult(dbresponse).Result; 
        }
        #endregion

        #region ReadCollection 
        /// <summary>
        /// Read or CreateCollection in Document DB
        /// </summary>
        private DocumentCollection ReadCollection(string databaseLink, string collectionId)
        {
            var Doccol = client.CreateDocumentCollectionQuery(databaseLink).Where(c => c.Id == collectionId).AsEnumerable().FirstOrDefault();
            if (Doccol == null)
            {
                var collectionResponse = CreateCollection(databaseLink, collectionId).Result;
                return collectionResponse;
            }
            else
            {
                return Doccol;
            } 
        }
        #endregion 

        #region ReadORCreateCollection
        /// <summary>
        /// Read or CreateCollection in Document DB
        /// </summary>
        private async Task<ResourceResponse<DocumentCollection>> CreateCollection(string databaseLink, string collectionId)
        {
            // Use UriFactory to build the DocumentLink 
            var collectionResponse = await client.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = collectionId }, null);
            return Task.FromResult(collectionResponse).Result;
        }
        #endregion

        #region CreateOrUpdateDocuments

        /// <summary>
        /// Get the collection and conver to json and send to document db
        /// </summary>
        /// <param name="collectionLink"></param>
        private async Task<ResourceResponse<Document>> CreateDocumentsAsync(string collectionLink, SurveyQuestionandAnswer surveyData, RequestOptions requestoption)
        {
            
            Document docs = new Document();
            try
            {
                surveyData.Id = surveyData.GlobalRecordID;

               // Family testfamil = new Family { FamilyName = "muthu", lastname = "Raja",Id="124578" };
                var documentTasks = await client.UpsertDocumentAsync(collectionLink, surveyData);
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
            
            Document docs = new Document();
            try
            {
                surveyProperties.Id = surveyProperties.GlobalRecordID;
                var _responseDB = ReadSurveyDataPropertiesFromDocumentDB(surveyProperties, SurveyName, collection);
                if (_responseDB == null)
                {
                    //Family testfamil = new Family { FamilyName = "muthu", lastname = "Raja",Id= "12345756" };
                    var documentTasks = await client.CreateDocumentAsync(collection.SelfLink, surveyProperties);
                    var _StatusCode = ((ResourceResponse<Document>)documentTasks).StatusCode;
                }
                else
                {
                    surveyProperties.DateCreated = _responseDB.DateCreated; 
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
        private SurveyQuestionandAnswer ReadSurveyDataFromDocumentDB(string dbname,string collectionName, string responseId,DocumentCollection collection)
        {
            // Use UriFactory to build the DocumentLink
            Uri docUri = UriFactory.CreateDocumentCollectionUri(dbname, collectionName);

            SurveyQuestionandAnswer surveyData = new SurveyQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = client.CreateDocumentQuery(docUri, "SELECT " + collectionName + ".GlobalRecordID," + collectionName + ".PageId," + collectionName + ".SurveyQAList FROM   " + collectionName + " WHERE " + collectionName + ".id = '" + responseId + "'", queryOptions);
                var _surveyDataFromDocumentDB1 = query.AsEnumerable().FirstOrDefault();


                var _surveyDataFromDocumentDB = (SurveyQuestionandAnswer)query.AsEnumerable().FirstOrDefault();

                surveyData.SurveyQAList = _surveyDataFromDocumentDB.SurveyQAList;

                return surveyData;
            }
            catch(DocumentQueryException ex)
            {
                Console.WriteLine(ex.ToString());
            }


            return null;

        }
        #endregion

        #region ReadDataFromCollectionDocumentDB
        private SurveyProperties ReadSurveyDataPropertiesFromDocumentDB(SurveyProperties SurveyProperties, string surveyName, DocumentCollection collection)
        {
             

            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            var query = this.client.CreateDocumentQuery(collection.SelfLink, "SELECT " + surveyName + ".DateCreated," + surveyName + "._self from " + surveyName + " WHERE " + surveyName + ".GlobalRecordID = '" + SurveyProperties.GlobalRecordID + "'" + " and " + surveyName + ".SurveyID ='" + SurveyProperties.SurveyID + "'", queryOptions);
            var _surveyDataFromDocumentDB = (SurveyProperties)query.AsEnumerable().FirstOrDefault();
            return _surveyDataFromDocumentDB;
        }
        #endregion

        #region ReadSurveyFromDocumentDBByResponseId,PAgeId
        public SurveyQuestionandAnswer ReadSruveyFromDocumentDBByPageandRespondId(string dbName, string responseId, string pageId)
        {
            string serviceEndpoint = "https://epiclouddev.documents.azure.com:443/";// ConfigurationManager.AppSettings["serviceEndpoint"];
            string authKey = "9y0faG9P1x0t5L95GCdSZtcQ37K7xbZaCe2Cs1LMkImqJUxED2lXuWvKfa9onyA1Ptguw5gNnwM5uFz8lcMbWA==";// ConfigurationManager.AppSettings["authKey"];


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
                    Database database = ReadDatabase(dbName);

                    //Read Collection
                    DocumentCollection collection = ReadCollection(database.SelfLink, dbName);


                    //Read collection and store data  
                    _surveyAnswer = ReadSurveyDataFromDocumentDB(dbName, dbName + "_" + pageId, responseId,collection);
                    return _surveyAnswer;
                }
                catch (DocumentQueryException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return _surveyAnswer;
        }
        #endregion
    }
}


