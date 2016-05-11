
using Epi.Cloud.DataEntryServices.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Epi.Cloud.DataEntryServices
{
    public class CRUDSurveyResponse
    {
        static DocumentClient client;
        /// <summary>
        /// Created instance of DocumentClient and Getting reference to database and Document collections
        /// </summary>
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
                    Console.WriteLine("1. Create an instance of DocumentClient");
                    using (client = new DocumentClient(new Uri(serviceEndpoint), authKey))
                    {
                        // 2.
                        Console.WriteLine("2. Getting reference to Database");
                        //Database database = ReadOrCreateDatabase(_surveyData.SurveyName);

                        // 3.
                        Console.WriteLine("3. Getting reference to a DocumentCollection");
                       // DocumentCollection collection = ReadOrCreateCollection(database.SelfLink, _surveyData.SurveyName);

                        // 4. 
                        Console.WriteLine("4. Inserting Documents");
                        //CreateDocuments(collection.SelfLink, _surveyData);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return true;
        }

        #region ReadOrCreateDabase
        /// <summary>
        ///If DB is not avaliable in Document Db create DB
        /// </summary>
        private static Database ReadOrCreateDatabase(string databaseId)
        { 
            var db = client.CreateDatabaseQuery()
                            .Where(d => d.Id == databaseId)
                            .AsEnumerable()
                            .FirstOrDefault();

            // In case there was no database matching, go ahead and create it. 
            if (db == null)
            {
                Console.WriteLine("2. Database not found, creating");

                db = client.CreateDatabaseAsync(new Database { Id = databaseId }).Result;
            }

            return db;
        }
        #endregion

        #region ReadORCreateCollection
        /// <summary>
        /// Read or CreateCollection in Document DB
        /// </summary>
        private static DocumentCollection ReadOrCreateCollection(string databaseLink, string collectionId)
        {
            var Doccol = client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(c => c.Id == collectionId)
                              .AsEnumerable()
                              .FirstOrDefault();
            if (Doccol == null)
            {
                Console.WriteLine("3. DocumentCollection not found.");
                Console.WriteLine("3. Creating new DocumentCollection.");
                Doccol = client.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = collectionId }).Result;
            }

            Console.WriteLine("3. Creating DocumentCollection");
            return Doccol;
        }
        #endregion

        #region CreateDocuments

        /// <summary>
        /// Get the collection and conver to json and send to document db
        /// </summary>
        /// <param name="collectionLink"></param>
        private static void CreateDocuments(string collectionLink, dynamic surveyData)
        {
            // DocumentDB provides many different ways of working with documents. 
            // 1. You can create an object that extends the Document base class
            // 2. You can use any POCO whether as it is without extending the Document base class
            // 3. You can use dynamic types
            // 4. You can even work with Streams directly.
            //
            // In DocumentDB every Document must have an "id" property. If you supply one, it must be unique. 
            // If you do not supply one, DocumentDB will generate a GUID for you and add it to the Document as "id".
            // You can disable the auto generaction of ids if you prefer by setting the disableAutomaticIdGeneration option on CreateDocumentAsync method
            var task1 = client.CreateDocumentAsync(collectionLink, surveyData); 
            // Wait for the above Async operations to finish executing
            Task.WaitAll(task1);

            Console.WriteLine("4. Documents successfully created");
        }
        #endregion 

    }
}

