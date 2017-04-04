using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Cloud.CloudOperation;
using System.Configuration;
using Epi.Cloud.MetadataServices.Common;
using Epi.Cloud.MetadataServices.Common.MetadataBlobService;
using Epi.FormMetadata;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.CacheServices;
using Epi.FormMetadata.DataStructures;
using StackExchange.Redis; 

namespace Epi.Cloud.PublishMetaData
{
    public class MetaDataToCloud
    {
        static string containerName = AppSettings.GetStringValue(AppSettings.Key.MetadataBlogContainerName);
        MetadataBlobCRUD _metadataBlobCRUD = new MetadataBlobCRUD(containerName);

        #region Start and Stop Cloud Web Job
        public bool StartAndStopWebJob(string status)
        {
            string webJobUserName = "$EICDCApp";// ConfigurationManager.AppSettings["webJobUserName"];
            string webJobPassWord = "TXlHvy0F6nag33DwbimSrwejcS6LdNurmEm7AijGDjCr6evGCkTqRQgJlC6m";// ConfigurationManager.AppSettings["webJobPassWord"];
            string webJobName = "CloudJob";// ConfigurationManager.AppSettings["webJobName"];             
            string webJobUrl = "https://eicdcapp.scm.azurewebsites.net/api/continuouswebjobs/";// ConfigurationManager.AppSettings["webJobUrl"];
            return WebJobHandler.EnableandDisableWebJob(webJobUserName, webJobPassWord, webJobName, status, webJobUrl); 
        } 
        #endregion

        #region Upload To Blob
        public bool UploadBlob()
        {
            try
            {
                MetadataProvider metadataProvider = new MetadataProvider();
                var metaData = metadataProvider.RetrieveProjectMetadataViaAPIAsync(Guid.Empty).Result;
                return SaveMetadataToBlob(metaData);
            }
            catch
            {
                return false;// ("error");
            }
        }
        private bool SaveMetadataToBlob(Template metadata)
        {
            return _metadataBlobCRUD.SaveMetadataToBlobStorage(metadata);
        }
        #endregion

        #region ClearCache


        
        public bool ClearCache()
        {
            RedisCacheHandler _redishCacheHandler = new RedisCacheHandler();
            try
            {
                _redishCacheHandler.ClearCache();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
 
    }
}
