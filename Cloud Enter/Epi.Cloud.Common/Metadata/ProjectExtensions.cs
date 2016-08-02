using System.Collections.Generic;
using System.Linq;

namespace Epi.Cloud.Common.Metadata
{
    public static class ProjectExtensions
    {
        public static ProjectDigest[] FilterDigest(this ProjectDigest[] projectDigest, string[] fieldNames)
        {
            fieldNames = fieldNames.Select(n => n.ToLower()).ToArray();
            List<ProjectDigest> filteredDigest = new List<ProjectDigest>();
            if (projectDigest != null)
            {
                foreach (var digest in projectDigest)
                {
                    var filteredFields = new List<AbridgedFieldInfo>();
                    digest.Fields.Where(f => { if (fieldNames.Contains(f.Name.ToLower())) { filteredFields.Add(f); return true; } else return false; });
                    if (filteredFields.Count > 0)
                    {
                        filteredDigest.Add(new ProjectDigest(digest.FormName, digest.FormId, digest.ViewId, digest.IsRelatedView, digest.PageId, digest.Position, filteredFields.ToArray()));
                    }
                }
            }
            return filteredDigest.ToArray();
        }

        public static int PageIdFromPageNumber(this Template projectTemplateMetadata, string formId, int pageNumber)
        {
            int pageId = 0;
            var view = projectTemplateMetadata.Project.Views.Where(v => v.EWEFormId == formId).SingleOrDefault();
            if (view != null)
            {
                var viewId = view.ViewId;
                var pagePosition = pageNumber - 1;
                var digestElement = projectTemplateMetadata.Project.Digest.Where(p => p.Position == pagePosition).SingleOrDefault();
                if (digestElement != null)
                {
                    pageId = digestElement.PageId;
                }
            }
            return pageId;
        }
    }
}
