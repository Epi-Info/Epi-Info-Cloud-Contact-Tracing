using System;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : IEpiCloudCache, IPageDigestCache
    {
        private const string ProjectPageDigestsKey = "ProjectPageDigests";
        private const string FormPageDigestsKey = "FormPageDigests";

        private string ComposeFormPageDigestsKey(Guid formId)
        {
            return FormPageDigestsKey + FormSubKey + formId.ToString("N");
        }

        public bool ProjectPageDigestsExists(Guid projectId)
        {
            return KeyExists(projectId, ProjectPageDigestsKey, NoTimeout).Result;
        }

        public PageDigest[][] GetProjectPageDigests(Guid projectId)
        {
            PageDigest[][] projectPageDigest = null;
            var json = Get(projectId, ProjectPageDigestsKey, NoTimeout).Result;
            if (json != null)
            {
                projectPageDigest = JsonConvert.DeserializeObject<PageDigest[][]>(json);
            }
            return projectPageDigest;
        }

        public bool SetProjectPageDigests(Guid projectId, PageDigest[][] projectPageDigests)
        {
            var json = JsonConvert.SerializeObject(projectPageDigests, DontSerializeNulls);
            var result = Set(projectId, ProjectPageDigestsKey, json).Result;

            foreach (var formPageDigests in projectPageDigests)
            {
                var formId = new Guid(formPageDigests[0].FormId);
                result = result & SetPageDigests(projectId, formId, formPageDigests);
            }
            return result;
        }

        public bool PageDigestsExists(Guid projectId, Guid formId)
        {
            return KeyExists(projectId, ComposeFormPageDigestsKey(formId), NoTimeout).Result;
        }

        public PageDigest[] GetPageDigests(Guid projectId, Guid formId)
        {
            PageDigest[] pageDigest = null;
            var json = Get(projectId, ComposeFormPageDigestsKey(formId), NoTimeout).Result;
            if (json != null)
            {
                pageDigest = JsonConvert.DeserializeObject<PageDigest[]>(json);
            }
            return pageDigest;
        }

        public bool SetPageDigests(Guid projectId, Guid formId, PageDigest[] formPageDigests)
        {
            var json = JsonConvert.SerializeObject(formPageDigests, DontSerializeNulls);
            return Set(projectId, ComposeFormPageDigestsKey(formId), json).Result;
        }

        public void ClearAllFormPageDigests(Guid projectId)
        {
            Delete(projectId, FormPageDigestsKey);
        }
    }
}
