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

        private const string MetadataPrefix = "metadata_";
        private const string PagePrefix = "#";
        private const string FormPrefix = "@";

        private ConditionalWeakTable<string, Template> _weakProjectMetadataObjectCache = new ConditionalWeakTable<string, Template>();
        private ConditionalWeakTable<string, Page> _weakPageMetadataObjectCache = new ConditionalWeakTable<string, Page>();
        private static Dictionary<string, Template> _dictionaryProjectMetadataObjectCache = new Dictionary<string, Template>();

        private string ComposeFullMetadataKey(string projectId)
        {
            return projectId + '!';
        }
        private string ComposePageKey(string projectId, int pageId)
        {
            return projectId + PagePrefix + Convert.ToInt32(pageId);
        }

        private string ComposePageFieldAttributesKey(string projectId, string formId, int pageNumber)
        {
            return projectId + FormPrefix + formId + PagePrefix + Convert.ToInt32(pageNumber);
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
        /// <param name="pageId"></param>
        /// <returns>ProjectTemplateMetadata</returns>
        public Template GetProjectTemplateMetadata(string projectId, int? pageId)
        {
            Template metadata = null;
            Template clonedMetadata = null;
            var projectMetadataWithOutPagesKey = projectId;
            if (!_weakProjectMetadataObjectCache.TryGetValue(projectMetadataWithOutPagesKey, out metadata))
            {
                var json = Get(MetadataPrefix, projectMetadataWithOutPagesKey).Result;
                if (json != null)
                {
                    metadata = JsonConvert.DeserializeObject<Template>(json);
                    _weakProjectMetadataObjectCache. Add(projectMetadataWithOutPagesKey, metadata);
                }
            }

            if (metadata != null)
            {
                clonedMetadata = metadata.Clone();
                if (pageId.HasValue)
                {
                    var pageMetadata = GetPageMetadata(projectId, pageId.Value);
                    clonedMetadata.Project.Views.Where(v => v.ViewId == pageMetadata.ViewId).Single().Pages = new Page[] { pageMetadata };
                }
            }
            return clonedMetadata;
        }
        public Template GetProjectTemplateMetadata(string projectId, string formId, int? pageNumber)
        {
            Template metadata = null;
            Template clonedMetadata = null;
            var projectMetadataWithOutPagesKey = projectId;
            if (!_weakProjectMetadataObjectCache.TryGetValue(projectMetadataWithOutPagesKey, out metadata))
            {
                var json = Get(MetadataPrefix, projectMetadataWithOutPagesKey).Result;
                if (json != null)
                {
                    metadata = JsonConvert.DeserializeObject<Template>(json);
                    _weakProjectMetadataObjectCache.Add(projectMetadataWithOutPagesKey, metadata);
                }
            }

            if (metadata != null)
            {
                clonedMetadata = metadata.Clone();
                if (pageNumber.HasValue)
                {
                    var pageId = metadata.PageIdFromPageNumber(formId, pageNumber.Value);
                    if (pageId != 0)
                    {
                        var pageMetadata = GetPageMetadata(projectId, pageId);
                        clonedMetadata.Project.Views.Where(v => v.ViewId == pageMetadata.ViewId).Single().Pages = new Page[] { pageMetadata };
                    }
                }
            }
            return clonedMetadata;
        }

        public bool PageMetadataExists(string projectId, int pageId)
        {
            bool keyExists = true;
            var pageKey = ComposePageKey(projectId, pageId);
            Template metadata;
            if (!_weakProjectMetadataObjectCache.TryGetValue(pageKey, out metadata))
            {
                keyExists = KeyExists(MetadataPrefix, pageKey).Result;
            }
            return keyExists;
        }

        /// <summary>
        /// GetPageMetadata
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="pageId"></param>
        /// <returns>PageMetadata</returns>
        public Page GetPageMetadata(string projectId, int pageId)
        {
            Page metadata = null;
            var pageKey = ComposePageKey(projectId, pageId);
            if (!_weakPageMetadataObjectCache.TryGetValue(pageKey, out metadata))
            {
                var json = Get(MetadataPrefix, pageKey).Result;
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

                // Cache the project digest
                SetProjectDigest(projectId, projectTemplateMetadata.Project.Digest);

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
                    var pageId = pageMetadata.PageId.Value;
                    var fieldsRequiringSourceTable = pageMetadata.Fields.Where(f => !string.IsNullOrEmpty(f.SourceTableName));
                    foreach (var field in fieldsRequiringSourceTable)
                    {
                        field.SourceTableItems = projectTemplateMetadataClone
                            .SourceTables.Where(st => st.TableName == field.SourceTableName).Single().Items;
                    }
                    json = JsonConvert.SerializeObject(pageMetadata, DontSerializeNulls);
                    var pageKey = ComposePageKey(projectId, pageId);
                    isSuccessful &= Set(MetadataPrefix, pageKey, json).Result;
                }

                json = JsonConvert.SerializeObject(projectTemplateMetadataClone, DontSerializeNulls);
                isSuccessful &= Set(MetadataPrefix, projectId, json).Result;
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
                DeleteAllKeys(MetadataPrefix + projectId, key =>
                {
                    if (((string)key).Contains(PagePrefix)) _weakPageMetadataObjectCache.Remove(key);
                    else _weakProjectMetadataObjectCache.Remove(key);
                });
            }
        }

        public void ClearAllMetadataFromCache()
        {
            lock (_gate)
            {
                DeleteAllKeys(MetadataPrefix);
                DeleteAllKeys(ProjectDigestPrefix);
            }
        }

        public bool PageFieldAttributesExists(string projectId, string formId, int pageNumber)
        {
            bool keyExists = true;
            var pageFieldAttributesKey = ComposePageFieldAttributesKey(projectId, formId, pageNumber);
            keyExists = KeyExists(MetadataPrefix, pageFieldAttributesKey).Result;
            return keyExists;
        }

        public FieldAttributes[] GetPageFieldAttributes(string projectId, string formId, int pageNumber)
        {
            FieldAttributes[] fieldAttributesArray = null;
            var pageFieldAttributesKey = ComposePageFieldAttributesKey(projectId, formId, pageNumber);
            var json = Get(MetadataPrefix, pageFieldAttributesKey).Result;
            if (json != null)
            {
                fieldAttributesArray = JsonConvert.DeserializeObject<FieldAttributes[]>(json);
            }
            return fieldAttributesArray;
        }
 
        public bool SetPageFieldAttributes(FieldAttributes[] fieldAttributes, string projectId, string formId, int pageNumber)
        {
            bool isSuccessful = false;
            string json;
            lock (_gate)
            {
                json = JsonConvert.SerializeObject(fieldAttributes, DontSerializeNulls);
                var pageFieldAttributesKey = ComposePageFieldAttributesKey(projectId, formId, pageNumber);
                isSuccessful = Set(MetadataPrefix, pageFieldAttributesKey, json).Result;
            }
            return isSuccessful;
        }
        public void ClearPageFieldAttributesFromCache(string projectId)
        {
            DeleteAllKeys(MetadataPrefix + projectId + FormPrefix);
        }
    }
}
