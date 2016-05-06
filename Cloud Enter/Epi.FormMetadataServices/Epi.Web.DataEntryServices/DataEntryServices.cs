using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Epi.Web.DataEntryServices
{
    public class TableStorageInfo
    {
        string tblname = "SurveyTable";
        // CloudTable table = CreateTableAsync(tblname).Result;

        /// <summary>
        ///  If table is not exist then create new table in Table Storage
        /// </summary>
        /// <returns>A CloudTable object</returns>
        public static bool CreateTableAsync(string tableName)
        {
            bool response;
            // Create a table client for interacting with the table service
            CloudTableClient tableClient = TableClient();

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            try
            {
                response = table.CreateIfNotExists();
                // response = await table.CreateIfNotExistsAsync(); 
            }
            catch (StorageException ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return response;
        }

        //Table Client
        public static CloudTableClient TableClient()
        {
            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString("DefaultEndpointsProtocol=https;AccountName=cloudenterstorag;AccountKey=8u2BWYObbZpTCrC430dGQQ0okv7AjCm3Hqwv7+ckdpij5DahqLR3KwxCCbjzlntudyWDjIKqcfm7a50ab5BN0g==");//CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            return tableClient;
        }

        /// <summary>
        /// Validate the connection string information in app.config and throws an exception if it looks like 
        /// the user hasn't updated this to valid values. 
        /// </summary>
        /// <param name="storageConnectionString">Connection string for the storage service or the emulator</param>
        /// <returns>CloudStorageAccount object</returns>
        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }


    }
}

