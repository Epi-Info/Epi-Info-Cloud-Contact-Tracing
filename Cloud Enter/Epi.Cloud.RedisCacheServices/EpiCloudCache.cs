using System;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache
    {
        public void ClearAllCache(string projectId)
        {
            DeleteAllKeys(new Guid(projectId), null);
        }
    }
}
