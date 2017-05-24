using System;
using Epi.Cloud.MetadataServices.Common.ProxyService;
using Epi.Cloud.ServiceBus;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Newtonsoft.Json;

namespace Epi.Cloud.WebJobs
{
    public class SqlConsistencyHandler
    {
        public SqlConsistencyHandler()
        {

        }
        static void Main(string[] args)
        {
            try
            {
                //Read Message from Service Bus 
                var messagePayload = ReadMessageFromServiceBus();

                if (messagePayload == null)
                {
                    Console.WriteLine(messagePayload.Body);
                }
                else
                {
                    SendSurveyDataToSQL(messagePayload);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static MessagePayload ReadMessageFromServiceBus()
        {
            //Read Message from Service Bus
            var serviceBusCRUD = new ServiceBusCRUD();
            var messagePayload = serviceBusCRUD.ReceiveMessages();

            if (messagePayload == null)
            {
                //messagePayload.Body = "null";
                return messagePayload;

            }
            else
            {
                return messagePayload;
            }
        }

        public static bool SendSurveyDataToSQL(MessagePayload messagePayload)
        {
            //Convert to FormResponseDetails
            FormResponseDetail formResponseDetail = JsonConvert.DeserializeObject<FormResponseDetail>(messagePayload.Body);
            PageDigest[][] pagedigest = GetPageDigest();

            //Send to SQL Server
            Epi.Cloud.SqlServer.PersistToSqlServer objPersistResponse = new Cloud.SqlServer.PersistToSqlServer();
            objPersistResponse.PersistToSQLServerDB(formResponseDetail, pagedigest);
            return true;
        }

        public static PageDigest[][] GetPageDigest()
        {
            ProjectMetadataServiceProxy ProjectMetadataServiceProxy = new ProjectMetadataServiceProxy();
            Epi.FormMetadata.DataStructures.PageDigest[][] pageDigest = ProjectMetadataServiceProxy.GetPageDigestMetadataAsync().Result;
            return pageDigest;
        }

    }
}
