using System;

namespace Epi.Cloud.CacheServices
{
    public interface IEpiCloudCache : IMetadataCache, IFormDigestCache, IPageDigestCache, IFormSettingCache
    {
        void ClearAllCache(Guid projectId);
    }
}
