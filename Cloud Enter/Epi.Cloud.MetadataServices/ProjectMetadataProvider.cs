using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epi.Cloud.CacheServices;
using Epi.Cloud.Common;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.MetadataServices.Common;
using Epi.Common.Constants;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.MetadataServices
{
    public class ProjectMetadataProvider : IProjectMetadataProvider
    {
        private static object ConcurrencyGate = new object();

        private static Guid _projectGuid;
        private readonly IEpiCloudCache _epiCloudCache;

        public ProjectMetadataProvider(IEpiCloudCache epiCloudCache)
        {
            _epiCloudCache = epiCloudCache;
        }

        public IEpiCloudCache Cache { get { return _epiCloudCache; } }

        public string ProjectId { get { return _projectGuid.ToString("N"); } }

        public Guid ProjectGuid { get { return _projectGuid; } }


        public string GetProjectId_RetrieveProjectIfNecessary()
        {
            lock (ConcurrencyGate)
            {
                if (_projectGuid == Guid.Empty)
                {
                    if (DoesCacheNeedToBeRefreshed(Guid.Empty, out _projectGuid))
                    {
                        var metadata = RefreshCache(Guid.Empty);
                    }
                }
                return ProjectId;
            }
        }

        public async Task<Template> RetrieveProjectMetadataAsync(Guid projectId)
        {
            var metadataProvider = new MetadataProvider();
            Template metadata = await metadataProvider.RetrieveProjectMetadataAsync(projectId);
            return metadata;
        }

        public async Task<Page> GetPageMetadataAsync(string formId, int pageId)
        {
            var metadata = _epiCloudCache.GetPageMetadata(ProjectGuid, new Guid(formId), pageId);
            if (metadata == null)
            {
                var fullMetadata = RefreshCache(ProjectGuid);
                metadata = _epiCloudCache.GetPageMetadata(ProjectGuid, new Guid(formId), pageId);
            }
            return await Task.FromResult(metadata);
        }

        public async Task<FormDigest[]> GetFormDigestsAsync()
        {
            var formDigests = ProjectId != null ? _epiCloudCache.GetFormDigests(ProjectGuid) : null;
            if (formDigests == null)
            {
                var fullMetadata = RefreshCache(ProjectGuid);

                formDigests = _epiCloudCache.GetFormDigests(ProjectGuid);
            }
            return await Task.FromResult(formDigests);
        }

        public async Task<FormDigest> GetFormDigestAsync(string formId)
        {
            var formDigests = ProjectId != null ? _epiCloudCache.GetFormDigests(ProjectGuid) : null;
            if (formDigests == null)
            {
                var fullMetadata = RefreshCache(ProjectGuid);

                formDigests = _epiCloudCache.GetFormDigests(ProjectGuid);
            }
            var formDigest = formDigests != null ? formDigests.Where(d => CaseInsensitiveEqualityComparer.Instance.Equals(d.FormId, formId)).SingleOrDefault() : null;
            return await Task.FromResult(formDigest);
        }

        public async Task<PageDigest[][]> GetProjectPageDigestsAsync()
        {
            var projectPageDigests = ProjectId != null ? _epiCloudCache.GetProjectPageDigests(ProjectGuid) : null;
            if (projectPageDigests == null)
            {
                var fullMetadata = RefreshCache(ProjectGuid);
                projectPageDigests = fullMetadata.Project.FormPageDigests;
            }
            return await Task.FromResult(projectPageDigests);
        }

        public async Task<PageDigest[]> GetPageDigestsAsync(string formId)
        {
            var pageDigests = _epiCloudCache.GetPageDigests(ProjectGuid, new Guid(formId));
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

        bool DoesCacheNeedToBeRefreshed(Guid projectId, out Guid cachedProjectId)
        {
            var cacheIsUpToDate = false;
            cachedProjectId = Guid.Empty;
            lock (ConcurrencyGate)
            {
                var cachedDeploymentProperties = Cache.GetDeploymentProperties(projectId);
                if (cachedDeploymentProperties != null)
                {
                    cachedProjectId = Guid.Parse(cachedDeploymentProperties[BlobMetadataKeys.ProjectId]);
                    string cachedPublishDate;
                    if (cachedDeploymentProperties.TryGetValue(BlobMetadataKeys.PublishDate, out cachedPublishDate))
                    {
                        var metadataProvider = new MetadataProvider();
                        var mostRecentDeploymentProperties = metadataProvider.GetMostRecentDeploymentPropertiesAsync().Result;
                        cacheIsUpToDate = mostRecentDeploymentProperties != null && cachedDeploymentProperties != null
                            && mostRecentDeploymentProperties[BlobMetadataKeys.PublishDate] == cachedPublishDate;
                        if (projectId != Guid.Empty)
                        {
                            cacheIsUpToDate &= (projectId == Guid.Parse(mostRecentDeploymentProperties[BlobMetadataKeys.ProjectId]));
                        }
                    }
                }
            }
            return !cacheIsUpToDate;
        }

        private Template RefreshCache(Guid projectId)
        {
            lock (ConcurrencyGate)
            {
                var metadataProvider = new MetadataProvider();
                Template metadata = metadataProvider.RetrieveProjectMetadataAsync(projectId).Result;
                if (metadata != null) _projectGuid = Guid.Parse(metadata.Project.Id);
                _epiCloudCache.SetProjectTemplateMetadata(metadata);
                return metadata;
            }
        }
    }
}
