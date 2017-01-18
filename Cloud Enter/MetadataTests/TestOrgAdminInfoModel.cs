using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.Message;
using Epi.Web.MVC.Models;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestOrgAdminInfoModel
    {
        //private const int OrganizationResponsePropertyCount =;
        private const int OrgAdminInfoModelPropertyCount = 8;
        [TestMethod]
        public void TestOrgAdminInfo()
        {
            //OrganizationResponse objOrganizationResponse = new OrganizationResponse();
            OrgAdminInfoModel objOrgAdminInfoModel = new OrgAdminInfoModel();

           // int numberOfPublicPropertiesofOrganizationResponse = objOrganizationResponse.GetType().GetProperties().Count();
            int numberOfPublicPropertiesofOrgAdminInfoModel = objOrgAdminInfoModel.GetType().GetProperties().Count();

           // Assert.AreEqual(numberOfPublicPropertiesofOrganizationResponse, OrganizationResponsePropertyCount, "OrganizationResponse properties has been changed");
            Assert.AreEqual(numberOfPublicPropertiesofOrgAdminInfoModel, OrgAdminInfoModelPropertyCount, "OrgAdminInfoModel properties has been changed");

        }
    }
}
