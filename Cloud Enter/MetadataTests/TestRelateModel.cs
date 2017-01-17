using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Web.MVC.Models;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestRelateModel
    {
        private const int RelateModelPropertyCount = 6;
        [TestMethod]
        public void TestMethod1()
        {

            RelateModel objRelateModel = new RelateModel();
            int numberOfPublicPropertiesofRelateModel = objRelateModel.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofRelateModel, RelateModelPropertyCount, "RelateModel properties has been changed");
        }
    }
}
