using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Epi.Cloud.Common.Configuration;
using Epi.Common.Constants;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Epi.Cloud.MetadataServices.Common.MetadataBlobService
{
    public partial class MetadataBlobCRUD
    {

        //these variables are used throughout the class
        string _containerName { get; set; }

        CloudStorageAccount _cloudStorageAccount;

        CloudBlobClient _cloudBlobClient;
        CloudBlobContainer _cloudBlobContainer { get; set; }

        public MetadataBlobCRUD(string containerName)
        {
            Initialize(containerName);
        }

        private CloudBlobClient BlobClient { get { return _cloudBlobClient ?? GetBlobClient(); } }

        private CloudBlobContainer  BlobContainer { get { return _cloudBlobContainer ?? GetBlobContainer(_containerName); } }

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

        public bool UploadText(string content, string blobName, string description = null)
        {
            try
            {
                CloudBlockBlob blob = BlobContainer.GetBlockBlobReference(blobName);
                blob.UploadText(content);
                CloudBlockBlob blobReference = BlobContainer.GetBlockBlobReference(blobName);
                blobReference.Metadata.Add(BlobMetadataKeys.Id, blobName);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    blobReference.Metadata.Add(BlobMetadataKeys.Description, description);
                }
                blobReference.SetMetadata();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UploadText(string content, string blobName, IDictionary<string, string> metadataDictionary = null)
        {
            try
            {
                CloudBlockBlob blob = BlobContainer.GetBlockBlobReference(blobName);
                blob.UploadText(content);
                CloudBlockBlob blobReference = BlobContainer.GetBlockBlobReference(blobName);
                blobReference.Metadata.Add(BlobMetadataKeys.Id, blobName);
                if (metadataDictionary != null)
                {
                    foreach (var kvp in metadataDictionary)
                    {
                        blobReference.Metadata.Add(kvp.Key, kvp.Value);
                    }
                }
                blobReference.SetMetadata();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string DownloadText(string blobName)
        {
            CloudBlockBlob blobSource = BlobContainer.GetBlockBlobReference(blobName);
            string content = blobSource.DownloadText();
            return content;
        }


        public bool RenameBlob(string blobName, string newBlobName)
        {
            try
            {
                CloudBlockBlob blobSource = BlobContainer.GetBlockBlobReference(blobName);
                if (blobSource.Exists())
                {
                    CloudBlockBlob blobTarget = BlobContainer.GetBlockBlobReference(newBlobName);
                    blobTarget.StartCopy(blobSource);
                    blobSource.Delete();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //if the blob is there, delete it 
        //check returning value to see if it was there or not
        public bool DeleteBlob(string blobName)
        {
            try
            {
                CloudBlockBlob blobSource = BlobContainer.GetBlockBlobReference(blobName);
                bool blobExisted = blobSource.DeleteIfExists();
                return blobExisted;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// parse the blob URI to get just the file name of the blob 
        /// after the container. So this will give you /directory1/directory2/filename if it's in a "subfolder"
        /// </summary>
        /// <param name="theUri"></param>
        /// <returns>name of the blob including subfolders (but not container)</returns>
        private string GetFileNameFromBlobURI(Uri theUri, string containerName)
        {
            string theFile = theUri.ToString();
            int dirIndex = theFile.IndexOf(containerName);
            string oneFile = theFile.Substring(dirIndex + containerName.Length + 1,
                theFile.Length - (dirIndex + containerName.Length + 1));
            return oneFile;
        }

        public List<string> GetBlobList(BlobListingDetails details = BlobListingDetails.None)
        {
            List<string> listOfBlobs = new List<string>();
            foreach (IListBlobItem blobItem in BlobContainer.ListBlobs(null, true, details))
            {
                string blobInfo;
                if (details == BlobListingDetails.Metadata)
                {
                    blobInfo = Newtonsoft.Json.JsonConvert.SerializeObject(((CloudBlockBlob)blobItem).Metadata);
                }
                else
                {
                    blobInfo = GetFileNameFromBlobURI(blobItem.Uri, _containerName);
                }
                listOfBlobs.Add(blobInfo);
            }
            return listOfBlobs;
        }

        public List<string> GetBlobListWithDescription()
        {
            List<string> listOfBlobs = new List<string>();
            foreach (IListBlobItem blobItem in BlobContainer.ListBlobs(null, true, BlobListingDetails.None))
            {
                string blobName = GetFileNameFromBlobURI(blobItem.Uri, _containerName);
                CloudBlockBlob blobReference = BlobContainer.GetBlockBlobReference(blobName);
                blobReference.FetchAttributes();
                var value = blobReference.Metadata[BlobMetadataKeys.Description];
                blobName = blobName + (string.IsNullOrWhiteSpace(value) ? "" : ':' + value);
                listOfBlobs.Add(blobName);
            }
            return listOfBlobs;
        }
        public List<string> GetBlobListWithKeys(params string[] metadataKeys)
        {
            List<string> listOfBlobs = new List<string>();
            foreach (IListBlobItem blobItem in BlobContainer.ListBlobs(null, true, BlobListingDetails.None))
            {
                string blobName = GetFileNameFromBlobURI(blobItem.Uri, _containerName);
                CloudBlockBlob blobReference = BlobContainer.GetBlockBlobReference(blobName);
                blobReference.FetchAttributes();
                var keys = blobReference.Metadata.Keys;
                var sb = new StringBuilder();
                sb.Append(blobName);
                bool isFirst = true;
                foreach (var key in metadataKeys)
                {
                    if (keys.Contains(key))
                    {
                        var value = blobReference.Metadata[key];
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            sb.Append((isFirst ? ":" : "|") + key + "=" + value);
                            isFirst = false;
                        }
                    }
                }
                var blobMetadata = sb.ToString();
                listOfBlobs.Add(blobMetadata);
            }
            return listOfBlobs;
        }
    }
}