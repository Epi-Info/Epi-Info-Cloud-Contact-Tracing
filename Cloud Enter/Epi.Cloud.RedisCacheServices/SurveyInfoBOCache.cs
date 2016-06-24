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

        private ConditionalWeakTable<string, SurveyInfoBO> _weakSurveyInfoBoMetadataCache = new ConditionalWeakTable<string, SurveyInfoBO>();
        private Dictionary<string, SurveyInfoBO> _dictionarySurveyInfoBoMetadataCache = new Dictionary<string, SurveyInfoBO>();

        public bool SurveyInfoBoMetadataExists(string projectId)
        {
            bool keyExists = true;
            SurveyInfoBO surveyInfoBO;
            if (!_weakSurveyInfoBoMetadataCache.TryGetValue(projectId, out surveyInfoBO))
            {
                keyExists = KeyExists(SurveyInfoBOPrefix, projectId).Result;
            }
            return keyExists;
        }

        public SurveyInfoBO GetSurveyInfoBoMetadata(string surveyId)
        {
            SurveyInfoBO surveyInfoBO;
            //if (!_weakSurveyInfoBoMetadataCache.TryGetValue(projectId, out surveyInfoBO))
            //{
            //    string surveyInfoBOJson = Get(SurveyInfoBOPrefix, projectId).Result;
            //    if (surveyInfoBOJson != null)
            //    {
            //        surveyInfoBO = JsonConvert.DeserializeObject<SurveyInfoBO>(surveyInfoBOJson);

            //        _weakSurveyInfoBoMetadataCache.Add(projectId, surveyInfoBO);
            //    }
            //}
            _dictionarySurveyInfoBoMetadataCache.TryGetValue(surveyId, out surveyInfoBO);
            if (surveyInfoBO != null && surveyInfoBO.ProjectTemplateMetadata != null)
            {
                var projectId = surveyInfoBO.ProjectTemplateMetadata.Project.Id;
                if (GetProjectIdFromSurveyId(surveyId) != projectId)
                {
                    SetSurveyIdProjectIdMap(surveyId, projectId);
                }
            }
            return surveyInfoBO;
        }

        public bool SetSurveyInfoBoMetadata(string surveyId, SurveyInfoBO surveyInfoBO)
        {
            bool isSuccessful = true;
            //string surveyInfoBOJson = JsonConvert.SerializeObject(surveyInfoBO);
            //var isSuccessful = Set(SurveyInfoBOPrefix, surveyId, surveyInfoBOJson).Result;
            //_weakSurveyInfoBoMetadataCache.Add(surveyId, surveyInfoBO);
            _dictionarySurveyInfoBoMetadataCache[surveyId] = surveyInfoBO;
            if (surveyInfoBO.ProjectTemplateMetadata != null)
            {
                var projectId = surveyInfoBO.ProjectTemplateMetadata.Project.Id;
                if (GetProjectIdFromSurveyId(surveyId) != projectId)
                {
                    SetSurveyIdProjectIdMap(surveyId, projectId);
                }
                if (!ProjectTemplateMetadataExists(projectId))
                {
                    isSuccessful = SetProjectTemplateMetadata(surveyInfoBO.ProjectTemplateMetadata);
                }
            }
            return isSuccessful;
        }

        public void ClearAllSurveyInfoBoMetadataFromCache()
        {
            //DeleteAllKeys(SurveyInfoBOPrefix, key => _weakSurveyInfoBoMetadataCache.Remove(key));
            _dictionarySurveyInfoBoMetadataCache.Clear();
        }
    }
}
