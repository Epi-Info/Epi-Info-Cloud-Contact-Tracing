using System;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache
    {
        public void ClearAllCache(Guid projectId)
        {
            lock (MetadataAccessor.StaticCache.Gate)
            {
                DeleteAllKeys(projectId, null);
                MetadataAccessor.StaticCache.ClearAll();
            }
        }
    }
}
