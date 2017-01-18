using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.BusinessObjects;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestSurveyRequestBOProCount
    {
        private const int SurveyRequestBOPropertyCount = 8;
        [TestMethod]
        public void TestMethod1()
        {
            SurveyRequestBO objSurveyRequestBO = new SurveyRequestBO();
            int numberOfPublicPropertiesofSurveyRequestBO = objSurveyRequestBO.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofSurveyRequestBO, SurveyRequestBOPropertyCount, "SurveyRequestBO properties has been changed");

        }
    }
}
