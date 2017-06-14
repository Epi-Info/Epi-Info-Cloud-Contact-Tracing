using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.BusinessObjects;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestSurveyResponseBOPropertyCount
    {
        private const int SurveyResponseBOPropertyCount = 29;
        [TestMethod]
        public void TestMethod1()
        {
            SurveyResponseBO objSurveyResponseBO = new SurveyResponseBO();
            int numberOfPublicPropertiesofSurveyResponseBO = objSurveyResponseBO.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofSurveyResponseBO, SurveyResponseBOPropertyCount, "SurveyResponseBO properties has been changed");

        }
    }
}
