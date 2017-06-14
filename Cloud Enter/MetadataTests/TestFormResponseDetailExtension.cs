using System;
//using Epi.DataPersistence.Extensions;
using Epi.Cloud.DataEntryServices.Extensions;
using Epi.DataPersistence.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Epi.DataPersistence.Common;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.Metadata;
using Moq;
using Epi.Cloud.Interfaces.MetadataInterfaces;

namespace MetadataTests
{
    [TestClass]
    public class TestFormResponseDetailExtension
    {
        [TestMethod]
        public void GetSurveyResponseFromFormDetail()
        {
            //var json = System.IO.File.ReadAllText(@"c:\junk\ZikaMetadataFromService.json");
            //Template metadataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);
            MetadataAccessor metaDataAccessor = new MetadataAccessor("2e1d01d4-f50d-4f23-888b-cd4b7fc9884b");

            metaDataAccessor.CurrentFormId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b";
            var _projectMetadataProvider = new Mock<IProjectMetadataProvider>();
            MetadataAccessor.StaticCache.ProjectMetadataProvider = _projectMetadataProvider.Object;
            var formResponseDetail = new FormResponseDetail() { FormId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b", FormName = "Zika", RecStatus = 1, ResponseId = "cb07a6bc-d7e2-4f78-a3b0-de194227d351", UserId = 1014, LastSaveTime = DateTime.UtcNow };
            var surveyResponse = formResponseDetail.ToSurveyResponseBO();
            Assert.AreEqual(formResponseDetail.RecStatus, surveyResponse.Status);
            Assert.AreEqual(formResponseDetail.FormId, surveyResponse.FormId);

        }
    }
}
