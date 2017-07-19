using System;
using Epi.FormMetadata.DataStructures;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : IEpiCloudCache, IFormDigestCache
    {
        private const string FormDigestsKey = "FormDigests";

        public bool FormDigestsExists(Guid projectId)
        {
            return KeyExists(projectId, FormDigestsKey, NoTimeout).Result;
        }

        public FormDigest[] GetFormDigests(Guid projectId)
        {
            FormDigest[] formDigests = null;
            var json = Get(projectId, FormDigestsKey, NoTimeout).Result;
            if (json != null)
            {
                formDigests = JsonConvert.DeserializeObject<FormDigest[]>(json);
            }
            return formDigests;
        }

        public bool SetFormDigests(Guid projectId, FormDigest[] formDigests)
        {
            var json = JsonConvert.SerializeObject(formDigests, DontSerializeNulls);
            return Set(projectId, FormDigestsKey, json, NoTimeout).Result;
        }

        public void ClearFormDigests(Guid projectId)
        {
            Delete(projectId, FormDigestsKey);
        }
    }
}
