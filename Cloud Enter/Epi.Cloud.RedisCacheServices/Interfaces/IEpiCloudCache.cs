using System;
using System.Collections.Generic;

namespace Epi.Cloud.CacheServices
{
    public interface IEpiCloudCache : IMetadataCache, IFormDigestCache, IPageDigestCache
    {
        void ClearAllCache(Guid projectId);
    }
}
