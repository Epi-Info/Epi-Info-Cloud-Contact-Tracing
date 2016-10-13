using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.CacheServices
{
    public interface ISurveyInfoBOCache
    {
        SurveyInfoBO GetSurveyInfoBoMetadata(string projectId);
        bool SetSurveyInfoBoMetadata(string projectId, SurveyInfoBO surveyInfoBO);
        bool SurveyInfoBoMetadataExists(string projectId);
        void ClearAllSurveyInfoBoMetadataFromCache();
    }
}
