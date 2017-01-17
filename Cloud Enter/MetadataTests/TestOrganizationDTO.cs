using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.DTO;
using System.Linq;
using Epi.Web.MVC.Models;

namespace MetadataTests
{
    [TestClass]
    public class TestOrganizationDTO
    {
        private const int OrganizationDTOPropertyCount = 4 ;
        private const int OrganizationModelPropertyCount = 4;
        [TestMethod]
        public void testOrgDTOModel()
        {
            OrganizationDTO objOrgDTO = new OrganizationDTO();
            OrganizationModel objOrganizationModel = new OrganizationModel();
            int numberOfPublicPropertiesofOrgDTO = objOrgDTO.GetType().GetProperties().Count();
            int numberOfPublicPropertiesofOrgmodel = objOrganizationModel.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofOrgDTO, OrganizationDTOPropertyCount, "OrganizationDTO properties has been changed");
            Assert.AreEqual(numberOfPublicPropertiesofOrgmodel, OrganizationModelPropertyCount, "OrganizationModel properties has been changed");

        }
    }
}
