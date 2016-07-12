using System.Threading.Tasks;
using System.Linq;
using Epi.Cloud.MetadataServices.ProxiesService;
using Epi.Cloud.Common.Metadata;
using System;
using Epi.Cloud.CacheServices;
using System.Collections.Generic;

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

        public async Task<Template> GetProjectMetadataAsync(string projectId, ProjectScope scope = ProjectScope.TemplateWithAllPages)
        {
            Template metadata = null;
            if (scope == ProjectScope.TemplateWithAllPages)
            {
                metadata = projectId != null ? _epiCloudCache.GetFullProjectTemplateMetadata(projectId) : null;
                if (metadata == null)
                {
                    metadata = await RefreshCache(projectId);
                    PopulateRequiredPageLevelSourceTables(metadata);
                }
            }
            else if (scope == ProjectScope.TemplateWithNoPages)
            {
                metadata = projectId != null ? _epiCloudCache.GetProjectTemplateMetadata(projectId, null) : null;
                if (metadata == null)
                {
                    Template fullMetadata = projectId != null ? _epiCloudCache.GetFullProjectTemplateMetadata(projectId) : null;
                    if (fullMetadata == null)
                    {
                        fullMetadata = await RefreshCache(projectId);
                        projectId = fullMetadata.Project.Id;
                        metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId, null);
                        if (metadata == null)
                        {
                            metadata = fullMetadata;
                        }
                    }
                }
            }
            return metadata;
        }

        public async Task<Template> GetProjectMetadataWithPageByPageIdAsync(string projectId, int pageId)
        {
            var metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId, pageId);
            if (metadata == null)
            {
                var fullMetadata = await RefreshCache(projectId);
                metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId, pageId);
            }
            return metadata;
        }

        public Task<Template> GetProjectMetadataAsync(string projectId, string formId, ProjectScope scope = ProjectScope.TemplateWithNoPages)
        {
            if (scope == ProjectScope.TemplateWithAllPages)
            {
                return GetProjectMetadataAsync(projectId, scope);
            }
            return GetProjectMetadataWithPageByPageNumberAsync(projectId, formId, null);
        }

        public async Task<Template> GetProjectMetadataWithPageByPageNumberAsync(string projectId, string formId, int? pageNumber)
        {
            var metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId, formId, pageNumber);
            if (metadata == null)
            {
                var fullMetadata = await RefreshCache(projectId);
                metadata = _epiCloudCache.GetProjectTemplateMetadata(projectId, formId, pageNumber);
            }
            return metadata;
        }

        public async Task<ProjectDigest[]> GetProjectDigestAsync(string projectId)
        {
            var projectDigest = _epiCloudCache.GetProjectDigest(projectId);
            if (projectDigest == null)
            {
                var fullMetadata = await RefreshCache(projectId);
                projectDigest = fullMetadata.Project.Digest;
            }
            return projectDigest;
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
        private static void GenerateDigest(Template projectTemplateMetadata)
        {
            var viewIdToViewMap = new Dictionary<int, View>();
            var pages = new Page[0];
            foreach (var view in projectTemplateMetadata.Project.Views)
            {
                viewIdToViewMap[view.ViewId] = view;
                pages = pages.Union(view.Pages).ToArray();
            }

            int numberOfPages = pages.Length;
            var digest = new ProjectDigest[numberOfPages];
            for (int i = 0; i < numberOfPages; ++i)
            {
                var pageMetadata = pages[i];
                int viewId = pageMetadata.ViewId;
                bool isRelatedView = viewIdToViewMap[viewId].IsRelatedView;
                string formName = viewIdToViewMap[viewId].Name;
                string formId = viewIdToViewMap[viewId].EWEFormId;
                int pageId = pageMetadata.PageId.Value;
                int position = pageMetadata.Position;
                string[] fieldNames = pageMetadata.Fields.Select(f => f.Name).ToArray();
                digest[i] = new ProjectDigest(formName, formId, viewId, isRelatedView, pageId, position, fieldNames);
            }
            projectTemplateMetadata.Project.Digest = digest;
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
