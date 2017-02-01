using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.MetadataServices.Common.MetadataBlobService;
using Epi.Cloud.MetadataServices.Common.ProxyService;
using Epi.FormMetadata.Extensions;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.MetadataServices.Common
{
    public class MetadataProvider
    {
        [ThreadStatic]
        MetadataBlobCRUD _metadataBlobCRUD;

        private static Guid _projectId;

        public async Task<Template> RetrieveProjectMetadata(Guid projectId)
        {

            Template metadata = RetriveMetadataFromBlobStorage(projectId);
            if (metadata == null)
            {
                metadata = await RetrieveProjectMetadataViaAPI(projectId);
            }
            return metadata;
        }

        public async Task<Template> RetrieveProjectMetadataViaAPI(Guid projectId)
        {
            ProjectMetadataServiceProxy serviceProxy = new ProjectMetadataServiceProxy();
            var metadata = await serviceProxy.GetProjectMetadataAsync(projectId == Guid.Empty ? null : projectId.ToString("N"));
            _projectId = metadata != null ? new Guid(metadata.Project.Id) : Guid.Empty;
#if CaptureMetadataJson
            var metadataFromService = Newtonsoft.Json.JsonConvert.SerializeObject(metadata);
            if (!System.IO.Directory.Exists(@"C:\Junk")) System.IO.Directory.CreateDirectory(@"C:\Junk");
            System.IO.File.WriteAllText(@"C:\Junk\ZikaMetadataFromService.json", metadataFromService);

            var json = System.IO.File.ReadAllText(@"C:\Junk\ZikaMetadataFromService.json");
            Template metadataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);
#endif
            PopulateRequiredPageLevelSourceTables(metadata);
            GenerateDigests(metadata);
            SaveMetadata(metadata);
            return metadata;
        }

        private Template RetriveMetadataFromBlobStorage(Guid projectId)
        {
            Template metadata = null;
            if (projectId == Guid.Empty)
            {
                var containerName = AppSettings.GetStringValue(AppSettings.Key.MetadataBlogContainerName);
                _metadataBlobCRUD = _metadataBlobCRUD ?? new MetadataBlobCRUD(containerName);
                var metadataBlobs = _metadataBlobCRUD.GetBlobList();
                if (metadataBlobs.Count > 0)
                {
                    Guid.TryParse(metadataBlobs.First(), out projectId);
                }
            }
            if (projectId != Guid.Empty)
            {
                var json = _metadataBlobCRUD.DownloadText(projectId.ToString("N"));
                metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);
            }

            return metadata;
        }

        private void SaveMetadata(Template metadata)
        {
            var metadataWithDigestsJson = Newtonsoft.Json.JsonConvert.SerializeObject(metadata);

#if CaptureMetadataJson
            if (!System.IO.Directory.Exists(@"C:\Junk")) System.IO.Directory.CreateDirectory(@"C:\Junk");
            System.IO.File.WriteAllText(@"C:\Junk\ZikaMetadataWithDigests.json", metadataWithDigests);
#endif

            SaveMetadataToBlobStorage(metadata, metadataWithDigestsJson);
        }

        private void SaveMetadataToBlobStorage(Template metadata, string metadataWithDigestsJson)
        {
            if (_metadataBlobCRUD == null)
            {
                var containerName = AppSettings.GetStringValue(AppSettings.Key.MetadataBlogContainerName);
                _metadataBlobCRUD = new MetadataBlobCRUD(containerName);
            }
            var projectKey = new Guid(metadata.Project.Id).ToString("N");

            _metadataBlobCRUD.DeleteBlob(projectKey);

            var blobMetadataDictionary = new Dictionary<string, string>();
            blobMetadataDictionary.Add(BlobMetadataKeys.ProjectId, metadata.Project.Id);
            blobMetadataDictionary.Add(BlobMetadataKeys.ProjectName, metadata.Project.Name);
            blobMetadataDictionary.Add(BlobMetadataKeys.Description, string.IsNullOrWhiteSpace(metadata.Project.Description) ? metadata.Project.Name : metadata.Project.Description);
            blobMetadataDictionary.Add(BlobMetadataKeys.PublishDate, DateTime.UtcNow.ToString());
            StringBuilder sb = new StringBuilder();
            foreach (var form in metadata.Project.FormDigests)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.AppendFormat("{0}({1} page{2})", form.FormName, form.NumberOfPages, form.NumberOfPages == 1 ? "" : "s");
            }
            string forms = sb.ToString();
            blobMetadataDictionary.Add(BlobMetadataKeys.Forms, forms);
            var isUploadBlobSuccessful = _metadataBlobCRUD.UploadText(metadataWithDigestsJson, projectKey, blobMetadataDictionary);
            var metadataList = _metadataBlobCRUD.GetBlobListWithKeys(BlobMetadataKeys.ProjectName, BlobMetadataKeys.Forms);
            metadataList = _metadataBlobCRUD.GetBlobList(Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Metadata);
        }

        private void GenerateDigests(Template projectTemplateMetadata)
        {
            projectTemplateMetadata.Project.FormDigests = projectTemplateMetadata.ToFormDigests();
            projectTemplateMetadata.Project.FormPageDigests = projectTemplateMetadata.ToPageDigests();
        }


        private void PopulateRequiredPageLevelSourceTables(Template metadata)
        {
            foreach (var view in metadata.Project.Views)
            {
                var numberOfPages = view.Pages.Length;
                for (int i = 0; i < numberOfPages; ++i)
                {
                    var pageMetadata = view.Pages[i];
                    var pageId = pageMetadata.PageId.Value;
                    var fieldsRequiringSourceTable = pageMetadata.Fields.Where(f => !string.IsNullOrEmpty(f.SourceTableName));
                    foreach (var field in fieldsRequiringSourceTable)
                    {
                        field.SourceTableValues = metadata.SourceTables.Where(st => st.TableName == field.SourceTableName).First().Values;
                    }
                }
            }
        }
    }
}
