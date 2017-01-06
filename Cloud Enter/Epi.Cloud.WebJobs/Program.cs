﻿using System;
using Epi.Cloud.MetadataServices.ProxiesService;
using Epi.Cloud.ServiceBus;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Newtonsoft.Json;

namespace Epi.Cloud.WebJobs
{
    class Program
    {
        static void Main(string[] args)
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

        public static PageDigest[][] GetPageDigest()
        {
            ProjectMetadataServiceProxy ProjectMetadataServiceProxy = new ProjectMetadataServiceProxy();
            Epi.FormMetadata.DataStructures.PageDigest[][] pageDigest = ProjectMetadataServiceProxy.GetPageDigestMetadataAsync().Result;
            return pageDigest;
        }

    }
}