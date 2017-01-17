using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.BusinessObjects;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestFormsHierarchyBOExtension
    {
        private const int FormsHierarchyDTOPropertyCount = 7;
        private const int FormsHierarchyBOPropertyCount = 7;
        [TestMethod]
        public void TestFormHierachyBODTO()
        {


            FormsHierarchyDTO objformsHierarchyDTO = new FormsHierarchyDTO();
            FormsHierarchyBO objformsHierarchyBO = new FormsHierarchyBO();

            int numberOfPublicPropertiesofFormHierarchyDTO = objformsHierarchyDTO.GetType().GetProperties().Count();
            int numberOfPublicPropertiesofFormHierarchyBO = objformsHierarchyBO.GetType().GetProperties().Count();

            Assert.AreEqual(numberOfPublicPropertiesofFormHierarchyDTO, FormsHierarchyDTOPropertyCount, "FormHierarchyDTO properties has been changed");
            Assert.AreEqual(numberOfPublicPropertiesofFormHierarchyBO, FormsHierarchyBOPropertyCount, "FormHierarchyBO properties has been changed");






        }
    }
}
