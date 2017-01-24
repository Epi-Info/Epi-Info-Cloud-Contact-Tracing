using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Epi.Cloud.MetadataServices.MetadataBlobService
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

        public bool UploadText(string content,  string blobName)
        {
            try
            {
                CloudBlockBlob blob = BlobContainer.GetBlockBlobReference(blobName);
                blob.UploadText(content);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UploadFromByteArray(Byte[] buffer, string blobName)
        {
            try
            {
                CloudBlockBlob blob = BlobContainer.GetBlockBlobReference(blobName);
                blob.UploadFromByteArray(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UploadFromStream(Stream stream, string blobName)
        {
            try
            {
                //reset the stream back to its starting point (no partial saves)
                stream.Position = 0;
                CloudBlockBlob blob = BlobContainer.GetBlockBlobReference(blobName);
                blob.UploadFromStream(stream);
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

        public bool DownloadFile(string blobName, string downloadFolder)
        {
            try
            {
                CloudBlockBlob blobSource = BlobContainer.GetBlockBlobReference(blobName);
                if (blobSource.Exists())
                {
                    //blob storage uses forward slashes, windows uses backward slashes; do a replace
                    //  so localPath will be right
                    string localPath = Path.Combine(downloadFolder, blobSource.Name.Replace(@"/", @"\"));
                    //if the directory path matching the "folders" in the blob name don't exist, create them
                    string dirPath = Path.GetDirectoryName(localPath);
                    if (!Directory.Exists(localPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    blobSource.DownloadToFile(localPath, FileMode.Create);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Byte[] DownloadToByteArray(string blobName)
        {
            CloudBlockBlob blob = BlobContainer.GetBlockBlobReference(blobName);
            //you have to fetch the attributes to read the length
            blob.FetchAttributes();
            long fileByteLength = blob.Properties.Length;
            Byte[] myByteArray = new Byte[fileByteLength];
            blob.DownloadToByteArray(myByteArray, 0);
            return myByteArray;
        }

        public bool DownloadToStream(string blobName, Stream stream)
        {
            try
            {
                CloudBlockBlob blob = BlobContainer.GetBlockBlobReference(blobName);
                blob.DownloadToStream(stream);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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

        public List<string> GetBlobList()
        {
            List<string> listOfBlobs = new List<string>();
            foreach (IListBlobItem blobItem in BlobContainer.ListBlobs(null, true, BlobListingDetails.All))
            {
                string oneFile = GetFileNameFromBlobURI(blobItem.Uri, _containerName);
                listOfBlobs.Add(oneFile);
            }
            return listOfBlobs;
        }

        public List<string> GetBlobListForRelPath(string relativePath)
        {
            //first, check the slashes and change them if necessary
            //second, remove leading slash if it's there
            relativePath = relativePath.Replace(@"\", @"/");
            if (relativePath.Substring(0, 1) == @"/")
                relativePath = relativePath.Substring(1, relativePath.Length - 1);

            List<string> listOBlobs = new List<string>();
            foreach (IListBlobItem blobItem in
            _cloudBlobContainer.ListBlobs(relativePath, true, BlobListingDetails.All))
            {
                string oneFile = GetFileNameFromBlobURI(blobItem.Uri, _containerName);
                listOBlobs.Add(oneFile);
            }
            return listOBlobs;
        }
    }
}