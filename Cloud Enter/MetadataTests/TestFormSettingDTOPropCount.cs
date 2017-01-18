using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.DTO;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestFormSettingDTOPropCount
    {
         private const int FormSettingDTOPropertyCount = 14;
        [TestMethod]
        public void TestMethod1()
        {
            FormSettingDTO objFormSettingDTO = new FormSettingDTO();
            int numberOfPublicPropertiesofFormSettingDTO = objFormSettingDTO.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofFormSettingDTO, FormSettingDTOPropertyCount, "FormSettingDTO properties has been changed");

        }
    }
}
