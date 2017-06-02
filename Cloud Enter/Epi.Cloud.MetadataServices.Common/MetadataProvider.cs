using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.MetadataServices.Common.MetadataBlobService;
using Epi.Cloud.MetadataServices.Common.ProxyService;
using Epi.Common.Constants;
using Epi.FormMetadata.DataStructures;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Epi.Cloud.MetadataServices.Common
{
    public class MetadataProvider
    {
        private static Guid _projectId;

        [ThreadStatic]
        MetadataBlobCRUD _metadataBlobCRUD;

        MetadataBlobCRUD MetadataBlobCRUD
        {
            get
            {
                if (_metadataBlobCRUD == null)
                {
                    var containerName = AppSettings.GetStringValue(AppSettings.Key.MetadataBlogContainerName);
                    _metadataBlobCRUD = new MetadataBlobCRUD(containerName);
                }
                return _metadataBlobCRUD;
            }
        }

        public async Task<Dictionary<string, string>> GetMostRecentBlobDeploymentPropertiesAsync()
        {
            return await Task.FromResult(MetadataBlobCRUD.GetMostRecentDeploymentProperties());
        }

        public async Task<Template> RetrieveProjectMetadataAsync(Guid projectId, Dictionary<string, string> deploymentProperties = null)
        {
            Template metadata = await RetrieveMetadataFromBlobStorage(projectId);
            if (metadata == null)
            {
                metadata = await RetrieveProjectMetadataViaAPIAsync(projectId);
                if (metadata == null)
                {
                    MetadataBlobCRUD.SaveMetadataToBlobStorage(metadata);
                }
            }
            _projectId = metadata != null ? new Guid(metadata.Project.Id) : Guid.Empty;
            return metadata;
        }

        public async Task<Template> RetrieveProjectMetadataViaAPIAsync(Guid projectId)
        {
            ProjectMetadataServiceProxy serviceProxy = new ProjectMetadataServiceProxy();
            var metadata = await serviceProxy.GetProjectMetadataAsync(projectId == Guid.Empty ? null : projectId.ToString("N"));

#if CaptureMetadataJson
            var metadataFromService = Newtonsoft.Json.JsonConvert.SerializeObject(metadata);
            if (!System.IO.Directory.Exists(@"C:\Junk")) System.IO.Directory.CreateDirectory(@"C:\Junk");
            System.IO.File.WriteAllText(@"C:\Junk\ZikaMetadataFromService.json", metadataFromService);

            var json = System.IO.File.ReadAllText(@"C:\Junk\ZikaMetadataFromService.json");
            Template metadataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);
#endif

            return metadata;
        }

        public async Task<Template> RetrieveMetadataFromBlobStorage(Guid projectId)
        {
            Template metadata = null;
            Dictionary<string, string> blobMetadataDictionary = null;
            if (projectId == Guid.Empty)
            {
                var metadataBlobs = MetadataBlobCRUD.GetBlobList(BlobListingDetails.Metadata);
                if (metadataBlobs.Count > 0)
                {
                    var blobMetadataJson = metadataBlobs.First();
                    blobMetadataDictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(blobMetadataJson);
                    Guid.TryParse(blobMetadataDictionary[BlobMetadataKeys.ProjectId], out projectId);
                }
            }
            if (projectId != Guid.Empty)
            {
                var json = MetadataBlobCRUD.DownloadText(projectId.ToString("N"));
                if (!string.IsNullOrWhiteSpace(json))
                {
                    metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);
                    if (blobMetadataDictionary != null)
                    {
                        metadata.ProjectDeploymentProperties = blobMetadataDictionary;
                    }
                    else
                    {
                        metadata.ProjectDeploymentProperties = MetadataBlobCRUD.GetBlobMetadata(projectId.ToString("N")) as Dictionary<string, string>;
                    }
                }
            }
            return await Task.FromResult(metadata);
        }

        public async Task<bool> UpdateMetadataInBlobStorage(Template metadata)
        {
            var deploymentProperties = metadata.ProjectDeploymentProperties;
            return await Task.FromResult(MetadataBlobCRUD.SaveMetadataToBlobStorage(metadata, deploymentProperties, deleteBeforeUpload: false));
        }
    }
}
