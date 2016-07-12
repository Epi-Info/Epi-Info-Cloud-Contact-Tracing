using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Epi.Web.Enter.Common.BusinessObject;
using Newtonsoft.Json;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : RedisCache, ISurveyInfoBOCache
    {
        private const string SurveyInfoBOPrefix = "surveyInfoBO_";

        private static Dictionary<string, SurveyInfoBO> _dictionarySurveyInfoBoMetadataCache = new Dictionary<string, SurveyInfoBO>();

        public bool SurveyInfoBoMetadataExists(string projectId)
        {
            lock (_dictionarySurveyInfoBoMetadataCache)
            {
                SurveyInfoBO surveyInfoBO;
                return _dictionarySurveyInfoBoMetadataCache.TryGetValue(projectId, out surveyInfoBO);
            }
        }

        public SurveyInfoBO GetSurveyInfoBoMetadata(string surveyId)
        {
            SurveyInfoBO surveyInfoBO;
            _dictionarySurveyInfoBoMetadataCache.TryGetValue(surveyId, out surveyInfoBO);
            return surveyInfoBO;
        }

        public bool SetSurveyInfoBoMetadata(string surveyId, SurveyInfoBO surveyInfoBO)
        {
            bool isSuccessful = true;
            _dictionarySurveyInfoBoMetadataCache[surveyId] = surveyInfoBO;
            if (surveyInfoBO.ProjectTemplateMetadata != null)
            {
                var projectId = surveyInfoBO.ProjectTemplateMetadata.Project.Id;
                if (!FullProjectTemplateMetadataExists(projectId))
                {
                    isSuccessful = SetProjectTemplateMetadata(surveyInfoBO.ProjectTemplateMetadata);
                }
            }
            return isSuccessful;
        }

        public void ClearAllSurveyInfoBoMetadataFromCache()
        {
            _dictionarySurveyInfoBoMetadataCache.Clear();
        }
    }
}
