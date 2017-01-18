using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.BusinessObjects;
using System.Linq;
using Epi.Cloud.Common.DTO;

namespace MetadataTests
{
    [TestClass]
    public class TestSurveyInfoBO
    {
       private const int SurveyInfoBOPropertyCount = 23;
       private const int SurveyInfoDTOPropertyCount = 26;

 [TestMethod]
        public void TestSurveyInfoBODTO()
        {
            SurveyInfoBO objsurveyinfobo = new SurveyInfoBO();
            SurveyInfoDTO objsurveyInfoDTO = new SurveyInfoDTO();

            int numberOfPublicPropertiesofSurveyInfoBO = objsurveyinfobo.GetType().GetProperties().Count();
            int numberOfPublicPropertiesofSurveyInfoDTO = objsurveyInfoDTO.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofSurveyInfoBO, SurveyInfoBOPropertyCount, "SurveyInfoBO properties has been changed");
            Assert.AreEqual(numberOfPublicPropertiesofSurveyInfoDTO, SurveyInfoDTOPropertyCount, "SurveyInfoDTO properties has been changed");
        }
    }
}


