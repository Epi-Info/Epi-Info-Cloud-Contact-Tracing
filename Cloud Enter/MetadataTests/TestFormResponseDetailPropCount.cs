using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.DataPersistence.DataStructures;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestFormResponseDetailPropCount
    {
        private const int FormResponseDetailPropertyCount = 31;
        [TestMethod]
        public void TestMethod1()
        {
            FormResponseDetail objFormResponseDetail = new FormResponseDetail();
            int numberOfPublicPropertiesofFormResponseDetail = objFormResponseDetail.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofFormResponseDetail, FormResponseDetailPropertyCount, "FormResponseDetail properties has been changed");
        }
    }
}
