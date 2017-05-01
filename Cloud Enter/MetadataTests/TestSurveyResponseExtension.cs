using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.Metadata;
using Moq;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.DataEntryServices.Extensions;
using Epi.Cloud.Common.BusinessObjects;

namespace MetadataTests
{
    [TestClass]
    public class TestSurveyResponseExtension
    {
        [TestMethod]
        public void GetSurveyResponseBO()
        {
            var json = System.IO.File.ReadAllText(@"c:\junk\ZikaMetadataFromService.json");
            Template metadataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);
            var json1 = System.IO.File.ReadAllText(@"c:\junk\ZikaMetadataWithDigests.json");
            Template metadataObject1 = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);
            MetadataAccessor metaDataAccessor = new MetadataAccessor("2e1d01d4-f50d-4f23-888b-cd4b7fc9884b");

            metaDataAccessor.CurrentFormId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b";
            //MetadataAccessor.ProjectMetadataProvider.GetFormDigestAsync(formId: "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b");
            
            var _projectMetadataProvider = new Mock<IProjectMetadataProvider>();
            MetadataAccessor.StaticCache.ProjectMetadataProvider = _projectMetadataProvider.Object;

            var surveyresp = new SurveyResponse()
            {
                SurveyId = new Guid("2e1d01d4-f50d-4f23-888b-cd4b7fc9884b"),
                ResponseId = new Guid("66ddfe0b-1a0b-4497-98c1-81ca51046b5a"),
                StatusId = 0,
                DateUpdated = DateTime.UtcNow,
                IsDraftMode = false,
                IsLocked = false,
                ParentResponseId = new Guid("00000000-0000-0000-0000-000000000000"),
                RelateParentId = new Guid("00000000-0000-0000-0000-000000000000"),
                DateCompleted=null,
                DateCreated=DateTime.UtcNow
                

            };

            var surveyrespbo = new SurveyResponseBO()
            {
                SurveyId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b",
                ResponseId = "66ddfe0b-1a0b-4497-98c1-81ca51046b5a",                
                RootResponseId = "66ddfe0b-1a0b-4497-98c1-81ca51046b5a",
                DateUpdated = DateTime.UtcNow,
                IsDraftMode = false,
                IsLocked = false,
                ParentResponseId = "00000000-0000-0000-0000-000000000000",
                DateCompleted = null,
                DateCreated = DateTime.UtcNow,
                ViewId=1

            };

            // var iCacheServices = new Mock<Epi.Cloud.CacheServices.IEpiCloudCache>();
           //metaDataAccessor.GetFormDigest(surveyresp.SurveyId.ToString()).ViewId = 1;
            var surveyResponseBO = surveyresp.ToSurveyResponseBO();
            Assert.AreEqual(surveyrespbo.SurveyId, surveyresp.SurveyId);
            Assert.AreEqual(surveyrespbo.DateUpdated, DateTime.UtcNow);


            //var organizationbo = new OrganizationBO() { IsEnabled = true, Organization = "Epi Info", OrganizationId = 1, OrganizationKey = "evEY87gI3K68xcoi2Bx4YBb8AuAfuJW3lPXLo3cWuCxy5nSOKsB+5ZtvUuHEMC76" };
            //organizationbo or = new organization
            //OrganizationBOExtensions orbo = new OrganizationBOExtensions();
            //var Organization = OrganizationBOExtensions.ToOrganization(organizationbo);//    organizationbo.ToOrganization();// organizationbo.ToO



            //var form = new Form() { ResponseId = "d1def644-931a-4f9c-8eb7-ba6e45bd5250", Height = 1016, Width = 780, PageId = "1", IsMobile = false };


            //var pageResponseProperties = form.ToPageResponseProperties(responseId);

        }
    }
}
