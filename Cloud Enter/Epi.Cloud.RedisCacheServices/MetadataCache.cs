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
        private const string MetadataPrefix = "metadata_";

        private ConditionalWeakTable<string, Template> _weakProjectMetadataObjectCache = new ConditionalWeakTable<string, Template>();
        private ConditionalWeakTable<string, Page> _weakPageMetadataObjectCache = new ConditionalWeakTable<string, Page>();

        private string ComposePageKey(string projectName, int pageId)
        {
            return projectName + '#' + Convert.ToInt32(pageId);
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
        /// <param name="projectTemplateMetadata"></param>
        /// <returns></returns>
        public bool SetProjectTemplateMetadata(Template projectTemplateMetadata)
        {
            bool isSuccessful = false;
            string json;
            // save the pageMetadata list
            var pages = new Page[0];
            foreach (var view in projectTemplateMetadata.Project.Views)
            {
                pages = pages.Union(view.Pages).ToArray();
                // don't save the page metadata with the cached project metadata
                view.Pages = new Page[0];
            }

            int numberOfPages = pages.Length;
            var pageIdInfo = new Tuple<int, int, int>[numberOfPages];

            // Cached the metadata for each of the pages and remember the pageIds
            for (int i = 0; i < numberOfPages; ++i)
            {
                var pageMetadata = pages[i];
                int viewId = pageMetadata.ViewId;
                int pageId = pageMetadata.PageId.Value;
                int position = pageMetadata.Position;
                pageIdInfo[i] = new Tuple<int, int, int>(viewId, pageId, position);
                var fieldsRequiringSourceTable = pageMetadata.Fields.Where(f => !string.IsNullOrEmpty(f.SourceTableName));
                foreach (var field in fieldsRequiringSourceTable)
                {
                    field.SourceTableItems = projectTemplateMetadata
                        .SourceTables.Where(st => st.TableName == field.SourceTableName).Single().Items;
                }
                json = JsonConvert.SerializeObject(pageMetadata);
                isSuccessful = Set(MetadataPrefix, ComposePageKey(projectTemplateMetadata.Project.Name, pageId), json).Result;
            }

            // save the page ids in the cached object
            projectTemplateMetadata.Project.PageIdInfo = pageIdInfo;

            json = JsonConvert.SerializeObject(projectTemplateMetadata);
            isSuccessful = Set(MetadataPrefix, projectTemplateMetadata.Project.Id, json).Result;

            // restore the page metadata 
            foreach (var view in projectTemplateMetadata.Project.Views)
            {
                view.Pages = pages.Where(p => p.ViewId == view.ViewId).ToArray();
            }
            return isSuccessful;
        }

        /// <summary>
        /// ClearProjectTemplateMetadataFromCache
        /// </summary>
        /// <param name="projectId"></param>
        public void ClearProjectTemplateMetadataFromCache(string projectId)
        {
            var metadata = GetProjectTemplateMetadata(projectId);
            if (metadata != null)
            {
                // remove each of the pages from cache
                // PageIdInfo - Tuple - ViewId, PageId, Position
                foreach (var pageIdInfo in metadata.Project.PageIdInfo)
                {
                    var pageKey = ComposePageKey(projectId, pageIdInfo.Item2 /* PageId */);
                    _weakPageMetadataObjectCache.Remove(pageKey);
                    Delete(MetadataPrefix, pageKey);
                }

                _weakProjectMetadataObjectCache.Remove(projectId);
                Delete(MetadataPrefix, projectId);
            }
        }
    }
}
