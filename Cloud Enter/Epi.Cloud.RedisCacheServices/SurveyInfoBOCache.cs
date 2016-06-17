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

        private ConditionalWeakTable<string, SurveyInfoBO> _weakSurveyInfoBoMetadataJsonCache = new ConditionalWeakTable<string, SurveyInfoBO>();

        public bool SurveyInfoBoMetadataExists(string projectId)
        {
            bool keyExists = true;
            SurveyInfoBO surveyInfoBO;
            if (!_weakSurveyInfoBoMetadataJsonCache.TryGetValue(projectId, out surveyInfoBO))
            {
                keyExists = KeyExists(SurveyInfoBOPrefix, projectId).Result;
            }
            return keyExists;
        }

        public SurveyInfoBO GetSurveyInfoBoMetadata(string projectId)
        {
            SurveyInfoBO surveyInfoBO;
            if (!_weakSurveyInfoBoMetadataJsonCache.TryGetValue(projectId, out surveyInfoBO))
            {
                string surveyInfoBOJson = Get(SurveyInfoBOPrefix, projectId).Result;
                if (surveyInfoBOJson != null)
                {
                    surveyInfoBO = JsonConvert.DeserializeObject<SurveyInfoBO>(surveyInfoBOJson);

                    _weakSurveyInfoBoMetadataJsonCache.Add(projectId, surveyInfoBO);
                }
            }
            return surveyInfoBO;
        }

        public bool SetSurveyInfoBoMetadata(string projectId, SurveyInfoBO surveyInfoBO)
        {
            string surveyInfoBOJson = JsonConvert.SerializeObject(surveyInfoBO);
            var isSuccessful = Set(SurveyInfoBOPrefix, projectId, surveyInfoBOJson).Result;
            _weakSurveyInfoBoMetadataJsonCache.Add(projectId, surveyInfoBO);
            if (!ProjectTemplateMetadataExists(projectId))
            {
                SetProjectTemplateMetadata(surveyInfoBO.ProjectTemplateMetadata);
            }
            return isSuccessful;
        }

        public void ClearAllSurveyInfoBoMetadataFromCache()
        {
            DeleteAllKeys(SurveyInfoBOPrefix, key => _weakSurveyInfoBoMetadataJsonCache.Remove(key));
        }
    }
}
