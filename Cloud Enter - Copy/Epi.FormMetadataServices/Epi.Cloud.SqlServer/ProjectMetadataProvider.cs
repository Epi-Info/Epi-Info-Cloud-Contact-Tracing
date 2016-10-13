using System.Threading.Tasks;
using System.Linq;
using Epi.Cloud.MetadataServices.ProxiesService;
using Epi.Cloud.Common.Metadata;
using System;
using Epi.Cloud.CacheServices;

namespace Epi.Cloud.MetadataServices
{
    public class ProjectMetadataProvider : IProjectMetadataProvider
    {
        private readonly IEpiCloudCache _epiCloudCache;
        public ProjectMetadataProvider(IEpiCloudCache epiCloudCache)
        {
            _epiCloudCache = epiCloudCache;
        }

        //Pass the project id and call the DBAccess API and get the project metadata.

        public async Task<Template> GetProjectMetadataAsync(string projectId, ProjectElement elements = ProjectElement.Full)
        {
            Template metadata = null;
            if (elements == ProjectElement.Full)
            {
                metadata = await RefreshCache(projectId);
                PopulateRequiredPageLevelSourceTables(metadata);
            }
            else if (elements == ProjectElement.TemplateWithoutPages)
            {
                Template fullMetadata = null;
                metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId);
                if (metadata == null)
                {
                    fullMetadata = await RefreshCache(projectId);
                    metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId);
                    if (metadata == null)
                    {
                        metadata = fullMetadata;
                    }
                }
            }
            return metadata;
        }

        private async Task<Template> RefreshCache(string projectId)
        {
            Template metadata = await RetrieveProjectMetadata(projectId);
            GenerateDigest(metadata);
            _epiCloudCache.SetProjectTemplateMetadata(metadata);
            return metadata;
        }

        private static async Task<Template> RetrieveProjectMetadata(string projectId)
        {
            ProjectMetadataServiceProxy serviceProxy = new ProjectMetadataServiceProxy();
            var templateMetadata = await serviceProxy.GetProjectMetadataAsync(projectId);
            return templateMetadata;
        }

        public async Task<Template> GetProjectMetadataAsync(string projectId, int pageId)
        {
            var metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId, pageId);
            if (metadata == null)
            {
                var fullMetadata = await RefreshCache(projectId);
                metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId, pageId);
            }
            return metadata;
        }

        private static void GenerateDigest(Template projectTemplateMetadata)
        {
            var pages = new Page[0];
            foreach (var view in projectTemplateMetadata.Project.Views)
            {
                pages = pages.Union(view.Pages).ToArray();
                // don't save the page metadata with the cached project metadata
                view.Pages = new Page[0];
            }

            int numberOfPages = pages.Length;
            var digest = new ProjectDigest[numberOfPages];
            for (int i = 0; i < numberOfPages; ++i)
            {
                var pageMetadata = pages[i];
                int viewId = pageMetadata.ViewId;
                int pageId = pageMetadata.PageId.Value;
                int position = pageMetadata.Position;
                string[] fieldNames = pageMetadata.Fields.Select(f => f.Name).ToArray();
                digest[i] = new ProjectDigest(viewId, pageId, position, fieldNames);
            }
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
                        field.SourceTableItems = metadata
                            .SourceTables.Where(st => st.TableName == field.SourceTableName).Single().Items;
                    }
                }
            }
        }
    }
 }
