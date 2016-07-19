using System.Runtime.CompilerServices;
using System.Linq;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Epi.Cloud.CacheServices
{
    /// <summary>
    /// MetadataCache
    /// </summary>
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache, IMetadataCache
    {
        private object _gate = new object();

        private const string MetadataKey = "Metadata";
        private const string PageSubKey = "#";
        private const string FormSubKey = "@";
        private const string FullSubKey = "!";

        private ConditionalWeakTable<string, Template> _weakProjectMetadataObjectCache = new ConditionalWeakTable<string, Template>();
        private ConditionalWeakTable<string, Page> _weakPageMetadataObjectCache = new ConditionalWeakTable<string, Page>();
        private static Dictionary<string, Template> _dictionaryProjectMetadataObjectCache = new Dictionary<string, Template>();

        private string ComposeFullMetadataKey(string projectId)
        {
            return projectId + FullSubKey;
        }
        private string ComposePageKey(string formId, int pageId)
        {
            return FormSubKey + (new Guid(formId)).ToString("N") + PageSubKey + pageId;
        }

        private string ComposePageFieldAttributesKey(string formId, int pageNumber)
        {
            return FormSubKey + (new Guid(formId)).ToString("N") + PageSubKey + pageNumber;
        }

        public bool FullProjectTemplateMetadataExists(string projectId)
        {
            bool keyExists = true;
            string fullProjectMetadataKey = ComposeFullMetadataKey(projectId);
            Template metadata;
            if (!_weakProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata))
            {
                lock (_gate)
                {
                    keyExists = _dictionaryProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata);
                }
            }
            return keyExists;
        }
        public Template GetFullProjectTemplateMetadata(string projectId)
        {
            string fullProjectMetadataKey = ComposeFullMetadataKey(projectId);
            
            Template metadata = null;

            if (!_weakProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata))
            {
                lock (_gate)
                {
                    _dictionaryProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata);
                }
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
        public Template GetProjectTemplateMetadata(string projectId, string formId, int? pageId)
        {
            Template metadata = null;
            Template clonedMetadata = null;
            string fullProjectMetadataKey = ComposeFullMetadataKey(projectId);
            if (!_weakProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata))
            {
                if (_dictionaryProjectMetadataObjectCache.TryGetValue(fullProjectMetadataKey, out metadata))
                {
                    _weakProjectMetadataObjectCache. Add(fullProjectMetadataKey, metadata);
                }
            }

            if (metadata != null)
            {
                clonedMetadata = metadata.Clone();
                if (!string.IsNullOrWhiteSpace(formId) && pageId.HasValue)
                {
                    var pageMetadata = GetPageMetadata(projectId, formId, pageId.Value);
                    clonedMetadata.Project.Views.Where(v => v.ViewId == pageMetadata.ViewId).Single().Pages = new Page[] { pageMetadata };
                }
            }
            return clonedMetadata;
        }

        public Template GetProjectTemplateMetadataByPageNumber(string projectId, string formId, int? pageNumber)
        {
            Template metadata = null;
            Template clonedMetadata = null;
            var projectMetadataWithOutPagesKey = ComposeFullMetadataKey(projectId);
            if (!_weakProjectMetadataObjectCache.TryGetValue(projectMetadataWithOutPagesKey, out metadata))
            {
                if (_dictionaryProjectMetadataObjectCache.TryGetValue(projectId, out metadata))
                {
                    _weakProjectMetadataObjectCache.Add(projectId, metadata);
                }
            }

            if (metadata != null)
            {
                clonedMetadata = metadata.Clone();
                if (pageNumber.HasValue)
                {
                    var pageId = metadata.Project.FormPageDigests.PageIdFromPageNumber(formId, pageNumber.Value);
                    if (pageId != 0)
                    {
                        var pageMetadata = GetPageMetadata(projectId, formId, pageId);
                        clonedMetadata.Project.Views.Where(v => v.ViewId == pageMetadata.ViewId).Single().Pages = new Page[] { pageMetadata };
                    }
                }
            }
            return clonedMetadata;
        }

        public bool PageMetadataExists(string projectId, string formId, int pageId)
        {
            bool keyExists = true;
            var pageKey = ComposePageKey(formId, pageId);
            Template metadata;
            if (!_weakProjectMetadataObjectCache.TryGetValue(pageKey, out metadata))
            {
                keyExists = KeyExists(new Guid(projectId), pageKey).Result;
            }
            return keyExists;
        }

        /// <summary>
        /// GetPageMetadata
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="formId"></param>
        /// <param name="pageId"></param>
        /// <returns>PageMetadata</returns>
        public Page GetPageMetadata(string projectId, string formId, int pageId)
        {
            Page metadata = null;
            var pageKey = ComposePageKey(formId, pageId);
            if (!_weakPageMetadataObjectCache.TryGetValue(pageKey, out metadata))
            {
                var json = Get(new Guid(projectId), pageKey).Result;
                if (json != null)
                {
                    metadata = JsonConvert.DeserializeObject<Page>(json);
                    _weakPageMetadataObjectCache.Add(pageKey, metadata);
                }
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
            lock (_gate)
            {
                var projectId = projectTemplateMetadata.Project.Id;

                var fullMetadataKey = ComposeFullMetadataKey(projectId);
                _weakProjectMetadataObjectCache.Remove(fullMetadataKey);
                _weakProjectMetadataObjectCache.Add(fullMetadataKey, projectTemplateMetadata);
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
                    var formId = projectTemplateMetadataClone.Project.Views.Where(v => v.ViewId == pageMetadata.ViewId).Select(v => v.FormId).Single();
                    var pageId = pageMetadata.PageId.Value;
                    var fieldsRequiringSourceTable = pageMetadata.Fields.Where(f => !string.IsNullOrEmpty(f.SourceTableName));
                    json = JsonConvert.SerializeObject(pageMetadata, DontSerializeNulls);
                    var pageKey = ComposePageKey(formId, pageId);
                    isSuccessful &= Set(new Guid(projectId), pageKey, json).Result;
                }

                json = JsonConvert.SerializeObject(projectTemplateMetadataClone, DontSerializeNulls);
                isSuccessful &= Set(new Guid(projectId), MetadataKey, json).Result;
            }
            return isSuccessful;
        }

        /// <summary>
        /// ClearProjectMetadataFromCache
        /// </summary>
        /// <param name="projectId"></param>
        public void ClearProjectTemplateMetadataFromCache(string projectId)
        {
            lock (_gate)
            {
                _dictionaryProjectMetadataObjectCache.Remove(ComposeFullMetadataKey(projectId));
                _dictionaryProjectMetadataObjectCache.Remove(projectId);
                DeleteAllKeys(new Guid(projectId), MetadataKey, key =>
                {
                    if (((string)key).Contains(PageSubKey)) _weakPageMetadataObjectCache.Remove(key);
                    else _weakProjectMetadataObjectCache.Remove(key);
                });
            }
        }

        public void ClearAllMetadataFromCache()
        {
            lock (_gate)
            {
                var projectId = Guid.Empty;
                DeleteAllKeys(projectId, MetadataKey);
                DeleteAllKeys(projectId, FormPageDigestsKey);
                DeleteAllKeys(projectId, FormDigestsKey);
            }
        }
    }
}
