using System;
using System.Configuration;
using Epi.Cloud.CloudOperation;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.MetadataServices.Common;
using Epi.Cloud.MetadataServices.Common.MetadataBlobService;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.PublishMetaData
{
    public class MetaDataToCloud
    {
        static string containerName = AppSettings.GetStringValue(AppSettings.Key.MetadataBlogContainerName);



        #region Start and Stop Cloud Web Job
        public bool StartAndStopWebJob(string status)
        {
            var environmentKey = ConfigurationManager.AppSettings["Environment"];
            string webJobUserName = GetEnvironmentValue("WebJobUsername");
            string webJobPassWord = GetEnvironmentValue("WebJobPassWord");
            string webJobName = GetEnvironmentValue("WebJobName");
            string webJobUrl = GetEnvironmentValue("WebJobURL");
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
            MetadataBlobCRUD _metadataBlobCRUD = new MetadataBlobCRUD(containerName);
            return _metadataBlobCRUD.SaveMetadataToBlobStorage(metadata);
        }
        #endregion

        #region ClearCache

        public string GetEnvironmentValue(string resourceName)
        {
            string connectionValue = string.Empty;
            if (resourceName != null)
            {
                connectionValue = ConfigurationManager.AppSettings[resourceName];
                return connectionValue;
            }
            return null;
        }

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
