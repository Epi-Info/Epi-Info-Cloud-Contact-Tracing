﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.CacheServices;

namespace MetadataTests
{
    /// <summary>
    /// Summary description for FullMetadatacache
    /// </summary>
    [TestClass]
    public class FullProjectTemplateMetadata
    {
        public FullProjectTemplateMetadata()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void FullProjectTemplateMetadataExists()
        {

            EpiCloudCache metaDataCache = new EpiCloudCache();

          //  Guid projectguid = Guid.NewGuid();// Guid.Parse("257b05f2-dab2-c8e3-caed-92f0f6a88169");
            Guid projectguid =  Guid.Parse("257b05f2-dab2-c8e3-caed-92f0f6a88169");


            bool result = metaDataCache.FullProjectTemplateMetadataExists(projectguid);
            //  Assert.AreEqual(result, false,"Project does not exist");
            //_weakProjectMetadataObjectCache count 0

            Assert.IsTrue(result, " Metadatacache Doesnot Exists");
        }
    }
}