using System.Runtime.CompilerServices;
using System.Linq;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Epi.FormMetadata.DataStructures;
using Epi.Common.Constants;

namespace Epi.Cloud.CacheServices
{
    /// <summary>
    /// MetadataCache
    /// </summary>
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache, IMetadataCache
    {
        private const string DeploymentPropertiesKey = "DeploymentProperties";
        private const string PageSubKey = "#";
        private const string FormSubKey = "@";

        private string ComposePageKey(Guid formId, int pageId)
        {
            return FormSubKey + (formId).ToString("N") + PageSubKey + pageId;
        }

        public Guid GetDeployedProjectId()
        {
            Guid projectId = Guid.Empty;

            string projectIdString;
            var deploymentProperties = GetDeploymentProperties(Guid.Empty);
            if (deploymentProperties.TryGetValue(BlobMetadataKeys.ProjectId, out projectIdString))
            {
                projectId = Guid.Parse(projectIdString);
            }
            return projectId;
        }

        public Dictionary<string, string> GetDeploymentProperties(Guid projectId)
        {
            Dictionary<string, string> deploymentProperties = null;
            var json = Get(projectId, DeploymentPropertiesKey).Result;
            if (json != null)
            {
                deploymentProperties = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            return deploymentProperties;
        }

        public bool SetDeploymentProperties(Dictionary<string, string> deploymentProperties)
        {
            bool isSuccessful = false;
            Guid projectId = Guid.Empty;
            string projectIdString;
            if (deploymentProperties.TryGetValue(BlobMetadataKeys.ProjectId, out projectIdString))
            {
                projectId = Guid.Parse(projectIdString);
            }

            var json = JsonConvert.SerializeObject(deploymentProperties, DontSerializeNulls);
            isSuccessful &= Set(projectId, DeploymentPropertiesKey, json).Result;
            isSuccessful &= Set(Guid.Empty, DeploymentPropertiesKey, json).Result;
            return isSuccessful;
        }

        public bool PageMetadataExists(Guid projectId, Guid formId, int pageId)
        {
            var pageKey = ComposePageKey(formId, pageId);
            bool keyExists = KeyExists(projectId, pageKey).Result;
            return keyExists;
        }

        /// <summary>
        /// GetPageMetadata
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="formId"></param>
        /// <param name="pageId"></param>
        /// <returns>PageMetadata</returns>
        public Page GetPageMetadata(Guid projectId, Guid formId, int pageId)
        {
            Page metadata = null;
            var pageKey = ComposePageKey(formId, pageId);
            var json = Get(projectId, pageKey).Result;
            if (json != null)
            {
                metadata = JsonConvert.DeserializeObject<Page>(json);
            }
            return metadata;
        }

        /// <summary>
        /// SetProjectTemplateMetadata
        /// </summary>
        /// <param name="projectTemplateMetadataClone"></param>
        /// <returns></returns>
        public bool SetProjectTemplateMetadata(Template projectTemplateMetadata)
        {
            bool isSuccessful = false;
            string json;
            lock (MetadataAccessor.StaticCache.Gate)
            {
                var projectId = new Guid(projectTemplateMetadata.Project.Id);

                // Cache the form digests
                SetFormDigests(projectId, projectTemplateMetadata.Project.FormDigests);
                // Cache the page digests
                SetProjectPageDigests(projectId, projectTemplateMetadata.Project.FormPageDigests);

                // Create a clone of the Template. We will make changes to the clone
                // that we don't want reflected in the original.
                var projectTemplateMetadataClone = projectTemplateMetadata.Clone();

                // save the pageMetadata list
                var pages = new Page[0];
                foreach (var view in projectTemplateMetadataClone.Project.Views)
                {
                    pages = pages.Union(view.Pages).ToArray();
                    // don't save the page metadata with the cached project metadata
                    view.Pages = new Page[0];
                }

                int numberOfPages = pages.Length;

                // Cache the metadata for each of the pages
                for (int i = 0; i < numberOfPages; ++i)
                {
                    var pageMetadata = pages[i];
                    var formId = new Guid(projectTemplateMetadataClone.Project.Views.Where(v => v.ViewId == pageMetadata.ViewId).Select(v => v.FormId).Single());
                    var pageId = pageMetadata.PageId.Value;
                    var fieldsRequiringSourceTable = pageMetadata.Fields.Where(f => !string.IsNullOrEmpty(f.SourceTableName));
                    json = JsonConvert.SerializeObject(pageMetadata, DontSerializeNulls);
                    var pageKey = ComposePageKey(formId, pageId);
                    isSuccessful &= Set(projectId, pageKey, json).Result;
                }

                var projectDeploymentProperties = new Dictionary<string, string>();
                projectDeploymentProperties.Add(BlobMetadataKeys.ProjectId, projectId.ToString("N"));
                projectDeploymentProperties.Add(BlobMetadataKeys.RootFormName, projectTemplateMetadataClone.Project.FormDigests.First().FormName);
                foreach(var deploymentProperty in projectTemplateMetadata.ProjectDeploymentProperties)
                {
                    projectDeploymentProperties[deploymentProperty.Key] = deploymentProperty.Value;
                }
                isSuccessful = SetDeploymentProperties(projectDeploymentProperties);
            }
            return isSuccessful;
        }

        /// <summary>
        /// ClearProjectMetadataFromCache
        /// </summary>
        /// <param name="projectId"></param>
        public void ClearProjectTemplateMetadataFromCache(Guid projectId)
        {
            lock (MetadataAccessor.StaticCache.Gate)
            {
                DeleteAllKeys(projectId, DeploymentPropertiesKey);
            }
        }

        public void ClearAllMetadataFromCache()
        {
            lock (MetadataAccessor.StaticCache.Gate)
            {
                var projectId = GetDeployedProjectId();
                DeleteAllKeys(projectId, FormPageDigestsKey);
                DeleteAllKeys(projectId, FormDigestsKey);

                Delete(projectId, DeploymentPropertiesKey);
                Delete(Guid.Empty, DeploymentPropertiesKey);
            }
        }
    }
}
