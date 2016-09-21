using System.Threading.Tasks;
using System.Linq;
using Epi.Cloud.MetadataServices.ProxiesService;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.CacheServices;
using System.Collections.Generic;
using Epi.Cloud.Common;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using System;

namespace Epi.Cloud.MetadataServices
{
    public class ProjectMetadataProvider : IProjectMetadataProvider
    {
        private static Guid _projectId;
        private readonly IEpiCloudCache _epiCloudCache;
        public ProjectMetadataProvider(IEpiCloudCache epiCloudCache)
        {
            _epiCloudCache = epiCloudCache;
        }

        public IEpiCloudCache Cache { get { return _epiCloudCache; } }

        public string ProjectId { get { return _projectId.ToString("N"); } }

        //Pass the project id and call the DBAccess API and get the project metadata.

        public async Task<Template> GetProjectMetadataAsync(ProjectScope scope)
        {
            Template metadata = null;
            if (scope == ProjectScope.TemplateWithAllPages)
            {
                metadata = _projectId != null ? _epiCloudCache.GetFullProjectTemplateMetadata(_projectId) : null;
                if (metadata == null)
                {
                    metadata = await RefreshCache(_projectId);
                }
            }
            else if (scope == ProjectScope.TemplateWithNoPages)
            {
                metadata = _projectId != null ? _epiCloudCache.GetProjectTemplateMetadata(_projectId, Guid.Empty, null) : null;
                if (metadata == null)
                {
                    Template fullMetadata = _projectId != null ? _epiCloudCache.GetFullProjectTemplateMetadata(_projectId) : null;
                    if (fullMetadata == null)
                    {
                        fullMetadata = await RefreshCache(_projectId);
                        _projectId = new Guid(fullMetadata.Project.Id);
                        metadata = _epiCloudCache.GetProjectTemplateMetadata(_projectId, Guid.Empty, null);
                        if (metadata == null)
                        {
                            metadata = fullMetadata;
                        }
                    }
                }
            }
            return metadata;
        }

        public async Task<Template> GetProjectMetadataWithPageByPageIdAsync(string formId, int pageId)
        {
            var metadata = _projectId != null ? _epiCloudCache.GetProjectTemplateMetadata(_projectId, new Guid(formId), pageId) : null;
            if (metadata == null)
            {
                var fullMetadata = await RefreshCache(_projectId);
                metadata = _epiCloudCache.GetProjectTemplateMetadata(_projectId, new Guid(formId), pageId);
            }
            return metadata;
        }

        public Task<Template> GetProjectMetadataAsync(string formId, ProjectScope scope)
        {
            if (scope == ProjectScope.TemplateWithAllPages)
            {
                return GetProjectMetadataAsync(scope);
            }
            return GetProjectMetadataWithPageByPageNumberAsync(formId, null);
        }

        public async Task<Template> GetProjectMetadataWithPageByPageNumberAsync(string formId, int? pageNumber)
        {
            var metadata = _projectId != null ? _epiCloudCache.GetProjectTemplateMetadataByPageNumber(_projectId, new Guid(formId), pageNumber) : null;
            if (metadata == null)
            {
                var fullMetadata = await RefreshCache(_projectId);
                metadata = _epiCloudCache.GetProjectTemplateMetadata(_projectId, new Guid(formId), pageNumber);
            }
            return metadata;
        }

        public async Task<Page> GetPageMetadataAsync(string formId, int pageId)
        {
            var metadata = _epiCloudCache.GetPageMetadata(_projectId, new Guid(formId), pageId);
            if (metadata == null)
            {
                var fullMetadata = await RefreshCache(_projectId);
                metadata = _epiCloudCache.GetPageMetadata(_projectId, new Guid(formId), pageId);
            }
            return metadata;
        }

        public async Task<FormDigest[]> GetFormDigestsAsync()
        {
            var formDigests = _projectId != null ? _epiCloudCache.GetFormDigests(_projectId) : null;
            if (formDigests == null)
            {
                var fullMetadata = await RefreshCache(_projectId);

                formDigests = _epiCloudCache.GetFormDigests(_projectId);
            }
            return formDigests;
        }

        public async Task<FormDigest> GetFormDigestAsync(string formId)
        {
            var formDigests = _projectId != null ? _epiCloudCache.GetFormDigests(_projectId) : null;
            if (formDigests == null)
            {
                var fullMetadata = await RefreshCache(_projectId);

                formDigests = _epiCloudCache.GetFormDigests(_projectId);
            }
            var formDigest = formDigests != null ? formDigests.Where(d => CaseInsensitiveEqualityComparer.Instance.Equals(d.FormId, formId)).SingleOrDefault() : null;
            return formDigest;
        }

        public async Task<PageDigest[][]> GetProjectPageDigestsAsync()
        {
            var projectPageDigests = _projectId != null ? _epiCloudCache.GetProjectPageDigests(_projectId) : null;
            if (projectPageDigests == null)
            {
                var fullMetadata = await RefreshCache(_projectId);
                projectPageDigests = fullMetadata.Project.FormPageDigests;
            }
            return projectPageDigests;
        }

        public async Task<PageDigest[]> GetPageDigestsAsync(string formId)
        {
            var pageDigests = _epiCloudCache.GetPageDigests(_projectId, new Guid(formId));
            if (pageDigests == null)
            {
                var projectPageDigests = GetProjectPageDigestsAsync().Result;
                foreach (var projectPageDigest in projectPageDigests)
                {
                    if (projectPageDigest[0].FormId == formId)
                    {
                        pageDigests = projectPageDigest;
                    }
                }
            }
            return await Task.FromResult(pageDigests);
        }

        public async Task<FieldDigest> GetFieldDigestAsync(string formId, string fieldName)
        {
            fieldName = fieldName.ToLower();
            var pageDigests = await GetPageDigestsAsync(formId);

            foreach (var pageDigest in pageDigests)
            {
                var field = pageDigest.Fields.Where(f => f.FieldName.ToLower() == fieldName).SingleOrDefault();
                if (field != null)
                {
                    return new FieldDigest(field, pageDigest);
                }
            }
            return null;
        }

        public async Task<FieldDigest[]> GetFieldDigestsAsync(string formId)
        {
            formId = formId.ToLower();
            List<FieldDigest> fieldDigests = new List<FieldDigest>();
            var pageDigests = await GetPageDigestsAsync(formId);
            foreach (var pageDigest in pageDigests)
            {
                fieldDigests.AddRange(pageDigest.Fields.Select(field => new FieldDigest(field, pageDigest)));
            }
            return fieldDigests.ToArray();
        }

        public async Task<FieldDigest[]> GetFieldDigestsAsync(string formId, IEnumerable<string> fieldNames)
        {
            formId = formId.ToLower();
            List<string> fieldNameList = fieldNames.Select(n => n.ToLower()).ToList();
            List<string> remainingFieldNamesList = fieldNames.Select(n => n.ToLower()).ToList();
            List<FieldDigest> fieldDigests = new List<FieldDigest>();
            int fieldNamesCount = fieldNames.Count();
            var pageDigests = await GetPageDigestsAsync(formId);
            foreach (var pageDigest in pageDigests)
            {
                fieldNameList = remainingFieldNamesList.ToList();
                foreach (string fieldName in fieldNameList)
                {
                    AbridgedFieldInfo field = pageDigest.Fields.Where(f => f.FieldName.ToLower() == fieldName).SingleOrDefault();
                    if (field != null)
                    {
                        fieldDigests.Add(new FieldDigest(field, pageDigest));
                        remainingFieldNamesList.Remove(fieldName);
                    }
                }
                if (remainingFieldNamesList.Count == 0) break;
            }
            return fieldDigests.ToArray();
        }

        private async Task<Template> RefreshCache(Guid projectId)
        {
            Template metadata = await RetrieveProjectMetadata(projectId);
            PopulateRequiredPageLevelSourceTables(metadata);
            GenerateDigests(metadata);
            _epiCloudCache.SetProjectTemplateMetadata(metadata);
            return metadata;
        }

        private static async Task<Template> RetrieveProjectMetadata(Guid projectId)
        {
            ProjectMetadataServiceProxy serviceProxy = new ProjectMetadataServiceProxy();
            var templateMetadata = await serviceProxy.GetProjectMetadataAsync(projectId.ToString("N"));
            _projectId = templateMetadata != null ? new Guid(templateMetadata.Project.Id) : Guid.Empty;
            return templateMetadata;
        }

        private void GenerateDigests(Template projectTemplateMetadata)
        {
            projectTemplateMetadata.Project.FormDigests = GenerateFormDigests(projectTemplateMetadata);
            projectTemplateMetadata.Project.FormPageDigests = GeneratePageDigests(projectTemplateMetadata);
        }

        private static FormDigest[] GenerateFormDigests(Template projectTemplateMetadata)
        {
            var formDigests = new List<FormDigest>();
            foreach (var view in projectTemplateMetadata.Project.Views)
            {
                formDigests.Add(new FormDigest
                {
                    ViewId = view.ViewId,
                    FormId = view.FormId,
                    FormName = view.Name,
                    ParentFormId = view.ParentFormId,
                    OrganizationId = view.OrganizationId,
                    OrganizationName = view.OrganizationName,
                    OrganizationKey = view.OrganizationKey,
                    OwnerUserId = view.OwnerUserId,

                    NumberOfPages = view.Pages.Length,
                    Orientation = view.Orientation,
                    Height = view.Height.HasValue ? view.Height.Value : 0,
                    Width = view.Width.HasValue ? view.Width.Value : 0,

                    CheckCode = view.CheckCode,

                    DataAccessRuleId = view.DataAccessRuleId,
                    IsDraftMode = view.IsDraftMode
                });
            }

            return formDigests.ToArray();
        }

        private static PageDigest[][] GeneratePageDigests(Template projectTemplateMetadata)
        {
            List<PageDigest[]> projectPageDigests = new List<PageDigest[]>();
            var viewIdToViewMap = new Dictionary<int, View>();
            foreach (var view in projectTemplateMetadata.Project.Views)
            {
                viewIdToViewMap[view.ViewId] = view;
                var pages = new Page[0];
                pages = pages.Union(view.Pages).ToArray();
                int numberOfPages = pages.Length;
                var pageDigests = new PageDigest[numberOfPages];
                for (int i = 0; i < numberOfPages; ++i)
                {
                    var pageMetadata = pages[i];
                    string pageName = pageMetadata.Name;
                    int pageId = pageMetadata.PageId.Value;
                    int position = pageMetadata.Position;
                    int viewId = pageMetadata.ViewId;
                    bool isRelatedView = viewIdToViewMap[viewId].IsRelatedView;
                    string formId = viewIdToViewMap[viewId].FormId;
                    string formName = viewIdToViewMap[viewId].Name;
                    pageDigests[i] = new PageDigest(pageName, pageId, position, formId, formName, viewId, isRelatedView, pageMetadata.Fields);
                }
                projectPageDigests.Add(pageDigests);
            }

            return projectPageDigests.ToArray();
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
