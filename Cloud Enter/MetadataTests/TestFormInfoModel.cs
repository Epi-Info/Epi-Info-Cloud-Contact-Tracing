using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.MVC.Models;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestFormInfoModel
    {
       private const int FormInfoModelPropertyCount = 14;
        [TestMethod]
        public void TestFormmodel()
        {
            FormInfoModel objforminfomodel = new FormInfoModel();
            int numberOfPublicPropertiesofFormInfoModel = objforminfomodel.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofFormInfoModel, FormInfoModelPropertyCount, "FormInfoModel properties has been changed");
        }
    }
}



