using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public class MetadataCache : RedisCache
    {
        private const string MetadataPrefix = "metadata_";
        private const string ProjectPrefix = "project_";

        public MetadataCache() : base(MetadataPrefix)
        {
        }

        /// <summary>
        /// GetProjectTemplateMetadata
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public ProjectTemplateMetadata GetProjectTemplateMetadata(string projectName)
        {
            ProjectTemplateMetadata metadata = null;
            var json = Get(ProjectPrefix + projectName).Result;
            if (json != null)
            {
                metadata = JsonConvert.DeserializeObject<ProjectTemplateMetadata>(json);
            }
            return metadata;
        }

        /// <summary>
        /// GetPageMetadata
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public PageMetadata GetPageMetadata(string projectName, int pageId)
        {
            PageMetadata metadata = null;
            var projectTemplateMetadata = GetProjectTemplateMetadata(projectName);
            if (projectTemplateMetadata != null)
            {
                metadata = projectTemplateMetadata.Project.View.Pages.Where(p => p.PageId == pageId).Single();
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
            var json = JsonConvert.SerializeObject(projectTemplateMetadata);
            isSuccessful = Set(ProjectPrefix + projectTemplateMetadata.Project.Name, json).Result;
            return isSuccessful;
        }

        /// <summary>
        /// ClearProjectTemplateMetadataFromCache
        /// </summary>
        /// <param name="projectName"></param>
        public void ClearProjectTemplateMetadataFromCache(string projectName)
        {
            Delete(ProjectPrefix + projectName);
        }
    }
}
