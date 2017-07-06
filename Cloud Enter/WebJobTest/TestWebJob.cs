using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.WebJobs;
using Epi.Cloud.ServiceBus;
using Epi.DataPersistence.DataStructures;
using Newtonsoft.Json;
using Epi.PersistenceServices.DocumentDB;
using Epi.Common.Core.Interfaces;
using System.IO;

namespace WebJobTest
{
    [TestClass]
    public class TestWebJob
    {
        private const string FormId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b";

        [TestMethod]
        public void ReadServiceBus()
        {
            MessagePayload messagePayload = null;
            messagePayload = ReadMessageFromServiceBus();
            if (messagePayload == null)
            {
                ServiceBusCRUD _serviceBus = new ServiceBusCRUD();
                _serviceBus.SendMessagesToTopic(GetFormResponDetails());
                messagePayload = ReadMessageFromServiceBus();
                FormResponseDetail _formSurveyData = GetFormResponDetailsFromJson(messagePayload);
                Assert.AreEqual(FormId, _formSurveyData.FormId);
            }
            else
            {

                FormResponseDetail formResponsefromServiceBus = JsonConvert.DeserializeObject<FormResponseDetail>(messagePayload.Body);
                Assert.AreEqual(FormId, formResponsefromServiceBus.FormId);
            }

        }

        [TestMethod]
        public void SendSurveyDataToSql()
        {
            MessagePayload messagePayload = new MessagePayload();
            messagePayload.Body = JsonConvert.SerializeObject(GetFormResponDetails(), Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var sqlConsistencyHandler = new SqlConsistencyHandler();
            var DBResponse = SqlConsistencyHandler.SendSurveyDataToSQL(messagePayload);
            Assert.IsTrue(DBResponse);
        }


        public MessagePayload ReadMessageFromServiceBus()
        {
            return SqlConsistencyHandler.ReadMessageFromServiceBus();
        }

        public FormResponseDetail GetFormResponDetailsFromJson(MessagePayload messagePayload)
        {
            return JsonConvert.DeserializeObject<FormResponseDetail>(messagePayload.Body);
        }

        public FormResponseDetail GetFormResponDetails()
        {
            FormResponseDetail _formResponse = new FormResponseDetail();
            _formResponse.ResponseId = "4735d953-a4db-42fb-9708-27ffeffa2e27";
            _formResponse.FormId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b";
            _formResponse.RootFormName = "Zika";
            _formResponse.RecStatus = 2;
            _formResponse.FirstSaveTime = DateTime.Now;
            _formResponse.LastSaveTime = DateTime.Now;
            _formResponse.UserOrgId = 1;
            _formResponse.UserId = 1014;
            _formResponse.UserName = "Ananth_Raja@sra.com";
            _formResponse.IsRelatedView = false;
            _formResponse.IsDraftMode = false;
            _formResponse.IsLocked = false;
            _formResponse.IsNewRecord = false;
            //_formResponse.PageIds = [1, 2, 3, 4, 5, 6, 9];
            _formResponse.LastPageVisited = 0;
            _formResponse.RequiredFieldsList = "patientname1";
            _formResponse.HiddenFieldsList = ",whh_illnessvacchist";
            _formResponse.HighlightedFieldsList = "";
            _formResponse.FormName = "Zika";
            _formResponse.DisabledFieldsList = ",vaccyear_dengue,vaccyear_japenceph,vaccyear_tickenceph,vaccyear_yellowfever,whh_illnessvacchist";
            return _formResponse;

        }
    }
}
