namespace Epi.Cloud.CacheServices
{
    public interface IEpiCloudCache : IMetadataCache, IFormDigestCache, IPageDigestCache
    {
        void ClearAllCache(string projectId);
    }
}
