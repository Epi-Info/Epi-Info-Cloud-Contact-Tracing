
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.DataEntryServices.Extensions;

namespace MetadataTests
{
    [TestClass]
    public class TestOrganizationExtension
    {
        [TestMethod]
        public void GetOrganization()
        {

//            var organization = new OrganizationBO() { Organization = "Epi Info"};

            string organization = "Epi Info";


            var Organization = OrganizationExtensions.ToOrganizationBO(organization);
            // OrganizationBOExtensions.ToOrganization(organizationbo);

            Assert.AreEqual(Organization.Organization, organization);
            
            
        }
    }
}
