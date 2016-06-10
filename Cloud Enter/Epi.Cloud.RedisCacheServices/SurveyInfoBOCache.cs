using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : RedisCache, ISurveyInfoBOCache
    {
        private const string SurveyInfoBOPrefix = "surveyInfoBO_";

        private ConditionalWeakTable<string, string> _weakSurveyInfoBoMetadataJsonCache = new ConditionalWeakTable<string, string>();
        private static Dictionary<string, string> _surveyInfoBOStaticCache = new Dictionary<string, string>();

        public string GetSurveyInfoBoMetadata(string projectId)
        {
            string surveyInfoBOJson;
            if (!_weakSurveyInfoBoMetadataJsonCache.TryGetValue(projectId, out surveyInfoBOJson) 
                && !_surveyInfoBOStaticCache.TryGetValue(projectId, out surveyInfoBOJson))
            {
                surveyInfoBOJson = Get(SurveyInfoBOPrefix, projectId).Result;
            }
            return surveyInfoBOJson;
        }

        public bool SetSurveyInfoBoMetadata(string projectId, string surveyInfoBOJson)
        {
            var isSuccessful = Set(SurveyInfoBOPrefix, projectId, surveyInfoBOJson).Result;
            _weakSurveyInfoBoMetadataJsonCache.Add(projectId, surveyInfoBOJson);
            _surveyInfoBOStaticCache[projectId] = surveyInfoBOJson;
            return isSuccessful;
        }
    }
}
