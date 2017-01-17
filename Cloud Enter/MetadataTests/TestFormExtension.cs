using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcDynamicForms;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.Metadata;
using Moq;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.DataEntryServices.Extensions;

namespace MetadataTests
{
    [TestClass]
    public class TestFormExtension
    {
        [TestMethod]
        public void GetPageRespondePropertiesfromForm()
        {
            var json = System.IO.File.ReadAllText(@"c:\junk\ZikaMetadataFromService.json");
            Template metadataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);
            MetadataAccessor metaDataAccessor = new MetadataAccessor("2e1d01d4-f50d-4f23-888b-cd4b7fc9884b");
            string responseId = "d1def644-931a-4f9c-8eb7-ba6e45bd5250";
            metaDataAccessor.CurrentFormId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b";
            var _projectMetadataProvider = new Mock<IProjectMetadataProvider>();
            MetadataAccessor.StaticCache._projectMetadataProvider = _projectMetadataProvider.Object;

            var form = new Form() { ResponseId = "d1def644-931a-4f9c-8eb7-ba6e45bd5250", Height = 1016, Width = 780, PageId = "1", IsMobile = false };

            //var surveyResponse = formResponseDetail.ToSurveyResponseBO();
            var pageResponseProperties = form.ToPageResponseProperties(responseId);

            //var formExtensions = new FormExtensions();
            //{ FormId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b", FormName = "Zika", RecStatus = 1, GlobalRecordID = "cb07a6bc-d7e2-4f78-a3b0-de194227d351", LastActiveUserId = 1014, LastSaveTime = DateTime.UtcNow };
            Assert.AreEqual(form.ResponseId, pageResponseProperties.GlobalRecordID);
            
            //formResponseDetail.RecStatus, surveyResponse.Status);
            //Assert.AreEqual(form.PageId, pageResponseProperties.PageId);    //formResponseDetail.FormId, surveyResponse.SurveyId);
        }
    }
}
