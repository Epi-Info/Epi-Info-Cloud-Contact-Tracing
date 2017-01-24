using System.Configuration;
using Epi.Cloud.Common.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Epi.Cloud.MetadataServices.MetadataBlobService
{
    public partial class MetadataBlobCRUD
    {
        private void Initialize(string containerName)
        {
            lock (this)
            {
                var connectionStringName = ConfigurationHelper.GetEnvironmentResourceKey("MetadataBlobStorage.ConnectionString");
                var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

                _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                _containerName = containerName.ToLower();
           }
        }

        private CloudBlobClient GetBlobClient()
        {
            _cloudBlobClient = _cloudStorageAccount.CreateCloudBlobClient();
            return _cloudBlobClient;
        }

        private CloudBlobContainer GetBlobContainer(string containerName)
        {
            _cloudBlobContainer = BlobClient.GetContainerReference(containerName);
            if (_cloudBlobContainer.CreateIfNotExists())
            {
                BlobContainerPermissions permissions = new BlobContainerPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                _cloudBlobContainer.SetPermissions(permissions);
            }
            return _cloudBlobContainer;
        }
    }
}
