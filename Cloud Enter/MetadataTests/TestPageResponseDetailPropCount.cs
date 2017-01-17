using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.DataPersistence.DataStructures;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestPageResponseDetailPropCount
    {
         private const int PageResponseDetailPropertyCount = 7;
        [TestMethod]
        public void TestMethod1()
        {
            PageResponseDetail objPageResponseDetail = new PageResponseDetail();
            int numberOfPublicPropertiesofPageResponseDetail = objPageResponseDetail.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofPageResponseDetail, PageResponseDetailPropertyCount, "PageResponseDetail properties has been changed");

        }
    }
}
