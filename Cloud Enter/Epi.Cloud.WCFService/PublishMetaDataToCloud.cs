using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using Epi.Cloud.CloudOperation;


namespace Epi.Cloud.WCFService
{
    public class PublishMetaDataToCloud
    {
        string webJobUserName = ConfigurationManager.AppSettings["webJobUserName"];
        string webJobPassWord = ConfigurationManager.AppSettings["webJobPassWord"];
        string webJobName = ConfigurationManager.AppSettings["webJobName"];
        string webJobStatus = ConfigurationManager.AppSettings["webJobStatus"];
        string webJobUrl = ConfigurationManager.AppSettings["webJobUrl"];

         
        public bool StartWebJob()
        {
            return WebJobHandler.EnableandDisableWebJob(webJobUserName, webJobPassWord, webJobName, "start", webJobUrl);
        }
        public bool StopWebJob()
        {
            return WebJobHandler.EnableandDisableWebJob(webJobUserName, webJobPassWord, webJobName, "stop", webJobUrl);
        }
    }
}
