using System;
using System.Collections.Generic;
using Epi.Cloud.Common.Metadata;
using Epi.Common.Constants;
using Newtonsoft.Json;

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
