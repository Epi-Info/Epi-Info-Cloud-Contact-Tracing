using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcDynamicForms;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestFormPropertyCount
    {
        private const int FormPropertyCount = 36;
        [TestMethod]
        public void TestForm()
        {
            Form objform = new Form();
            int numberOfPublicPropertiesofForm = objform.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofForm, FormPropertyCount, "Form properties has been changed");
        }
    }
}
