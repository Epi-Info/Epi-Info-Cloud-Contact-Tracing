using System.Collections.Generic;
using System.Data;
using System.Linq;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.CacheServices;
using System.Threading.Tasks;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using System;

namespace Epi.Cloud.MetadataServices
{
    public class FieldAttributesProvider : IFieldAttributesProvider
    {
        private readonly IEpiCloudCache _epiCloudCache;
        private readonly IProjectMetadataProvider _projectMetadataProvider;

        public FieldAttributesProvider(IEpiCloudCache epiCloudCache,
            IProjectMetadataProvider projectMetadataProvider)
        {
            _epiCloudCache = epiCloudCache;
            _projectMetadataProvider = projectMetadataProvider;
        }

        public Task<IDictionary<int, IDictionary<string, FieldAttributes>>> GetFormPageFieldAttributesForAllPagesAsync(string formId)
        {
            IDictionary<int, IDictionary<string, FieldAttributes>> formFieldAttributes = new Dictionary<int, IDictionary<string, FieldAttributes>>();

            //var pageDigests = _projectMetadataProvider.GetPageDigestsAsync().Result;
            //foreach (var pageDigest in pageDigests)
            //{
            //    var pageMetadata = _projectMetadataProvider.GetPageMetadataAsync(pageDigest.FormId, pageDigest.PageId);
            //    formFieldAttributes.Add(pageDigest.PageId, pageDigest.)
            //}
            return Task.FromResult(formFieldAttributes);
        }

        public async Task<IDictionary<string, FieldAttributes>> GetPageFieldAttributesAsync(string formId, int pageNumber)
        {
            var projectId = _projectMetadataProvider.ProjectId;

            IDictionary<string, FieldAttributes> results = projectId != null
                ? _epiCloudCache.GetPageFieldAttributes(projectId, formId, pageNumber).ToDictionary(f => f.Name.ToLower(), f => f)
                : null;
            //if (results == null)
            //{
            //    Template projectTemplateMetadata = await _projectMetadataProvider.GetProjectMetadataWithPageByPageNumberAsync(formId, pageNumber);
            //    var pagePosition = pageNumber - 1;
            //    var view = projectTemplateMetadata.Project.Views.Where(v => v.FormId == formId).Single();
            //    var page = view.Pages
            //    .Where(p => p.Position == pagePosition).Single();
            //    var fieldAttributesArray = FieldAttributes.MapFieldMetadataToFieldAttributes(page, projectTemplateMetadata.SourceTables);
            //    if (_epiCloudCache != null)
            //    {
            //        _epiCloudCache.SetPageFieldAttributes(fieldAttributesArray.ToArray(), projectId, formId, pageNumber);
            //    }
            //}
            return results;
        }
    }
}

