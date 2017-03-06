using System;
using Epi.Cloud.MetadataServices.Common.ProxyService;
using Epi.Cloud.ServiceBus;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Newtonsoft.Json;

namespace Epi.Cloud.WebJobs
{
    class SqlConsistencyHandler
    {
        static void Main(string[] args)
        {
            try
            {
                //Read Message from Service Bus
                var serviceBusCRUD = new ServiceBusCRUD();
                var surveyData = serviceBusCRUD.ReceiveMessages();

                if (surveyData == null)
                {
                    Console.WriteLine("SurveyData is not available in the Service Bus");
                }
                else
                {
                    //Convert to FormResponseDetails
                    FormResponseDetail formResponseDetail = JsonConvert.DeserializeObject<FormResponseDetail>(surveyData);
                    PageDigest[][] pagedigetst = GetPageDigest();

                    //Send to SQL Server
                    Epi.Cloud.SqlServer.PersistToSqlServer objPersistResponse = new Cloud.SqlServer.PersistToSqlServer();
                    objPersistResponse.PersistToSQLServerDB(formResponseDetail, pagedigetst);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static PageDigest[][] GetPageDigest()
        {
            ProjectMetadataServiceProxy ProjectMetadataServiceProxy = new ProjectMetadataServiceProxy();
            Epi.FormMetadata.DataStructures.PageDigest[][] pageDigest = ProjectMetadataServiceProxy.GetPageDigestMetadataAsync().Result;
            return pageDigest;
        }

    }
}
