using System.Runtime.CompilerServices;
using System.Linq;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.CacheServices
{
    /// <summary>
    /// MetadataCache
    /// </summary>
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache, IMetadataCache
    {
        private const string MetadataKey = "Metadata";
        private const string PageSubKey = "#";
        private const string FormSubKey = "@";
        private const string FullSubKey = "!";

        private static Dictionary<string, Template> _dictionaryProjectMetadataObjectCache = new Dictionary<string, Template>();

        private string ComposeFullMetadataKey(Guid projectId)
        {
            return projectId.ToString("N") + FullSubKey;
        }
        private string ComposePageKey(Guid formId, int pageId)
        {
            return FormSubKey + (formId).ToString("N") + PageSubKey + pageId;
        }

        private string ComposePageFieldAttributesKey(Guid formId, int pageNumber)
        {
            return FormSubKey + formId.ToString("N") + PageSubKey + pageNumber;
        }

        public bool FullProjectTemplateMetadataExists(Guid projectId)
        {
            bool keyExists = true;
            string fullProjectMetadataKey = ComposeFullMetadataKey(projectId);
            Template metadata;
            lock (MetadataAccessor.StaticCache.Gate)
            {
                keyExists = _dictionaryProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata);
            }
            return keyExists;
        }

        public Template GetFullProjectTemplateMetadata(Guid projectId)
        {
            string fullProjectMetadataKey = ComposeFullMetadataKey(projectId);
            
            Template metadata = null;

            lock (MetadataAccessor.StaticCache.Gate)
            {
                _dictionaryProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata);
            }
            return metadata;
        }

        /// <summary>
        /// GetProjectTemplateMetadata
        /// Get the metadata for the specified project without page level metadata 
        /// unless an optional page id is provided.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="formId"></param>
        /// <param name="pageId"></param>
        /// <returns>ProjectTemplateMetadata</returns>
        public Template GetProjectTemplateMetadata(Guid projectId, Guid formId, int? pageId)
        {
            Template metadata = null;
            Template clonedMetadata = null;
            string fullProjectMetadataKey = ComposeFullMetadataKey(projectId);
            _dictionaryProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata);

            if (metadata != null)
            {
                clonedMetadata = metadata.Clone();
                if (formId != Guid.Empty && pageId.HasValue)
                {
                    var pageMetadata = GetPageMetadata(projectId, formId, pageId.Value);
                    clonedMetadata.Project.Views.Where(v => v.ViewId == pageMetadata.ViewId).Single().Pages = new Page[] { pageMetadata };
                }
            }
            return clonedMetadata;
        }

        public Template GetProjectTemplateMetadataByPageNumber(Guid projectId, Guid formId, int? pageNumber)
        {
            Template metadata = null;
            Template clonedMetadata = null;
            var projectMetadataWithOutPagesKey = ComposeFullMetadataKey(projectId);
            _dictionaryProjectMetadataObjectCache.TryGetValue(projectId.ToString("N"), out metadata);

            if (metadata != null)
            {
                clonedMetadata = metadata.Clone();
                if (pageNumber.HasValue)
                {
                    var pageId = metadata.Project.FormPageDigests.PageIdFromPageNumber(formId.ToString("N"), pageNumber.Value);
                    if (pageId != 0)
                    {
                        var pageMetadata = GetPageMetadata(projectId, formId, pageId);
                        clonedMetadata.Project.Views.Where(v => v.ViewId == pageMetadata.ViewId).Single().Pages = new Page[] { pageMetadata };
                    }
                }
            }
            return clonedMetadata;
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

                var fullMetadataKey = ComposeFullMetadataKey(projectId);
                _dictionaryProjectMetadataObjectCache[fullMetadataKey] = projectTemplateMetadata;

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

                json = JsonConvert.SerializeObject(projectTemplateMetadataClone, DontSerializeNulls);
                isSuccessful &= Set(projectId, MetadataKey, json).Result;
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
                _dictionaryProjectMetadataObjectCache.Remove(ComposeFullMetadataKey(projectId));
                _dictionaryProjectMetadataObjectCache.Remove(projectId.ToString("N"));
                DeleteAllKeys(projectId, MetadataKey);
            }
        }

        public void ClearAllMetadataFromCache()
        {
            lock (MetadataAccessor.StaticCache.Gate)
            {
                var projectId = Guid.Empty;
                DeleteAllKeys(projectId, MetadataKey);
                DeleteAllKeys(projectId, FormPageDigestsKey);
                DeleteAllKeys(projectId, FormDigestsKey);
            }
        }
    }
}
