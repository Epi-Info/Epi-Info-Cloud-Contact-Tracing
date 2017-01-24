using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Epi.Cloud.MetadataServices.MetadataBlobService
{
    public partial class MetadataBlobCRUD
    {
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
