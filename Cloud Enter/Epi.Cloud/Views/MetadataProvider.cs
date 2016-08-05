using System.Collections.Generic;
using System.Data;
using System.Linq;
using Epi.Cloud.MetadataServices;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.CacheServices;
using System.Threading.Tasks;

namespace Epi.Cloud.FormMetadataServices
{
    public class MetadataProvider : IMetadataProvider
    {
        private readonly Cloud.CacheServices.IEpiCloudCache _epiCloudCache;
        private readonly IProjectMetadataProvider _projectMetadataProvider;

        public MetadataProvider(Cloud.CacheServices.IEpiCloudCache epiCloudCache,
            IProjectMetadataProvider projectMetadataProvider)
        {
            _epiCloudCache = epiCloudCache;
            _projectMetadataProvider = projectMetadataProvider;
        }

        public async Task<IEnumerable<FieldAttributes>> GetMetadataAsync(string formId, int pageNumber)
        {
            Template projectTemplateMetadata;

            ISurveyInfoBOCache surveyInfoBOCache = _epiCloudCache;
            var surveyInfoBO = surveyInfoBOCache.GetSurveyInfoBoMetadata(formId);
            if (surveyInfoBO != null)
            {
                projectTemplateMetadata = surveyInfoBO.ProjectTemplateMetadata;
            }
            else
            {
                var projectId = _epiCloudCache.GetProjectIdFromSurveyId(formId);
                projectTemplateMetadata = await _projectMetadataProvider.GetProjectMetadataAsync(projectId);
            }
            IEnumerable<FieldAttributes> results = GetFieldMedatadata(projectTemplateMetadata, formId, pageNumber);

            return results;
        }

        public IEnumerable<FieldAttributes> GetFieldMedatadata(Template projectTemplateMetadata, string formId, int pageNumber)
        {
            IEnumerable<FieldAttributes> fieldAttributesArray = null;
            var projectId = projectTemplateMetadata.Project.Id;
            if (_epiCloudCache != null)
            {
                fieldAttributesArray = _epiCloudCache.GetPageFieldAttributes(projectId, formId, pageNumber);
            }
            if (fieldAttributesArray == null)
            {
                var pagePosition = pageNumber - 1;
                var view = projectTemplateMetadata.Project.Views.Where(v => v.EWEFormId == formId).SingleOrDefault();
                var checkcode = view.CheckCode;
                var page = view.Pages
                .Where(p => p.Position == pagePosition).Single();
                fieldAttributesArray = FieldAttributes.MapFieldMetadataToFieldAttributes(page, projectTemplateMetadata.SourceTables, checkcode);
                if (_epiCloudCache != null)
                {
                    _epiCloudCache.SetPageFieldAttributes(fieldAttributesArray.ToArray(), projectId, formId, pageNumber);
                }
            }
            return fieldAttributesArray;
        }
    }
}

