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
using Epi.Web.Enter.Common.Security;
//using Epi.Web.Enter.Common.Security;

namespace Epi.Cloud.PublishMetaData
{
    public class MetaDataToCloud
    {
        static string containerName = AppSettings.GetStringValue(AppSettings.Key.MetadataBlogContainerName);

        MetadataBlobCRUD _metadataBlobCRUD = new MetadataBlobCRUD(containerName);

        #region Start and Stop Cloud Web Job
        public bool StartAndStopWebJob(string status)
        {
            var environmentKey = ConfigurationManager.AppSettings["Environment"];
            string webJobUserName = GetEnvironmentValue("WebJobUsername@" + environmentKey);
            string webJobPassWord = GetEnvironmentValue("WebJobPassWord@" + environmentKey);
            string webJobName = GetEnvironmentValue("WebJobName@" + environmentKey);
            string webJobUrl = GetEnvironmentValue("WebJobURL@" + environmentKey);
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

        public string GetEnvironmentValue(string resourceName)
        {
            string connectionKey = string.Empty;
            if (resourceName != null)
            {
                connectionKey = ConfigurationManager.AppSettings[resourceName];
                var DecryptConnectionValue = Cryptography.Decrypt(connectionKey);
                return DecryptConnectionValue;
            }
            return connectionKey;
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
