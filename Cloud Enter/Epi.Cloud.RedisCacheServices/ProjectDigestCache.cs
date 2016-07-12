using System.Linq;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : IEpiCloudCache, IProjectDigestCache
    {
        private const string ProjectDigestPrefix = "projectDigest_";

        public bool ProjectDigestExists(string projectId)
        {
            return KeyExists(ProjectDigestPrefix, projectId, NoTimeout).Result;
        }

        public ProjectDigest[] GetProjectDigest(string projectId)
        {
            ProjectDigest[] projectDigest = null;
            var json = Get(ProjectDigestPrefix, projectId, NoTimeout).Result;
            if (json != null)
            {
                projectDigest = JsonConvert.DeserializeObject<ProjectDigest[]>(json);
            }
            return projectDigest;
        }

        public bool SetProjectDigest(string projectId, ProjectDigest[] projectDigest)
        {
            var json = JsonConvert.SerializeObject(projectDigest);
            foreach (var formId in projectDigest.Select(d => d.FormId).Distinct())
            {
                SetSurveyIdProjectIdMap(formId, projectId);
            }
            return Set(ProjectDigestPrefix, projectId, json).Result;
        }
        public void ClearProjectDigest(string projectId)
        {
            Delete(ProjectDigestPrefix, projectId);
        }
    }
}
