using System;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache
    {
        public void ClearAllCache(Guid projectId)
        {
            DeleteAllKeys(projectId, null);
        }
    }
}
