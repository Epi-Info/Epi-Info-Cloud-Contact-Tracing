using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.DataEntryServices.Extensions;

namespace MetadataTests
{
    [TestClass]
    public class TestOrganizationBOExtension
    {
        [TestMethod]
        public void GetOrganization()
        {
            var organizationbo = new OrganizationBO() {IsEnabled = true, Organization = "Epi Info", OrganizationId = 1, OrganizationKey = "evEY87gI3K68xcoi2Bx4YBb8AuAfuJW3lPXLo3cWuCxy5nSOKsB+5ZtvUuHEMC76" };
            //organizationbo or = new organization
            //OrganizationBOExtensions orbo = new OrganizationBOExtensions();
            var Organization = OrganizationBOExtensions.ToOrganization(organizationbo);//    organizationbo.ToOrganization();// organizationbo.ToO



            //var form = new Form() { ResponseId = "d1def644-931a-4f9c-8eb7-ba6e45bd5250", Height = 1016, Width = 780, PageId = "1", IsMobile = false };


            //var pageResponseProperties = form.ToPageResponseProperties(responseId);


            //Assert.AreEqual(form.ResponseId, pageResponseProperties.GlobalRecordID);  
            //Assert.AreEqual(form.PageId, pageResponseProperties.PageId);    
            Assert.AreEqual(Organization.Organization1, organizationbo.Organization);
            Assert.AreEqual(Organization.OrganizationKey, organizationbo.OrganizationKey);
            Assert.IsTrue(Organization.IsEnabled);
        }
    }
}
