using System.Runtime.CompilerServices;
using System.Linq;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;
using System;

namespace Epi.Cloud.CacheServices
{
    /// <summary>
    /// MetadataCache
    /// </summary>
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache, IMetadataCache
    {
        private const string MetadataPrefix = "metadata_";
        private const string PagePrefix = "#";

        private ConditionalWeakTable<string, Template> _weakProjectMetadataObjectCache = new ConditionalWeakTable<string, Template>();
        private ConditionalWeakTable<string, Page> _weakPageMetadataObjectCache = new ConditionalWeakTable<string, Page>();

        private string ComposePageKey(string projectId, int pageId)
        {
            return projectId + PagePrefix + Convert.ToInt32(pageId);
        }

        public bool ProjectTemplateMetadataExists(string projectId)
        {
            bool keyExists = true;
            Template metadata;
            if (!_weakProjectMetadataObjectCache.TryGetValue(projectId, out metadata))
            {
                keyExists = KeyExists(MetadataPrefix, projectId).Result;
            }
            return keyExists;
        }

        /// <summary>
        /// GetProjectTemplateMetadata
        /// Get the metadata for the specified project without page level metadata 
        /// unless an optional page id is provided.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="pageId"></param>
        /// <returns>ProjectTemplateMetadata</returns>
        public Template GetProjectTemplateMetadata(string projectId, int? pageId = null)
        {
            Template metadata = null;
            Template clonedMetadata = null;
            if (!_weakProjectMetadataObjectCache.TryGetValue(projectId, out metadata))
            {
                var json = Get(MetadataPrefix, projectId).Result;
                if (json != null)
                {
                    metadata = JsonConvert.DeserializeObject<Template>(json);
                    _weakProjectMetadataObjectCache. Add(projectId, metadata);
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
            // Create a clone of the Template. We will make changes to the clone
            // that we don't want reflected in the original.
            var projectTemplateMetadataClone = projectTemplateMetadata.Clone();

            bool isSuccessful = false;
            string json;
            // save the pageMetadata list
            var pages = new Page[0];
            foreach (var view in projectTemplateMetadataClone.Project.Views)
            {
                pages = pages.Union(view.Pages).ToArray();
                // don't save the page metadata with the cached project metadata
                view.Pages = new Page[0];
            }

            int numberOfPages = pages.Length;
            var digest = new ProjectDigest[numberOfPages];

            // Cache the metadata for each of the pages and build the digest for the project
            for (int i = 0; i < numberOfPages; ++i)
            {
                var pageMetadata = pages[i];
                int viewId = pageMetadata.ViewId;
                int pageId = pageMetadata.PageId.Value;
                int position = pageMetadata.Position;
                string[] fieldNames = pageMetadata.Fields.Select(f => f.Name).ToArray();
                digest[i] = new ProjectDigest(viewId, pageId, position, fieldNames);
                var fieldsRequiringSourceTable = pageMetadata.Fields.Where(f => !string.IsNullOrEmpty(f.SourceTableName));
                foreach (var field in fieldsRequiringSourceTable)
                {
                    field.SourceTableItems = projectTemplateMetadataClone
                        .SourceTables.Where(st => st.TableName == field.SourceTableName).Single().Items;
                }
                json = JsonConvert.SerializeObject(pageMetadata);
                isSuccessful = Set(MetadataPrefix, ComposePageKey(projectTemplateMetadataClone.Project.Id, pageId), json).Result;
            }

            // save the project digest in the cached object
            projectTemplateMetadataClone.Project.Digest = digest;
            projectTemplateMetadata.Project.Digest = digest;

            json = JsonConvert.SerializeObject(projectTemplateMetadataClone);
            isSuccessful = Set(MetadataPrefix, projectTemplateMetadataClone.Project.Id, json).Result;
            return isSuccessful;
        }

        /// <summary>
        /// ClearProjectMetadataFromCache
        /// </summary>
        /// <param name="projectId"></param>
        public void ClearProjectTemplateMetadataFromCache(string projectId)
        {
            DeleteAllKeys(projectId, key =>
            {
                if (((string)key).Contains(PagePrefix)) _weakPageMetadataObjectCache.Remove(key);
                else _weakProjectMetadataObjectCache.Remove(key);
            });
        }

        public void ClearAllMetadataFromCache()
        {
            DeleteAllKeys(MetadataPrefix);
        }
    }
}
