using System;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : IEpiCloudCache, IPageDigestCache
    {
        private const string ProjectPageDigestsKey = "ProjectPageDigests";
        private const string FormPageDigestsKey = "FormPageDigests";

        private string ComposeFormPageDigestsKey(string formId)
        {
            return FormPageDigestsKey + FormSubKey + formId;
        }

        public bool ProjectPageDigestsExists(string projectId)
        {
            return KeyExists(new Guid(projectId), ProjectPageDigestsKey, NoTimeout).Result;
        }

        public PageDigest[][] GetProjectPageDigests(string projectId)
        {
            PageDigest[][] projectPageDigest = null;
            var json = Get(new Guid(projectId), ProjectPageDigestsKey, NoTimeout).Result;
            if (json != null)
            {
                projectPageDigest = JsonConvert.DeserializeObject<PageDigest[][]>(json);
            }
            return projectPageDigest;
        }

        public bool SetProjectPageDigests(string projectId, PageDigest[][] projectPageDigests)
        {
            var json = JsonConvert.SerializeObject(projectPageDigests, DontSerializeNulls);
            var result = Set(new Guid(projectId), ProjectPageDigestsKey, json).Result;

            foreach (var formPageDigests in projectPageDigests)
            {
                var formId = formPageDigests[0].FormId;
                result = result & SetPageDigests(projectId, formId, formPageDigests);
            }
            return result;
        }

        public bool PageDigestsExists(string projectId, string formId)
        {
            return KeyExists(new Guid(projectId), ComposeFormPageDigestsKey(formId), NoTimeout).Result;
        }

        public PageDigest[] GetPageDigests(string projectId, string formId)
        {
            PageDigest[] pageDigest = null;
            var json = Get(new Guid(projectId), ComposeFormPageDigestsKey(formId), NoTimeout).Result;
            if (json != null)
            {
                pageDigest = JsonConvert.DeserializeObject<PageDigest[]>(json);
            }
            return pageDigest;
        }

        public bool SetPageDigests(string projectId, string formId, PageDigest[] formPageDigests)
        {
            var json = JsonConvert.SerializeObject(formPageDigests, DontSerializeNulls);
            return Set(new Guid(projectId), ComposeFormPageDigestsKey(formId), json).Result;
        }

        public void ClearAllFormPageDigests(string projectId)
        {
            Delete(new Guid(projectId), FormPageDigestsKey);
        }
    }
}
