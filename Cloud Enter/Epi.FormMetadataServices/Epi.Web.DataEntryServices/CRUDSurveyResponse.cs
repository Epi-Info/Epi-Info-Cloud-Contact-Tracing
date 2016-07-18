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
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;

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
        public async Task<bool> InsertToSurveyToDocumentDBAsync(Survey surveyData)
        {

            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    var asyncTask = Task.Run(() =>
                    {
                        //Getting reference to Database 
                        Microsoft.Azure.Documents.Database database = ReadDatabaseAsync(client, surveyData.SurveyName).Result;

                        //Create Survey Properties 
                        DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, surveyData.SurveyName).Result;

                        RequestOptions requestOption = null;

                        //If any opertaion first upsert table properties
                        //var surveyProperties = CreateSurveyDocumentPropertiesAsync(client, collection, surveyData.SurveyProperties, requestOption, surveyData.SurveyName).Result;

                        Uri docUri = UriFactory.CreateDocumentCollectionUri(surveyData.SurveyName, surveyData.SurveyName);

                        //Read Surveyinfo from document db
                        var DocumentDBResponse = ReadSurveyInfoByResponseIdandSurveyId(client, docUri, surveyData.SurveyName, surveyData.SurveyProperties.GlobalRecordID);

                        if (DocumentDBResponse.Digest != null)
                        {
                            List<Digest> _digest = surveyData.SurveyQuestionandAnswer.Digest;
                            surveyData.SurveyQuestionandAnswer = MappingSurveyDataToDocumentDb(DocumentDBResponse, _digest);
                        }
                        else
                        {
                            surveyData.SurveyQuestionandAnswer.ProjectDigest = DocumentDBResponse.ProjectDigest;
                            surveyData.SurveyQuestionandAnswer.GlobalRecordID = DocumentDBResponse.GlobalRecordID;
                            surveyData.SurveyQuestionandAnswer.SurveyID = DocumentDBResponse.SurveyID;
                            surveyData.SurveyQuestionandAnswer.Id = DocumentDBResponse.Id;
                        }
                        var documentTasks = client.ReplaceDocumentAsync(DocumentDBResponse.SelfLink, surveyData.SurveyQuestionandAnswer).Result;

                    });

                    asyncTask.Wait();
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }
        #endregion

        #region SaveQuestionInDocumentDB
        public async Task<bool> SaveSurveyQuestionInDocumentDBAsync(Survey surveyData)
        {

            try
            {
                //Instance of DocumentClient"
                using (var client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                {
                    var asyncTask = Task.Run(() =>
                     {
                         //Getting reference to Database 
                         Microsoft.Azure.Documents.Database database = ReadDatabaseAsync(client, surveyData.SurveyName).Result;

                         //Create Survey Properties 
                         DocumentCollection collection = ReadCollectionAsync(client, database.SelfLink, surveyData.SurveyName).Result;

                         //Verify Response Id is exist or Not
                         var DocumentDBResponse = ResponseIdExistOrNot(client, collection, surveyData.SurveyName, surveyData.SurveyQuestionandAnswer.GlobalRecordID);
                         Uri docUri = UriFactory.CreateDocumentCollectionUri(surveyData.SurveyName, surveyData.SurveyName);
                         if (DocumentDBResponse == null)
                         {
                             //If any opertaion first Insert table properties
                             // var surveyProperties = client.CreateDocumentAsync(docUri, surveyData.SurveyProperties).Result;

                             docUri = UriFactory.CreateDocumentCollectionUri(surveyData.SurveyName, surveyData.SurveyName);
                             // Use UriFactory to build the DocumentLink
                             var surveyPage = client.CreateDocumentAsync(docUri, surveyData.SurveyQuestionandAnswer).Result;
                         }
                     });

                    asyncTask.Wait();
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }
        #endregion

        #region MappingSurveyDataToDocumentDb
        public SurveyQuestionandAnswer MappingSurveyDataToDocumentDb(SurveyQuestionandAnswer DocumentDBSurveyMetaPages, List<Digest> UIDigest)
        {
            SurveyQuestionandAnswer SurveyQA = new SurveyQuestionandAnswer();

            SurveyQA.ProjectDigest = DocumentDBSurveyMetaPages.ProjectDigest;


            //Get digest value from UI

            Dictionary<string, string> FieldsQA = new Dictionary<string, string>();
            Digest DigestQA = new Digest();
            Digest ResponseDigest = new Digest();
            SurveyQA.Digest = new List<Digest>();
            //var DocumentResponse = DocumentDBSurveyMetaPages.Digest;
            foreach (var item in DocumentDBSurveyMetaPages.Digest)
            {
                if (item.PageId == UIDigest.FirstOrDefault().PageId)
                {
                    SurveyQA.Digest.Add(UIDigest.FirstOrDefault());
                }
                else
                {
                    SurveyQA.Digest.Add(item);
                }
            }

            if (SurveyQA.Digest.Find(e => e.PageId == UIDigest.FirstOrDefault().PageId) == null)
            {
                SurveyQA.Digest.Add(UIDigest.FirstOrDefault());
            }
            //SurveyQA.Digest = DocumentResponse;
            SurveyQA.GlobalRecordID = DocumentDBSurveyMetaPages.GlobalRecordID;
            SurveyQA.SurveyID = DocumentDBSurveyMetaPages.SurveyID;
            SurveyQA.Id = DocumentDBSurveyMetaPages.Id;
            return SurveyQA;

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
        private Document CreateDocumentsAsync(DocumentClient client, string collectionLink, SurveyQuestionandAnswer surveyData, RequestOptions requestoption)
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
        private SurveyQuestionandAnswer ReadSurveyDataFromDocumentDB(DocumentClient client, string dbname, string collectionName, string responseId)
        {
            // Use UriFactory to build the DocumentLink
            Uri docUri = UriFactory.CreateDocumentCollectionUri(dbname, collectionName);

            SurveyQuestionandAnswer surveyData = new SurveyQuestionandAnswer();
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = client.CreateDocumentQuery(docUri, "SELECT  " + collectionName + ".Digest "
                    + " FROM " + collectionName
                    + " WHERE " + collectionName + ".GlobalRecordID = '" + responseId + "'"
                    , queryOptions);
                //var surveyDataFromDocumentDB1 = query.AsEnumerable().FirstOrDefault();                

                var surveyDataFromDocumentDB = (SurveyQuestionandAnswer)query.AsEnumerable().FirstOrDefault();

                if (surveyDataFromDocumentDB != null && surveyDataFromDocumentDB.Digest != null)
                {
                    surveyData.SurveyQAList = surveyDataFromDocumentDB.Digest.FirstOrDefault().Fields;

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
                        surveyAnswer = ReadSurveyDataFromDocumentDB(client, dbName, dbName, responseId);
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
            string collectionName = dbName;
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
                string ParameterString = string.Empty;
                foreach (string parameter in DocumentDBParameter)
                {
                    ParameterString += collectionName + ".Fields." + parameter + ",";
                }
                ParameterString = ParameterString.TrimEnd(',');

                //IQueryable<SurveyProperties> surveyInfo = client.CreateDocumentQuery<SurveyProperties>(
                //                                    UriFactory.CreateDocumentCollectionUri(dbname, dbname), queryOptions)
                //                                    .Where(f => f.SurveyID == surveyId && f.RecStatus == RecordStatus.InProcess);

                var query = client.CreateDocumentQuery(docUri, "SELECT " + ParameterString + " FROM  " + collectionName + " IN Families.Digest WHERE " + collectionName + ".FormId = '" + surveyId + "'", queryOptions);

                var surveyDataFromDocumentDB = query.AsQueryable();

                foreach (var items in surveyDataFromDocumentDB)
                {
                    SurveyResponse surveyResponse = new SurveyResponse();
                    var json = JsonConvert.SerializeObject(items);
                    surveyResponse.ResponseQA = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

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
              "_self");
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
        public SurveyQuestionandAnswer ReadSurveyInfoByResponseIdandSurveyId(DocumentClient client, Uri DocUri, string SurveyName, string ResponseId)
        {
            try
            {
                SurveyQuestionandAnswer ResponseDocumentDB = new SurveyQuestionandAnswer();

                // Set some common query options
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                var query = client.CreateDocumentQuery(DocUri, "SELECT " + SurveyName
                    + ".id," + SurveyName
                    + ".ProjectDigest," + SurveyName
                    + ".GlobalRecordID," + SurveyName
                    + ".SurveyID," + SurveyName
                    + "._self," + SurveyName
                    + ".Digest from " + SurveyName
                    + " WHERE " + SurveyName + ".GlobalRecordID = '" + ResponseId + "'", queryOptions);

                var surveyDatadFromDocumentDdfB = query.AsEnumerable();


                foreach (var item in surveyDatadFromDocumentDdfB)
                {
                    var DocumentDbDigest = (SurveyQuestionandAnswer)item;
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

        #endregion
    }
}
