namespace Epi.Cloud.CacheServices
{
    public interface ISurveyIdProjectIdMapCache
    {
        string GetProjectIdFromSurveyId(string surveyId);
        bool SetSurveyIdProjectIdMap(string surveyId, string projectId);
        void ClearSurveyIdMap(string surveyId);
        void ClearAllSurveyIdMap();
    }
}
