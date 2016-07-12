using System.Collections.Generic;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : RedisCache, ISurveyIdProjectIdMapCache
    {
        private const string SurveyIdProjectIdMapPrefix = "surveyIdProjectIdMap_";

        private Dictionary<string, string> _dictionarySurveyIdProjectIdMapCache = new Dictionary<string, string>();

        public string GetProjectIdFromSurveyId(string surveyId)
        {
            string projectId;
            lock (_dictionarySurveyIdProjectIdMapCache)
            {
                _dictionarySurveyIdProjectIdMapCache.TryGetValue(surveyId, out projectId);
            }
            if (projectId == null)
            {
                projectId = Get(SurveyIdProjectIdMapPrefix, surveyId).Result;
                if (projectId != null)
                {
                    lock (_dictionarySurveyIdProjectIdMapCache)
                    {
                        _dictionarySurveyIdProjectIdMapCache[surveyId] = projectId;
                    }
                }
            }
            return projectId;
        }

        public bool SetSurveyIdProjectIdMap(string surveyId, string projectId)
        {
            bool isSuccessful = true;
            lock (_dictionarySurveyIdProjectIdMapCache)
            {
                _dictionarySurveyIdProjectIdMapCache[surveyId] = projectId;
            }
            Set(SurveyIdProjectIdMapPrefix, surveyId, projectId, NoTimeout);
            return isSuccessful;
        }

        public void ClearSurveyIdMap(string surveyId)
        {
            lock (_dictionarySurveyIdProjectIdMapCache)
            {
                _dictionarySurveyIdProjectIdMapCache.Remove(surveyId);
            }
            Delete(SurveyIdProjectIdMapPrefix, surveyId);
        }

        public void ClearAllSurveyIdMap()
        {
            lock (_dictionarySurveyIdProjectIdMapCache)
            {
                _dictionarySurveyIdProjectIdMapCache.Clear();
            }
            DeleteAllKeys(SurveyIdProjectIdMapPrefix);
        }
    }
}
