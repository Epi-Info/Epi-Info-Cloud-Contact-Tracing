
using Epi.Web.DataEntryServices.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Epi.Web.DataEntryServices
{
    public class CRUDSurveyResponse
    {
        internal CloudTable cloudtable;

        #region Store the Survey data in Table Storage
        public bool Storeservey(SurveyEntity surveydata)
        {
            bool response;

            //var testreterive = ReteriveSurvey(surveydata);

            //Convert Question and Answer to Json 
            surveydata.SurveyData = JsonConvert.SerializeObject(surveydata.SurveyTableEntity);

            // Create or reference an existing table            
            response = TableStorageInfo.CreateTableAsync(surveydata.SurveyName);

            var tableclient = TableStorageInfo.TableClient();
            cloudtable = tableclient.GetTableReference(surveydata.SurveyName);


            bool respo = BatchInsertInToTableAsync(cloudtable, surveydata);
            return response;
        }
        #endregion

        #region Get the Azure Table Storage references
        public CloudTable Tablereference(string tableName)
        {
            var tableclient = TableStorageInfo.TableClient();
            cloudtable = tableclient.GetTableReference(tableName);
            return cloudtable;
        }
        #endregion

        #region Insert value in to the TableStorage
        public bool BatchInsertInToTableAsync(CloudTable table, SurveyEntity entity)
        {

            SurveyEntity surveyEntityObj = new SurveyEntity();
            try
            {
                // Create the InsertOrReplace  TableOperation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = table.Execute(insertOrMergeOperation);
                SurveyEntity insertedCustomer = result.Result as SurveyEntity;
                string strSurveyData = insertedCustomer.SurveyData;

                surveyEntityObj = new SurveyEntity();
                surveyEntityObj.SurveyTableEntity = new List<SurveyTableEntity>();
                surveyEntityObj.SurveyTableEntity = JsonConvert.DeserializeObject<List<SurveyTableEntity>>(strSurveyData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return true;
        }
        #endregion

        //#region Read data from TableStorage based on SurveyId and ResponseID to get particular record
        //public SurveyEntity RetrieveSurveyInfoFromTableStorage(SurveyEntity entity)
        //{
        //    SurveyEntity survey =new SurveyEntity();
        //    SurveyEntity surveyEntityObj = new SurveyEntity();

        //    // Create or reference an existing table             
        //    var tableclient = TableStorageInfo.TableClient();
        //    cloudtable = tableclient.GetTableReference(entity.SurveyName);


        //    TableOperation retrieveOperation = TableOperation.Retrieve<SurveyEntity>(entity.PartitionKey, entity.RowKey); 
        //    TableResult response = cloudtable.Execute(retrieveOperation);

        //    var strSurveyData = ((SurveyEntity)response.Result).SurveyData;            
        //    surveyEntityObj = new SurveyEntity();
        //    surveyEntityObj = JsonConvert.DeserializeObject<SurveyEntity>(strSurveyData);
        //    return survey;
        //}
        //#endregion

        #region Read data from TableStorage based on   Survey Id
        public List<SurveyEntity> RetrieveFromTableStorageBySurveyId(string surveyId, string tableName)
        {
            SurveyEntity survey = new SurveyEntity();

            // Create or reference an existing table             
            var tableclient = TableStorageInfo.TableClient();
            cloudtable = tableclient.GetTableReference(tableName);

            IQueryable<SurveyEntity> surveysdf = cloudtable.CreateQuery<SurveyEntity>().Where(pKy => pKy.PartitionKey == surveyId);
            List<SurveyEntity> surveytest = surveysdf.ToList();
            return surveytest;
        }
        #endregion 

        //Testing
        private static async Task<TableResult> BatchInsertOfCustomerEntitiesAsync(CloudTable table)
        {
            // Create an instance of a customer entity. See the Model\CustomerEntity.cs for a description of the entity.
            SurveyEntity customer = new SurveyEntity("Harp", "Walter")
            {
                SurveyData = "Walter@contoso.com"
            };

            // Demonstrate how to Update the entity by changing the phone number

            // Create the InsertOrReplace  TableOperation
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(customer);
            TableResult result = null;


            try
            {
                // Execute the operation.
                result = await table.ExecuteAsync(insertOrMergeOperation);
            }
            catch (StorageException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
            // Execute the batch operation.
        }

    }
}

