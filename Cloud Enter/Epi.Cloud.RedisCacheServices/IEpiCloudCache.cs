namespace Epi.Cloud.CacheServices
{
    public interface IEpiCloudCache : IMetadataCache, ISurveyInfoBOCache, ISurveyIdProjectIdMapCache
    {
        void ClearAllCache();
    }
}
