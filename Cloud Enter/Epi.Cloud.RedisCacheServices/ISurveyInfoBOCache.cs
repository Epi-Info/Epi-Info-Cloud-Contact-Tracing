namespace Epi.Cloud.CacheServices
{
    public interface ISurveyInfoBOCache
    {
        string GetSurveyInfoBoMetadata(string projectId);
        bool SetSurveyInfoBoMetadata(string projectId, string surveyInfoBOJson);
        bool SurveyInfoBoMetadataExists(string projectId);
        void ClearAllSurveyInfoBoMetadataFromCache();
    }
}
