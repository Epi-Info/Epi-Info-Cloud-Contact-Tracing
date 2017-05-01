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
            MetadataAccessor.StaticCache.ProjectMetadataProvider = _projectMetadataProvider.Object;

            var form = new Form() { ResponseId = "d1def644-931a-4f9c-8eb7-ba6e45bd5250", Height = 1016, Width = 780, PageId = "1", IsMobile = false };

            
            //formResponseDetail.RecStatus, surveyResponse.Status);
            //Assert.AreEqual(form.PageId, pageResponseProperties.PageId);    //formResponseDetail.FormId, surveyResponse.SurveyId);
        }
    }
}
