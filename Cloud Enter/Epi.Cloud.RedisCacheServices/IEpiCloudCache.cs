namespace Epi.Cloud.CacheServices
{
    public interface IEpiCloudCache : IMetadataCache, IProjectDigestCache, ISurveyInfoBOCache, ISurveyIdProjectIdMapCache
    {
        void ClearAllCache();
    }
}
