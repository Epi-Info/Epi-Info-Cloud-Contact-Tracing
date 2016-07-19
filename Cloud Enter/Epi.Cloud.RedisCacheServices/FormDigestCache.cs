using System;
using Epi.Cloud.Common.Metadata;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : IEpiCloudCache, IFormDigestCache
    {
        private const string FormDigestsKey = "FormDigests";

        public bool FormDigestsExists(string projectId)
        {
            return KeyExists(new Guid(projectId), FormDigestsKey, NoTimeout).Result;
        }

        public FormDigest[] GetFormDigests(string projectId)
        {
            FormDigest[] formDigests = null;
            var json = Get(new Guid(projectId), FormDigestsKey, NoTimeout).Result;
            if (json != null)
            {
                formDigests = JsonConvert.DeserializeObject<FormDigest[]>(json);
            }
            return formDigests;
        }

        public bool SetFormDigests(string projectId, FormDigest[] formDigests)
        {
            var json = JsonConvert.SerializeObject(formDigests, DontSerializeNulls);
            return Set(new Guid(projectId), FormDigestsKey, json).Result;
        }
        public void ClearFormDigests(string projectId)
        {
            Delete(new Guid(projectId), FormDigestsKey);
        }
    }
}
