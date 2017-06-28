using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using Epi.Web.Enter.Common.Constants;
using Epi.Cloud.PublishMetaData;
namespace PublishAndRepublishTest
{
    [TestClass]
    public class PublishRePublishTest
    {
        [TestMethod]
        public void PublishAndRepublishTest()
        {

        }


        [TestMethod]
        public void PublishRepublishTest()
        {

            MetaDataToCloud _publishMetaDataToCloud = new MetaDataToCloud();
            //Stop WebJob
            if (WebJobProcess(Constant.WebJob.Stop))
            {
                //Update the blob
                //Clear the Cache
                if (_publishMetaDataToCloud.ClearCache())
                {
                    //Start Web Job
                    var webJobResponse = _publishMetaDataToCloud.StartAndStopWebJob(Constant.WebJob.Start);
                    Assert.IsTrue(webJobResponse);
                }
            }

        }

        public bool WebJobProcess(string webJobStatus)
        {
            MetaDataToCloud _publishMetaDataToCloud = new MetaDataToCloud();
            if (_publishMetaDataToCloud.StartAndStopWebJob(webJobStatus))
            {
                return true;
            }
            return false;
        }
    }
}
