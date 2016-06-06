using System.Runtime.CompilerServices;
using System.Linq;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    /// <summary>
    /// MetadataCache
    /// </summary>
    public class MetadataCache : RedisCache, IMetadataCache
    {
        private const string MetadataPrefix = "metadata_";

        private ConditionalWeakTable<string, ProjectTemplateMetadata> _weakProjectMetadataObjectCache = new ConditionalWeakTable<string, ProjectTemplateMetadata>();
        private ConditionalWeakTable<string, PageMetadata> _weakPageMetadataObjectCache = new ConditionalWeakTable<string, PageMetadata>();

        public MetadataCache() : base(MetadataPrefix)
        {
        }

        private string ComposePageKey(string projectName, int pageId)
        {
            return projectName + '#' + pageId;
        }

        /// <summary>
        /// GetProjectTemplateMetadata
        /// Get the metadata for the specified project without page level metadata 
        /// unless an optional page id is provided.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pageId"></param>
        /// <returns>ProjectTemplateMetadata</returns>
        public ProjectTemplateMetadata GetProjectTemplateMetadata(string projectName, int? pageId = null)
        {
            ProjectTemplateMetadata metadata = null;
            ProjectTemplateMetadata clonedMetadata = null;
            if (!_weakProjectMetadataObjectCache.TryGetValue(projectName, out metadata))
            {
                var json = Get(projectName).Result;
                if (json != null)
                {
                    metadata = JsonConvert.DeserializeObject<ProjectTemplateMetadata>(json);
                    _weakProjectMetadataObjectCache. Add(projectName, metadata);
                }
            }

            if (metadata != null)
            {
                clonedMetadata = metadata.Clone();
                if (pageId.HasValue)
                {
                    var pageMetadata = GetPageMetadata(projectName, pageId.Value);
                    clonedMetadata.Project.Pages = new PageMetadata[] { pageMetadata };
                }
            }
            return clonedMetadata;
        }

        /// <summary>
        /// GetPageMetadata
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pageId"></param>
        /// <returns>PageMetadata</returns>
        public PageMetadata GetPageMetadata(string projectName, int pageId)
        {
            PageMetadata metadata = null;
            var pageKey = ComposePageKey(projectName, pageId);
            if (!_weakPageMetadataObjectCache.TryGetValue(pageKey, out metadata))
            {
                var json = Get(pageKey).Result;
                if (json != null)
                {
                    metadata = JsonConvert.DeserializeObject<PageMetadata>(json);
                    _weakPageMetadataObjectCache.Add(pageKey, metadata);
                }
            }
            return metadata;
        }

        /// <summary>
        /// SetProjectTemplateMetadata
        /// </summary>
        /// <param name="projectTemplateMetadata"></param>
        /// <returns></returns>
        public bool SetProjectTemplateMetadata(ProjectTemplateMetadata projectTemplateMetadata)
        {
            bool isSuccessful = false;
            string json;
            // save the pageMetadata list
            var pages = projectTemplateMetadata.Project.Pages;

            int numberOfPages = pages.Length;
            int[] pageIds = new int[numberOfPages];

            // Cached the metadata for each of the pages and remember the pageIds
            for (int i = 0; i < numberOfPages; ++i)
            {
                var pageMetadata = pages[i];
                int pageId = pageMetadata.PageId;
                pageIds[i] = pageId;
                var fieldsRequiringSourceTable = pageMetadata.Fields.Where(f => !string.IsNullOrEmpty(f.SourceTableName));
                foreach (var field in fieldsRequiringSourceTable)
                {
                    field.SourceTableItems = projectTemplateMetadata
                        .SourceTables.Where(st => st.TableName == field.SourceTableName).Single().Items;
                }
                json = JsonConvert.SerializeObject(pageMetadata);
                isSuccessful = Set(ComposePageKey(projectTemplateMetadata.Project.Name, pageId), json).Result;
            }

            // save the page ids in the cached object
            projectTemplateMetadata.Project.PageIds = pageIds;

            // don't save the page metadata with the cached project metadata
            projectTemplateMetadata.Project.Pages = null;
            json = JsonConvert.SerializeObject(projectTemplateMetadata);
            isSuccessful = Set(projectTemplateMetadata.Project.Name, json).Result;

            // restore the page metadata 
            projectTemplateMetadata.Project.Pages = pages;
            return isSuccessful;
        }

        /// <summary>
        /// ClearProjectTemplateMetadataFromCache
        /// </summary>
        /// <param name="projectName"></param>
        public void ClearProjectTemplateMetadataFromCache(string projectName)
        {
            var metadata = GetProjectTemplateMetadata(projectName);
            if (metadata != null)
            {
                // remove each of the pages from cache
                foreach (var pageId in metadata.Project.PageIds)
                {
                    var pageKey = ComposePageKey(projectName, pageId);
                    _weakPageMetadataObjectCache.Remove(pageKey);
                    Delete(pageKey);
                }

                _weakProjectMetadataObjectCache.Remove(projectName);
                Delete(projectName);
            }
        }
    }
}
